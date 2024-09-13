using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private MoveAroundObject cameraController;
    private Animator animator;
    private Transform lockedEnemy;  // The enemy the player is locked onto

    [Header("References")]
    public CharacterController controller;
    public Transform head;
    public Camera playerCamera;

    [Header("Movement Settings")]
    public float playerSpeed = 5f;
    public float movementRotSpeed = 10;
    public float cameraBasedRotSpeed = 720f;  // Speed of player rotation (degrees per second)

    [Header("Roll Settings")]
    public float rollDistance = 10f;  // Speed of the roll
    public float rollCooldown = 1.5f;  // Time between rolls
    private bool isRolling = false;  // Is the player currently rolling?
    private Vector3 rollDirection;  // Direction of the roll
    private float rollCooldownTimer = 0f;  // Timer to track time since last roll

    [Header("Gravity Settings")]
    public float gravity = -9.81f;  // Gravity constant
    public float jumpHeight = 1.5f; // Height of jump
    private float verticalVelocity = 0f;  // Track vertical speed for gravity

    [Header("Collider Settings")]
    private Vector3 originalColliderCenter;
    private float originalColliderHeight;
    
    private Vector3 movementDirection = Vector3.zero;
    private bool isGrounded;  // Check if the player is on the ground
    private bool jumpRequested = false;  // Track if the jump is requested

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        cameraController = playerCamera.GetComponent<MoveAroundObject>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main; // Fallback to main camera if not assigned
        }

        // Save original collider size
        originalColliderCenter = controller.center;
        originalColliderHeight = controller.height;
    }

    // Update is called once per frame
    void Update()
    {
        checkRunning();
        // Get horizontal velocity
        if (!isRolling)
        {
            // Handle rotation and regular movement while not rolling
            HandleRotation();
            HandleRollInput();
        }
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
            animator.SetBool("isJumping", true);
        }
    }

    void FixedUpdate()
    {
        if (!isRolling){
            HandleMovement();  // Regular movement if not rolling
        }
        else{
            HandleRoll();
        }

        ApplyGravity();
    }

    // Handles player movement based on user input
    void HandleMovement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right Arrow keys
        float verticalInput = Input.GetAxisRaw("Vertical");     // W/S or Up/Down Arrow keys

        // Get the camera's forward and right vectors
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;

        // Ignore the Y component
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        // Combine the input with the camera's direction to get the movement direction
        Vector3 inputDirection = (cameraForward * verticalInput + cameraRight * horizontalInput).normalized;

        // Store the movement direction
        movementDirection = inputDirection * playerSpeed;

        // Combine horizontal movement with vertical velocity (gravity/jump)
        Vector3 finalMovement = movementDirection + Vector3.up * verticalVelocity;
        controller.Move(finalMovement * Time.fixedDeltaTime);
    }

    void checkRunning(){
        Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
        animator.SetBool("isRunning", horizontalVelocity.magnitude > 0.1f);
    }

    // Handles player rotation based on movement direction
    void HandleRotation()
    {        
        float horizontalInput = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right Arrow keys
        float verticalInput = Input.GetAxisRaw("Vertical");     // W/S or Up/Down Arrow keys

        // Get the camera's forward and right vectors
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;

        // Ignore the Y component
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        // Combine the input with the camera's direction to get the movement direction
        Vector3 inputDirection = (cameraForward * verticalInput + cameraRight * horizontalInput).normalized;

        // If there is input, rotate the player towards the direction of movement
        if (inputDirection != Vector3.zero)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, movementRotSpeed * Time.deltaTime);
        }
    }

    // Apply gravity to the player
    void ApplyGravity()
    {
        // Check if the player is grounded
        isGrounded = controller.isGrounded;

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

    void HandleRollInput()
    {
        // Increment cooldown timer
        rollCooldownTimer += Time.deltaTime;

        // Check if enough time has passed since the last roll
        if (Input.GetKeyDown(KeyCode.LeftShift) && rollCooldownTimer >= rollCooldown && !isRolling)
        {
            // Initiate roll
            isRolling = true;
            rollCooldownTimer = 0f;  // Reset cooldown timer

            // Get current movement direction
            rollDirection = movementDirection.normalized;

            // Adjust collider for immunity frames
            controller.height = originalColliderHeight * 0.5f;  // Halve the collider height
            controller.center = new Vector3(controller.center.x, originalColliderCenter.y - 0.5f, controller.center.z); // Lower collider

            // Trigger roll animation (assuming you have one set up)
            animator.SetBool("isRolling", true);
        }
    }

    private void HandleRoll(){
        // Calculate the roll movement direction
        Vector3 rollMovement = rollDirection * rollDistance * Time.fixedDeltaTime;
        
        // Combine with vertical velocity (gravity) to ensure the character doesn't float
        rollMovement.y = verticalVelocity;

        // Move the character
        controller.Move(rollMovement);

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.normalizedTime >= 1.0f){
            // Restore collider after roll
            controller.height = originalColliderHeight;
            controller.center = originalColliderCenter;

            // End rolling
            isRolling = false;
            animator.SetBool("isRolling", false);
        }
    }
}