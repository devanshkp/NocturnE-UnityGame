using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private MoveAroundObject cameraController;

    [Header("References")]
    public CharacterController controller;
    public Transform head;
    public Camera playerCamera;

    [Header("Movement Settings")]
    public float playerSpeed = 5f;
    public float movementRotSpeed = 10;
    public float cameraBasedRotSpeed = 720f;  // Speed of player rotation (degrees per second)

    [Header("Gravity Settings")]
    public float gravity = -9.81f;  // Gravity constant
    public float jumpHeight = 1.5f; // Height of jump
    private float verticalVelocity = 0f;  // Track vertical speed for gravity
    
    private Vector3 movementDirection = Vector3.zero;
    private bool isGrounded;  // Check if the player is on the ground
    private bool jumpRequested = false;  // Track if the jump is requested

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraController = playerCamera.GetComponent<MoveAroundObject>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main; // Fallback to main camera if not assigned
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        HandleMovement();  // Movement is moved to FixedUpdate for smooth physics handling
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
    }
}