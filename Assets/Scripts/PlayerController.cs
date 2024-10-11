using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Timers;

// ORIGINAL COLLIDER HEIGHT = 1.8
// ORIGINAL COLLIDER CENTER = (0,0.075,0)

public class PlayerController : MonoBehaviour
{
    private MoveAroundObject cameraController;
    private Animator animator;

    [Header("References")]
    public CharacterController controller;
    public Transform cameraTarget;
    public Camera playerCamera;

    [Header("Health Settings")]
    private HealthManager healthManager;
    private bool isBurning = false;
    private Coroutine fireCoroutine;

    [Header("Movement Settings")]
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
    private bool isSlowed = false;

    [Header("FOV Settings")]
    public float normalFOV = 60f;
    public float sprintFOV = 75f;
    public float fovTransitionSpeed = 12.5f;

    [Header("Stamina Settings")]
    public Slider staminaSlider;
    public float maxStamina = 100f;
    private float currentStamina;
    public float staminaRegenRate = 5f;
    public float staminaDepletionRate = 10f; // Depletion rate when running
    public float jumpStaminaCost = 15f;
    public float staminaRegenCooldown = 2f; 
    private float staminaCooldownTimer = 0f;  // Timer to track cooldown before stamina starts regenerating

    [Header("Combat Settings")]
    public float comboResetTime = 1.5f;
    private int comboCounter = 0;
    public bool isSlashing = false;
    public float comboTimer = 0f;  // Timer to track time since last slash
    public bool stationarySlash = false;

    [Header("Auto-Target Settings")]
    public bool isDashing = false;
    public float dashRange = 10f; // Range within which the player auto-targets and dashes toward the locked enemy
    public float dashSpeed = 10f; // Speed of dashing toward the enemy
    public float autoTargetAngleThreshold = 45f; // Angle threshold to check if the player is facing the enemy

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
    private bool isGrounded;  // Check if the player is on the ground
    private bool jumpRequested = false;  // Track if the jump is requested

    [Header("Gravity Settings")]
    public float gravity = -9.81f;
    public float jumpHeight = 1.25f; // Height of jump
    private float verticalVelocity = 0f;  // Track vertical speed for gravity

