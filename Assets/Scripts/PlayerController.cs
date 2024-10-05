using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public float movementRotSpeed = 10;
    public float cameraBasedRotSpeed = 720f;  // Speed of player rotation (degrees per second)

    [Header("Combat Settings")]
    public float comboResetTime = 2.0f;  // Time to reset the combo
    private int comboCounter = 0;  // Tracks which slash is next
    private bool isSlashing = false;  // Tracks if the player is currently slashing
    private float comboTimer = 0f;  // Timer to reset the combo

    [Header("Roll")]
    public AnimationCurve rollCurve;
    private bool isRolling = false;  // Is the player currently rolling?
    private float rollTimer;
    private float rollCooldownTimer = 0f;  // Timer to track time since last roll
    public float rollCooldown = 1.5f;  // Time between rolls

    [Header("Gravity Settings")]
    public float gravity = -9.81f;  // Gravity constant
    public float jumpHeight = 1.5f; // Height of jump
    private float verticalVelocity = 0f;  // Track vertical speed for gravity

    [Header("Collider Settings")]
    private Vector3 originalColliderCenter;
    private float originalColliderHeight;
    
    private float playerSpeed = 5f;
    private Vector3 direction = Vector3.zero;
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

        Keyframe rollLastFrame = rollCurve[rollCurve.length - 1];
        rollTimer = rollLastFrame.time;
        // Allow player to roll immediately
        rollCooldownTimer = rollTimer;
    }

    // Update is called once per frame
    void Update()
    {
        checkRunning();
        RecordInputs();
        HandleRotation();
        rollCooldownTimer += Time.deltaTime;
        
        // Get horizontal velocity for animator
        Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
        animator.SetFloat("velocity", horizontalVelocity.magnitude, 0.1f, Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (!isRolling) HandleMovement();  // Regular movement if not rolling
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

        if (Input.GetKey(KeyCode.LeftShift))
        {
            playerSpeed = runSpeed; // Increase speed to runSpeed when Left Shift is pressed
        }
        else
        {
            playerSpeed = walkSpeed; // Reset to default walk speed
        }

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

        if (!isRolling && !isSlashing)
        {
            // Roll mechanic
            if (Input.GetKeyDown(KeyCode.Space) && rollCooldownTimer >= rollCooldown && direction.magnitude != 0)
            {
                StartCoroutine(Roll());
            }
            // Jump mechanic
            if (Input.GetKeyDown(KeyCode.F) && isGrounded)
            {
                jumpRequested = true;
                animator.SetBool("isJumping", true);
            }
            if (Input.GetKeyDown(KeyCode.Mouse0) && isGrounded)
            {
                PerformSlash();
            }
        }
    }

    void HandleMovement()
    {
        controller.Move((direction * playerSpeed + Vector3.up * verticalVelocity) * Time.fixedDeltaTime);
    }

    void checkRunning()
    {
        Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
        animator.SetBool("isRunning", horizontalVelocity.magnitude > 0.1f);
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
        isSlashing = false;
        animator.SetInteger("comboIndex", comboCounter);
    }

}