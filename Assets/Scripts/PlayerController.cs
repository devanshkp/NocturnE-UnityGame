using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Timers;
using TMPro;

// ORIGINAL COLLIDER HEIGHT = 1.8
// ORIGINAL COLLIDER CENTER = (0,0.075,0)

public class PlayerController : MonoBehaviour
{
    private static PlayerController instance;

    private bool GodMode = false;

    private MoveAroundObject cameraController;
    private Animator animator;

    public float score = 0;

    [Header("References")]
    public CharacterController controller;
    public Transform cameraTarget;
    public Camera playerCamera;
    public GameObject playerCanvas;
    public TMP_Text scoreText;
    public TMP_Text moneyText;

    [Header("Health Settings")]
    private HealthManager healthManager;
    private bool isBurning = false;
    private Coroutine fireCoroutine;
    public bool isDead = false;

    [Header("Movement Settings")]
    public bool isSlowed = false;
    private float speedMultiplier = 1f;
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float accelerationRate = 8f;
    public float decelerationRate = 5f;
    public float movementRotSpeed = 10;
    public float cameraBasedRotSpeed = 720f;  // Speed of player rotation (degrees per second)
    private float playerSpeed = 0f;
    private bool isRunning = false;
    private Vector3 horizontalVelocity;
    private Vector3 direction = Vector3.zero;
    private Coroutine speedCoroutine;

    [Header("FOV Settings")]
    public float normalFOV = 60f;
    public float sprintFOV = 75f;
    public float fovTransitionSpeed = 12.5f;

    [Header("Stamina Settings")]
    public Slider staminaSlider;
    public float maxStamina = 100f;
    private float currentStamina;
    public float staminaRegenRate = 9f;
    public float staminaDepletionRate = 10f; // Depletion rate when running
    public float jumpStaminaCost = 15f;
    public float staminaRegenCooldown = 2f; 
    private float staminaCooldownTimer = 0f;  // Timer to track cooldown before stamina starts regenerating

    [Header("Combat Settings")]
    public bool isSlashing = false;
    public bool stationarySlash = false;

    [Header("Auto-Target Settings")]
    public bool autoTargeting = false;
    public float autoTargetRange = 3f;
    public float autoTargetAngleThreshold = 120f; // Angle threshold to check if the player is facing the enemy

    [Header ("Target Settings")]
    public LayerMask targetLayer;
    public LayerMask obstructionLayers;
    public float targetRange = 15f;
    public float unlockDistance = 25f;
    private Transform lockedEnemy;  // The enemy the player is locked onto
    public float cameraLockSpeed = 5f;
    private EnemyUIManager enemyUIManager;

    [Header("Roll Settings")]
    public AnimationCurve rollCurve;
    public float rollCooldown = 1.5f; 
    private float rollCooldownTimer = 0f;  // Timer to track time since last roll
    private bool isRolling = false;  // Is the player currently rolling?
    private float rollTimer;

    [Header("Jump")]
    private int jumpsPerformed = 0;
    private bool isGrounded;  // Check if the player is on the ground
    private bool jumpRequested = false;  // Track if the jump is requested

    [Header("Gravity Settings")]
    public float gravity = -9.81f;
    public float jumpHeight = 1.25f; // Height of jump
    private float verticalVelocity = 0f;  // Track vertical speed for gravity

    [Header("Collider Settings")]
    private Vector3 originalColliderCenter;
    private float originalColliderHeight;

    [Header("Shop Settings")]
    public int money = 0;
    public bool isShopOpen = false;
    public bool doubleJump = false;
    public bool speedBuff = false;
    public bool damageBuff = false;

    void Awake()
    {
        // Check if another instance of the player already exists
        if (instance != null && instance != this){
            // If another player exists, destroy this one
            Destroy(gameObject);
            return;
        }

        // Otherwise, this is the instance to persist across scenes
        instance = this;
        DontDestroyOnLoad(gameObject);
        AssignCameraToCurrentPlayer();

    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        healthManager = GetComponentInChildren<HealthManager>();
        if (playerCamera == null){
            playerCamera = Camera.main;
        }
        if (playerCanvas == null){
            playerCanvas = GetComponentInChildren<Canvas>().gameObject;
        }
        cameraController = playerCamera.GetComponent<MoveAroundObject>();

        // Save original collider size
        originalColliderCenter = controller.center;
        originalColliderHeight = controller.height;

        Keyframe rollLastFrame = rollCurve[rollCurve.length - 1];
        rollTimer = rollLastFrame.time;

        // Initialise variables
        rollCooldownTimer = rollTimer;
        currentStamina = maxStamina;
        staminaSlider.value = currentStamina;
    }