    [Header("Collider Settings")]
    private Vector3 originalColliderCenter;
    private float originalColliderHeight;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        healthManager = GetComponentInChildren<HealthManager>();
        if (playerCamera == null)
            playerCamera = Camera.main;
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
        RecordInputs();
        UpdateVelocity();
        UpdateFOV();
        UpdateStamina();
        HandleRotation();
        if (lockedEnemy != null){
            CheckUnlockConditions();
        }
        isGrounded = controller.isGrounded;
        rollCooldownTimer += Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (!isRolling && !stationarySlash && !isDashing) HandleMovement();  // Allow movement if not rolling or attacking while stationary
        HandleVerticalMovement();
        if (isSlashing){
            comboTimer += Time.fixedDeltaTime;
            if (comboTimer > comboResetTime)
                ResetCombo();
        }
    }

    // Handles player movement based on user input
    void RecordInputs()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right Arrow keys
        float verticalInput = Input.GetAxisRaw("Vertical");     // W/S or Up/Down Arrow keys

        isRunning = Input.GetKey(KeyCode.LeftShift) && currentStamina > 0; // Can only run if stamina is available
        float targetSpeed = isRunning ? runSpeed : walkSpeed;

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

        if (!isRolling && !isSlashing){
            // Roll mechanic
            if (Input.GetKeyDown(KeyCode.Space) && rollCooldownTimer >= rollCooldown && direction.magnitude != 0){
                StartCoroutine(Roll());
            }
            // Jump mechanic
            if (Input.GetKeyDown(KeyCode.F) && isGrounded && currentStamina >= jumpStaminaCost){
                jumpRequested = true;
                animator.SetBool("isJumping", true);
                currentStamina -= jumpStaminaCost;
                staminaSlider.value = currentStamina;
                staminaCooldownTimer = 0f;
            }
            if (Input.GetKeyDown(KeyCode.Mouse0))
                PerformSlash();
        }
    }

    void DepleteStamina()
    {
        currentStamina -= staminaDepletionRate * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        staminaSlider.value = currentStamina;
        staminaCooldownTimer = 0f;
    }

    void UpdateStamina()
    {
        staminaCooldownTimer += Time.deltaTime;
        if (staminaCooldownTimer >= staminaRegenCooldown && currentStamina < maxStamina && playerSpeed <= walkSpeed && isGrounded)
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
        if (direction.magnitude == 0 || isDashing) return;
        float rotSpeed = movementRotSpeed;
        if (isRolling) rotSpeed *= 0.5f;
        Quaternion desiredRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotSpeed * Time.deltaTime);
    }

    // Apply gravity to the player
    void HandleVerticalMovement()
    {
        if (isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;  // Small downward velocity to keep the player grounded

        // Apply gravity
        verticalVelocity += gravity * Time.fixedDeltaTime;        

        // Jump logic
        if (jumpRequested && isGrounded){
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpRequested = false; // Reset jump request
        }

        if (isGrounded && verticalVelocity <= 0.1f)
            animator.SetBool("isJumping", false);  // Stop jump animation
    }


    IEnumerator Roll()
    {
        animator.SetBool("isRolling", true);
        isRolling = true;
        rollCooldownTimer = 0f;  // Reset cooldown timer
        controller.height = originalColliderHeight * 0.5f;  // Halve the collider height
        controller.center = new Vector3(0, -0.375f, 0); // Lower collider
        float rollSpeedMultiplier = (playerSpeed == walkSpeed) ? 0.7f : 1.0f;
        float timer = 0;
        while (timer < rollTimer){
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

    void AutoTargetAndDash()
    {
        if (lockedEnemy == null || isDashing) return;

        float distanceToEnemy = Vector3.Distance(transform.position, lockedEnemy.position);

        // Check if the player is close enough and facing the enemy
        if (distanceToEnemy <= dashRange)
        {
            // Vector3 directionToEnemy = (lockedEnemy.position - transform.position).normalized;
            // float angleToEnemy = Vector3.Angle(transform.forward, directionToEnemy);

            // // If within angle threshold, dash toward the enemy
            // if (angleToEnemy <= autoTargetAngleThreshold)
            // {
                
            // }
            StartCoroutine(DashTowardsEnemy(lockedEnemy));
        }
    }

    IEnumerator DashTowardsEnemy(Transform enemy)
    {
        isDashing = true;

        // Get direction toward the enemy, but ignore the Y component to prevent rotation towards the ground
        Vector3 directionToEnemy = (enemy.position - transform.position).normalized;
        directionToEnemy.y = 0; // Ignore Y axis for horizontal dash

        // Calculate the distance to the enemy, maintaining a minimum distance (1.5 units) away
        float distanceToEnemy = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), 
                                                new Vector3(enemy.position.x, 0, enemy.position.z)) - 2f;

        while (distanceToEnemy > 2f)
        {
            // Recalculate the direction and distance each frame in case the enemy moves
            directionToEnemy = (enemy.position - transform.position).normalized;
            directionToEnemy.y = 0; // Keep player level during the dash

            // Calculate the distance between player and enemy in the XZ plane
            distanceToEnemy = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), 
                                            new Vector3(enemy.position.x, 0, enemy.position.z));

            if (distanceToEnemy <= 2f) // Stop when we're close enough
            {
                break;
            }

            // Apply movement using CharacterController.Move to respect collisions
            Vector3 dashMovement = directionToEnemy * dashSpeed * Time.deltaTime;
            controller.Move(dashMovement);

            yield return null;
        }

        // After dashing, make the player face the enemy
        Vector3 finalDirectionToEnemy = (enemy.position - transform.position).normalized;
        finalDirectionToEnemy.y = 0; // Keep rotation horizontal
        Quaternion lookRotation = Quaternion.LookRotation(finalDirectionToEnemy);
        transform.rotation = lookRotation; // Instantly face the enemy after dash ends

        isDashing = false;
    }


    void PerformSlash()
    {
        // AutoTargetAndDash();
        // If already slashing, only allow second slash to be triggered within the animation sequence
        if (isSlashing && comboCounter == 1){
            // Perform second slash
            comboCounter = 2;
            animator.SetInteger("comboIndex", comboCounter);
        }
        else if (!isSlashing){
            if (horizontalVelocity.magnitude < 0.01)
                stationarySlash = true;
            // Start first slash
            isSlashing = true;
            comboCounter = 1;
            animator.SetInteger("comboIndex", comboCounter);
            animator.SetTrigger("Slash");
        }

        // Reset the combo timer so it doesn't time out too early
        comboTimer = 0f;
    }

    void ResetCombo()
    {
        comboCounter = 0;
        stationarySlash = false;
        isSlashing = false;
        animator.SetInteger("comboIndex", comboCounter);
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
                enemyUIManager.EnableLockOnIcon();
                enemyUIManager.EnableHealthBar();
                cameraController.SetLockedTarget(enemyUIManager.GetLockOnIcon());
                Debug.Log("Target Locked: " + lockedEnemy.name);
            }
        }
    }

    void CheckUnlockConditions() 
    {
        // Check if the lockedEnemy is still valid (active in hierarchy or not destroyed)
        if (lockedEnemy == null || !lockedEnemy.gameObject.activeInHierarchy)
        {
            UnlockTarget();
            return;
        }

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

    public void TakeDamage(int damage)
    {
        healthManager.UpdateHealth(-damage);

        if (healthManager.GetHealth() <= 0)
        {
            Die();
        }
    }

    public void TakeFireDamage(FireBulletInfo firedamageInfo)
    {
        if (!isBurning)
        {
            fireCoroutine = StartCoroutine(ApplyFireTick(firedamageInfo));
        }

        if (healthManager.GetHealth() <= 0)
        {
            Die();
        }
    }

    private IEnumerator ApplyFireTick(FireBulletInfo firedamageInfo)
    {
        isBurning = true;
        float elapsedTime = 0f;

        while (elapsedTime < firedamageInfo.fireLifeTime)
        {

            healthManager.UpdateHealth(-firedamageInfo.fireTickDamage);

            //  delay tick damage by some amount of time
            yield return new WaitForSeconds(firedamageInfo.fireTickRate);

            elapsedTime += firedamageInfo.fireTickRate;

            //  check if the tick damage kills the player
            if(healthManager.GetHealth() <= 0)
            {
                Die();
                break;
            }
        }
        
        isBurning = false;
    }

    public void TakeIceDamage(IceBulletInfo iceDamageInfo)
    {
        float slowedWalkSpeed = walkSpeed * iceDamageInfo.movementModifier;
        float slowedRunSpeed = runSpeed * iceDamageInfo.movementModifier;

        //  To prevent stacking, check if player is already slowed before applying speed changes and tick damage
        if (!isSlowed)
        {
            isSlowed = true;
            speedCoroutine = StartCoroutine(iceModifier(iceDamageInfo, slowedWalkSpeed, slowedRunSpeed));
        }

        if (healthManager.GetHealth() <= 0)
        {
            Die();
        }
    }

    private IEnumerator iceModifier(IceBulletInfo iceDamageInfo, float slowedWalkSpeed, float slowedRunSpeed)
    {
        float elaspedTime = 0f;

        float originalWalkSpeed = walkSpeed;
        float originalRunSpeed = runSpeed;

        //  Set running speed to be slowed
        walkSpeed = slowedWalkSpeed;
        runSpeed = slowedRunSpeed;

        //  tick damage
        while (elaspedTime < iceDamageInfo.iceLifeTime)
        {
            healthManager.UpdateHealth(-iceDamageInfo.iceTickDamage);

            // delay tick damage by some amount of time
            yield return new WaitForSeconds(iceDamageInfo.iceTickRate);

            elaspedTime += iceDamageInfo.iceTickRate;

            // check if the tick damage kills the player
            if(healthManager.GetHealth() <= 0)
            {
                Die();
                break;
            }
        }

        //  Revert to original speed and reset
        walkSpeed = originalWalkSpeed;
        runSpeed = originalRunSpeed;
        isSlowed = false;
    }

    void Die()
    {
        Debug.Log("Player is dead");
    }

    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(transform.position, targetRange);
    //     Gizmos.color = Color.green;
    //     Gizmos.DrawWireSphere(transform.position, unlockDistance);

    //     if (lockedEnemy != null)
    //     {
    //         // Set gizmo color for locked target (yellow)
    //         Gizmos.color = Color.yellow;
            
    //         // Draw a line from the player to the locked enemy
    //         Gizmos.DrawLine(transform.position, lockedEnemy.position);

    //         // Draw a wire sphere at the locked enemy's position
    //         Gizmos.DrawWireSphere(lockedEnemy.position, 1f);  // The radius can be adjusted as needed
    //     }
    // }
}