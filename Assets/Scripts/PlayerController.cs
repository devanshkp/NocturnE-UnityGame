using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// ORIGINAL COLLIDER HEIGHT = 1.8
// ORIGINAL COLLIDER CENTER = (0,0.075,0)

public class NewBehaviourScript : MonoBehaviour
{
    private MoveAroundObject cameraController;
    private Animator animator;
    private Transform lockedEnemy;  // The enemy the player is locked onto

    [Header("References")]
    public CharacterController controller;
    public Transform cameraTarget;
    public Camera playerCamera;

    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float accelerationRate = 8f;
    public float decelerationRate = 5f;
    public float movementRotSpeed = 10;
    public float cameraBasedRotSpeed = 720f;  // Speed of player rotation (degrees per second)
    private float playerSpeed = 0f;
    private Vector3 horizontalVelocity;
    private Vector3 direction = Vector3.zero;

    [Header("FOV Settings")]
    public float normalFOV = 60f;
    public float sprintFOV = 75f;
    public float fovTransitionSpeed = 5f;
    public float fovTransitionThres = 5f;

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
    public float comboResetTime = 1.5f;  // Time to reset the combo
    private int comboCounter = 0;  // Tracks which slash is next
    private bool isSlashing = false;  // Tracks if the player is currently slashing
    private float comboTimer = 0f;  // Timer to track time since last slash
    private bool stationarySlash = false;

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
    public float gravity = -9.81f;  // Gravity constant
    public float jumpHeight = 1.25f; // Height of jump
    private float verticalVelocity = 0f;  // Track vertical speed for gravity

    [Header("Collider Settings")]
    private Vector3 originalColliderCenter;
    private float originalColliderHeight;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        cameraController = playerCamera.GetComponent<MoveAroundObject>();

        // originalHealthBarWidth = healthBar.rectTransform.rect.width;

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

    // Update is called once per frame
    void Update()
    {
        RecordInputs();
        UpdateVelocityAndFOV();
        HandleRotation();
        UpdateStamina();
        isGrounded = controller.isGrounded;
        rollCooldownTimer += Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (!isRolling && !stationarySlash) HandleMovement();  // Allow movement if not rolling or attacking while stationary
        HandleVerticalMovement();
        if (isSlashing)
        {
            comboTimer += Time.fixedDeltaTime;
            if (comboTimer > comboResetTime)
            {
                ResetCombo();
            }
        }
    }

    // Handles player movement based on user input
    void RecordInputs()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right Arrow keys
        float verticalInput = Input.GetAxisRaw("Vertical");     // W/S or Up/Down Arrow keys

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && currentStamina > 0; // Can only run if stamina is available
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
        {
            playerSpeed = Mathf.MoveTowards(playerSpeed, targetSpeed, accelerationRate * Time.deltaTime);
        }
        else 
        {
            playerSpeed = Mathf.MoveTowards(playerSpeed, 0, decelerationRate * Time.deltaTime);
        }

        if (!isRolling && !isSlashing)
        {
            // Roll mechanic
            if (Input.GetKeyDown(KeyCode.Space) && rollCooldownTimer >= rollCooldown && direction.magnitude != 0)
            {
                StartCoroutine(Roll());
            }
            // Jump mechanic
            if (Input.GetKeyDown(KeyCode.F) && isGrounded && currentStamina >= jumpStaminaCost)
            {
                jumpRequested = true;
                animator.SetBool("isJumping", true);
                currentStamina -= jumpStaminaCost;
                staminaSlider.value = currentStamina;
                staminaCooldownTimer = 0f;
            }
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                PerformSlash();
            }
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

    void HandleMovement()
    {
        controller.Move((direction * playerSpeed + Vector3.up * verticalVelocity) * Time.fixedDeltaTime);
    }

    void UpdateVelocityAndFOV()
    {
        // Get horizontal velocity for animator
        horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
        animator.SetFloat("velocity", horizontalVelocity.magnitude, 0.1f, Time.deltaTime);
        if (horizontalVelocity.magnitude >= fovTransitionThres)
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, fovTransitionSpeed * Time.deltaTime);
        else
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, normalFOV, fovTransitionSpeed * Time.deltaTime);
    }


    // Handles player rotation based on movement direction
    void HandleRotation()
    {        
        if (direction.magnitude == 0) return;
        float rotSpeed = movementRotSpeed;
        if (isRolling) rotSpeed *= 0.5f;
        Quaternion desiredRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotSpeed * Time.deltaTime);
    }

    // Apply gravity to the player
    void HandleVerticalMovement()
    {
        // Check if the player is grounded

        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;  // Small downward velocity to keep the player grounded
        }

        // Apply gravity
        verticalVelocity += gravity * Time.fixedDeltaTime;        

        // Jump logic
        if (jumpRequested && isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpRequested = false; // Reset jump request
        }

        if (isGrounded && verticalVelocity <= 0.1f)
        {
            animator.SetBool("isJumping", false);  // Stop jump animation
        }
    }

    IEnumerator Roll()
    {
        animator.SetBool("isRolling", true);
        isRolling = true;
        rollCooldownTimer = 0f;  // Reset cooldown timer
        controller.height = originalColliderHeight * 0.5f;  // Halve the collider height
        controller.center = new Vector3(0, -0.375f, 0); // Lower collider
        float timer = 0;
        while (timer < rollTimer){
            float speed = rollCurve.Evaluate(timer);
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

    void PerformSlash()
    {
        // If already slashing, only allow second slash to be triggered within the animation sequence
        if (isSlashing && comboCounter == 1)
        {
            // Perform second slash
            comboCounter = 2;
            animator.SetInteger("comboIndex", comboCounter);
        }
        else if (!isSlashing)
        {
            if (horizontalVelocity.magnitude < 0.01){
                stationarySlash = true;
            }
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

    void Die()
    {
        Debug.Log("Player is dead");
    }
}