    void Update()
    {
        if (isDead || isShopOpen){
            if (isShopOpen){
                animator.SetFloat("velocity", 0f, 0.1f, Time.deltaTime);
            }
            return;
        } 
        RecordInputs();
        UpdateVelocity();
        UpdateFOV();
        UpdateStamina();
        HandleRotation();
        if (cameraController.LockedOn()){
            CheckUnlockConditions();
        }
        isGrounded = controller.isGrounded;
        rollCooldownTimer += Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (isDead || isShopOpen) return;
        if (!isRolling && !stationarySlash) HandleMovement();  // Allow movement if not rolling or attacking while stationary
        HandleVerticalMovement();
    }

    void AssignCameraToCurrentPlayer()
    {
        // Get the current main camera
        if (playerCamera == null){
            playerCamera = Camera.main;
        }

        // If the camera controller exists, assign it to follow this player
        if (playerCamera != null){
            cameraController = playerCamera.GetComponent<MoveAroundObject>();
            if (cameraController != null){
                cameraController.playerObj = this.gameObject;
                cameraController.target = cameraTarget;
                cameraController.player = this;
            }
        }
    }

    // Handles player movement based on user input
    void RecordInputs()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right Arrow keys
        float verticalInput = Input.GetAxisRaw("Vertical");     // W/S or Up/Down Arrow keys

        isRunning = Input.GetKey(KeyCode.LeftShift) && currentStamina > 0; // Can only run if stamina is available

        // Speed buff multiplier
        speedMultiplier = speedBuff ? 1.25f : 1f; // 1.5x speed when speed buff is active

        float targetSpeed = (isRunning ? runSpeed : walkSpeed) * speedMultiplier;

        // Deplete stamina while running
        if (isRunning) DepleteStamina();

        // Get the camera's forward and right vectors
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;

        // Ignore the Y component
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        // Combine the input with the camera's direction to get the movement direction
        direction = (cameraForward * verticalInput + cameraRight * horizontalInput).normalized;

        if (direction.magnitude > 0) 
            playerSpeed = Mathf.MoveTowards(playerSpeed, targetSpeed, accelerationRate * Time.deltaTime);
        else 
            playerSpeed = Mathf.MoveTowards(playerSpeed, 0, decelerationRate * Time.deltaTime);

        if (Input.GetMouseButtonDown(2))
            SetTarget();

        // Active GodMode for presentation/testing purposes
        if (Input.GetKeyDown(KeyCode.P) && healthManager.GetHealth() <= 200){
            if (!GodMode){
                GodMode = true;
                healthManager.SetMaxHealth(10000);
            }
            else{
                GodMode = false;
                healthManager.SetMaxHealth(200);
            }
            
        }

        if (!isRolling && !isSlashing){
            // Roll mechanic
            if (Input.GetKeyDown(KeyCode.Space) && rollCooldownTimer >= rollCooldown && direction.magnitude != 0){
                StartCoroutine(Roll());
            }
            // Jump mechanic
            if (Input.GetKeyDown(KeyCode.F) && (isGrounded || (doubleJump && !jumpRequested)) && currentStamina >= jumpStaminaCost){
                jumpRequested = true;
                if (isGrounded && !GodMode){
                    // First jump, reduce stamina
                    currentStamina -= jumpStaminaCost;
                    staminaSlider.value = currentStamina;
                    staminaCooldownTimer = 0f;
                }
            }
            if (Input.GetKeyDown(KeyCode.Mouse0))
                PerformSlash();
        }
    }

    void DepleteStamina()
    {
        if (GodMode) return;
        currentStamina -= staminaDepletionRate * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        staminaSlider.value = currentStamina;
        staminaCooldownTimer = 0f;
    }

    void UpdateStamina()
    {
        staminaCooldownTimer += Time.deltaTime;
        if (staminaCooldownTimer >= staminaRegenCooldown && currentStamina < maxStamina && (playerSpeed/speedMultiplier <= walkSpeed) && isGrounded)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina); // Ensure it doesn't exceed max stamina
            staminaSlider.value = currentStamina;
        }
    }

    void UpdateVelocity()
    {
        // Get horizontal velocity for animator
        horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
        animator.SetFloat("velocity", horizontalVelocity.magnitude, 0.1f, Time.deltaTime);
    }

    void UpdateFOV()
    {
        if (isRolling) 
            return;
        if (isRunning)
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, fovTransitionSpeed * Time.deltaTime);
        else
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, normalFOV, fovTransitionSpeed * Time.deltaTime);
    }

    void HandleMovement()
    {
        controller.Move((direction * playerSpeed + Vector3.up * verticalVelocity) * Time.fixedDeltaTime);
    }

    // Handles player rotation based on movement direction
    void HandleRotation()
    {        
        if (direction.magnitude == 0 || stationarySlash) return;
        if (autoTargeting && lockedEnemy != null){
            Vector3 directionToEnemy = (lockedEnemy.position - transform.position).normalized;
            directionToEnemy.y = 0; // Ignore vertical rotation
            RotateTowardsEnemy(directionToEnemy);
        }
        else{
            float rotSpeed = movementRotSpeed;
            if (isRolling) rotSpeed *= 0.5f;
            Quaternion desiredRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotSpeed * Time.deltaTime);
        }
    }

    // Apply gravity to the player
    void HandleVerticalMovement()
    {
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;  // Small downward velocity to keep the player grounded
            jumpsPerformed = 0;  // Reset jumps when grounded
        }

        // Apply gravity
        verticalVelocity += gravity * Time.fixedDeltaTime;

        // Jump logic
        if (jumpRequested)
        {
            if (isGrounded)
            {
                // First jump
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                animator.SetTrigger("JumpTrigger");  // Trigger jump animation
                jumpsPerformed = 1;  // First jump performed
            }
            else if (jumpsPerformed == 1 && doubleJump)
            {
                // Double jump
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                animator.SetTrigger("DoubleJumpTrigger");  // Trigger double jump animation
                jumpsPerformed = 2;  // Double jump performed
            }

            jumpRequested = false;  // Reset jump request after performing the jump
        }
    }


    IEnumerator Roll()
    {
        animator.SetBool("isRolling", true);
        isRolling = true;
        rollCooldownTimer = 0f;  // Reset cooldown timer
        controller.height = originalColliderHeight * 0.5f;  // Halve the collider height
        controller.center = new Vector3(0, -0.375f, 0); // Lower collider
        float rollSpeedMultiplier = (playerSpeed <= walkSpeed) ? 0.7f : 1.0f;
        float timer = 0;
        while (timer < rollTimer && !isDead){
            float speed = rollCurve.Evaluate(timer) * rollSpeedMultiplier;
            Vector3 dir = (transform.forward * speed + (Vector3.up * verticalVelocity));
            controller.Move(dir * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        controller.height = originalColliderHeight;
        controller.center = originalColliderCenter;
        isRolling = false;
        animator.SetBool("isRolling", false);
    }

    void AutoTargetAndRotate()
    {
        if (lockedEnemy == null || autoTargeting) return;

        Vector3 directionToEnemy = (lockedEnemy.position - transform.position).normalized;
        directionToEnemy.y = 0;

        float angleToEnemy = Vector3.Angle(transform.forward, directionToEnemy);
        float distanceToEnemy = Vector3.Distance(transform.position, lockedEnemy.position);

        // If the enemy is within the player's field of view and close enough, rotate towards enemy
        if (angleToEnemy <= autoTargetAngleThreshold && distanceToEnemy <= autoTargetRange)
        {
            autoTargeting = true;
            RotateTowardsEnemy(directionToEnemy);
        }
    }

    void RotateTowardsEnemy(Vector3 directionToEnemy)
    {
        Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * cameraBasedRotSpeed);
    }


    void PerformSlash()
    {
        if (isSlashing) return;
        AutoTargetAndRotate();
        
        if (horizontalVelocity.magnitude < 0.01){
            stationarySlash = true;
        }

        isSlashing = true;
        animator.SetTrigger("Slash");
    }

    public void ResetSlash()
    {
        autoTargeting = false; 
        isSlashing = false; 
        stationarySlash = false;
    }

    void SetTarget()
    {
        if (lockedEnemy != null){
            UnlockTarget();
            return;
        } 

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, targetRange, targetLayer);
        
        if (hitColliders.Length > 0){
            Transform closestTarget = null;
            float closestDistance = Mathf.Infinity;

            // Loop through all colliders and find the closest enemy
            foreach (Collider hitCollider in hitColliders){
                if (hitCollider.CompareTag("Enemy")){
                    float distanceToTarget = Vector3.Distance(transform.position, hitCollider.transform.position);
                    if (distanceToTarget < closestDistance && !TargetObstructed(hitCollider.transform)){
                        closestDistance = distanceToTarget;
                        closestTarget = hitCollider.transform;
                    }
                }
            }

            if (closestTarget != null){
                lockedEnemy = closestTarget;
                enemyUIManager = lockedEnemy.GetComponentInChildren<EnemyUIManager>();
                if (enemyUIManager != null){
                    enemyUIManager.EnableLockOnIcon();
                    enemyUIManager.EnableHealthBar();
                    cameraController.SetLockedTarget(enemyUIManager.GetLockOnIcon());
                    Debug.Log("Target Locked: " + lockedEnemy.name);
                }
                else{
                    lockedEnemy = null;
                }
            }
        }
    }

    void CheckUnlockConditions() 
    {
        // Check if the lockedEnemy is still valid (active in hierarchy or not destroyed)
        if (lockedEnemy == null || !lockedEnemy.gameObject.activeInHierarchy)
        {
            Debug.Log("enemy gone");
            UnlockTarget();
            return;
        }

        if (isRolling) return;

        // Check distance
        float distanceToEnemy = Vector3.Distance(transform.position, lockedEnemy.position);
        if (distanceToEnemy > unlockDistance || TargetObstructed(lockedEnemy))
            UnlockTarget();
    }

    bool TargetObstructed(Transform target)
    {
        RaycastHit hit;
        if(Physics.Linecast(transform.position + Vector3.up * 0.5f, target.position, out hit, obstructionLayers)){
            if(hit.collider.transform != target) return true;
        }
        return false;
    }


    void UnlockTarget() 
    {
        lockedEnemy = null;
        if (enemyUIManager != null) enemyUIManager.DisableLockOnIcon();
        cameraController.SetLockedTarget(lockedEnemy);
        Debug.Log("Target unlocked.");
    }

    public void AddMoneyAndScore(int moneyDelta, int scoreDelta)
    {
        money += moneyDelta;
        score += scoreDelta;
        UpdateUI();
    }

    public void UpdateUI()
    {
        moneyText.text = money.ToString("N0");
        scoreText.text = "Score: " + score.ToString("N0");
    }

    public void TakeDamage(int damage)
    {
        healthManager.UpdateHealth(-damage);
        if (healthManager.GetHealth() <= 0){
            Die();
        }
    }

    public void TakeFireDamage(FireBulletInfo firedamageInfo)
    {
        if (!isBurning){
            fireCoroutine = StartCoroutine(ApplyFireTick(firedamageInfo));
        }
        if (healthManager.GetHealth() <= 0){
            Die();
        }
    }

    private IEnumerator ApplyFireTick(FireBulletInfo firedamageInfo)
    {
        isBurning = true;
        float elapsedTime = 0f;

        while (elapsedTime < firedamageInfo.fireLifeTime && !isDead){
            healthManager.UpdateHealth(-firedamageInfo.fireTickDamage);
            //  delay tick damage by some amount of time
            yield return new WaitForSeconds(firedamageInfo.fireTickRate);
            elapsedTime += firedamageInfo.fireTickRate;
            //  check if the tick damage kills the player
            if(healthManager.GetHealth() <= 0){
                Die();
                break;
            }
        }
        
        isBurning = false;
    }

    public void TakeIceDamage(IceBulletInfo iceDamageInfo)
    {
        //  To prevent stacking, check if player is already slowed before applying speed changes and tick damage
        if (!isSlowed){
            speedCoroutine = StartCoroutine(iceModifier(iceDamageInfo));
        }

        if (healthManager.GetHealth() <= 0){
            Die();
        }
    }

    private IEnumerator iceModifier(IceBulletInfo iceDamageInfo)
    {
        isSlowed = true;
        float elaspedTime = 0f;
        float originalMultiplier = speedMultiplier;
        speedMultiplier *= iceDamageInfo.movementModifier;

        //  tick damage
        while (elaspedTime < iceDamageInfo.iceLifeTime){
            healthManager.UpdateHealth(-iceDamageInfo.iceTickDamage);
            // delay tick damage by some amount of time
            yield return new WaitForSeconds(iceDamageInfo.iceTickRate);
            elaspedTime += iceDamageInfo.iceTickRate;
            // check if the tick damage kills the player
            if(healthManager.GetHealth() <= 0){
                Die();
                break;
            }
        }
        speedMultiplier = originalMultiplier;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        animator.SetTrigger("Death");
        playerCanvas.SetActive(false);
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    // Disables player movement and input
    private void ResetPlayerControls()
    {
        // Disable movement, attacking, etc.
        isSlashing = false;
        isRolling = false;
        direction = Vector3.zero;
        playerSpeed = 0;
    }

    public void EndLevel()
    {
        SceneManager.LoadScene("Menu");  // Go back to menu
        Destroy(this.gameObject);
    }

    public float GetVelocity()
    {
        return horizontalVelocity.magnitude;
    }

    // GIZMOS

    void OnDrawGizmos()
    {
        // Gizmos.color = Color.red;
        // Gizmos.DrawWireSphere(transform.position, targetRange);
        // Gizmos.color = Color.green;
        // Gizmos.DrawWireSphere(transform.position, unlockDistance);

        DrawAutoTargetGizmo();

        // if (lockedEnemy != null)
        // {
        //     // Set gizmo color for locked target (yellow)
        //     Gizmos.color = Color.yellow;
            
        //     // Draw a line from the player to the locked enemy
        //     Gizmos.DrawLine(transform.position, lockedEnemy.position);

        //     // Draw a wire sphere at the locked enemy's position
        //     Gizmos.DrawWireSphere(lockedEnemy.position, 1f);  // The radius can be adjusted as needed
        // }
    }

    // Scene Load Logic

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (playerCamera == null){
            playerCamera = Camera.main;
            cameraController = playerCamera.GetComponent<MoveAroundObject>();
        }
        
        // Find the spawn point in the new scene
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawn");

        if (spawnPoint != null){
            // Move the player to the spawn point's position and rotation
            controller.GetComponent<Collider>().enabled = false;
            this.transform.position = spawnPoint.transform.position;
            this.transform.rotation = spawnPoint.transform.rotation;
            controller.GetComponent<Collider>().enabled = true;
        }
        else{
            Debug.LogWarning("No spawn point found in the new scene!");
        }
    }

    private void DrawAutoTargetGizmo()
    {
        // Set the gizmo color (optional)
        Gizmos.color = new Color(0f, 1f, 0f, 0.4f); // A transparent green color

        // Draw a wire sphere to represent the auto-target range
        Gizmos.DrawWireSphere(transform.position, autoTargetRange);

        // Draw the field of view (FOV) as two lines from the player to the edge of the angle threshold
        Vector3 forward = transform.forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -autoTargetAngleThreshold, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, autoTargetAngleThreshold, 0) * forward;

        // Draw lines representing the edges of the FOV
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * autoTargetRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * autoTargetRange);

        // Optionally, draw a cone to represent the FOV area
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f); // Lighter green for the cone
        Gizmos.DrawMesh(CreateConeMesh(autoTargetAngleThreshold, autoTargetRange), transform.position, transform.rotation);
    }

    // Create a cone mesh to visually represent the FOV
    private Mesh CreateConeMesh(float angle, float range)
    {
        Mesh mesh = new Mesh();

        int numSegments = 20; // Number of segments to approximate the cone
        float angleStep = (angle * 2) / numSegments;

        // Vertices array (1 center vertex + numSegments + 1 for the ring)
        Vector3[] vertices = new Vector3[numSegments + 2];
        vertices[0] = Vector3.zero; // The cone's origin (at the player's position)

        // Calculate vertices for the ring (edges of the FOV)
        for (int i = 0; i <= numSegments; i++)
        {
            float currentAngle = -angle + i * angleStep;
            Vector3 vertex = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * range;
            vertices[i + 1] = vertex;
        }

        // Triangles array (1 triangle per segment)
        int[] triangles = new int[numSegments * 3];
        for (int i = 0; i < numSegments; i++)
        {
            triangles[i * 3] = 0; // Center vertex
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}