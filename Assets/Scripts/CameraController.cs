using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAroundObject : MonoBehaviour
{
    private Camera enemyUICam; 
    private Camera mainCam;
    public float mouseSensitivity = 3.0f;
    public Transform target;  // Player to follow
    public Transform lockedTarget;  // Enemy to lock onto when in lock-on mode

    [Header("Camera Distance Settings")]
    public float minDistanceFromTarget = 1.0f;
    public float maxDistanceFromTarget = 4.0f;
    public float combatMaxDistance = 5.0f;
    public float collisionOffset = 0.2f;  // Offset to prevent camera clipping through walls

    [Header("Rotation Settings")]
    private float _rotationY;
    private float _rotationX;
    public float smoothTime = 0.2f;
    private Vector2 rotationXMinMax = new Vector2(-40, 40);  // Limit camera vertical rotation
    private Vector3 currentRotation;
    private Vector3 smoothVelocity = Vector3.zero;

    [Header("Collision Settings")]
    public LayerMask collisionLayers;
    private float currentMaxDistance;
    private float currentDistance;

    [Header("Lock-On Settings")]
    private bool isLockedOn = false; 
    private float cameraLockSpeed = 5f;  // Speed of the camera when rotating around the locked enemy
    private Vector2 combatRotationXMinMax = new Vector2(5, 10);

    void Start()
    {
        mainCam = this.GetComponent<Camera>();
        enemyUICam = transform.Find("EnemyUICamera").GetComponent<Camera>();
        if (target == null) target = GameObject.FindGameObjectWithTag("Player").transform.Find("CameraTarget");
        currentMaxDistance = maxDistanceFromTarget;
        currentDistance = currentMaxDistance;
    }

    void Update()
    {
        if (!isLockedOn)
            HandleFreeMovement();
        else
            HandleLockOnMovement();

        HandleCollision();
        UpdateCameraPosition();
        enemyUICam.fieldOfView = mainCam.fieldOfView;
    }

    // Handles normal free camera movement
    void HandleFreeMovement()
    {
        currentMaxDistance = maxDistanceFromTarget;
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        _rotationY += mouseX;
        _rotationX -= mouseY;
        _rotationX = Mathf.Clamp(_rotationX, rotationXMinMax.x, rotationXMinMax.y);

        Vector3 nextRotation = new Vector3(_rotationX, _rotationY);
        currentRotation = Vector3.SmoothDamp(currentRotation, nextRotation, ref smoothVelocity, smoothTime);
        transform.localEulerAngles = currentRotation;
    }

    // Handles lock-on movement, rotating around the locked enemy
    void HandleLockOnMovement()
    {
        if (lockedTarget == null)
            return; 

        // Max distance of camera from player (for collision function)
        currentMaxDistance = combatMaxDistance;

        // Calculate a position directly behind the player and rotate it to always look at the locked enemy
        Vector3 cameraOffset = target.position - lockedTarget.position;
        cameraOffset.y = 0;  // Flatten the vertical movement to keep the camera level

        // Determine the current position of the camera based on player movement and locked target
        Vector3 desiredCameraPosition = target.position - cameraOffset.normalized * currentDistance;

        // Perform smooth rotation to always look at the locked target
        Quaternion targetRotation = Quaternion.LookRotation(lockedTarget.position - transform.position);
        Vector3 eulerRotation = targetRotation.eulerAngles;
    
        // Clamp the vertical movement of the camera to prevent odd angles
        eulerRotation.x = Mathf.Clamp(eulerRotation.x, combatRotationXMinMax.x, combatRotationXMinMax.y);

        // Rotate the camera accordingly
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(eulerRotation), Time.deltaTime * cameraLockSpeed);

        transform.position = Vector3.Lerp(transform.position, desiredCameraPosition, Time.deltaTime * cameraLockSpeed);
    }


    // Handles camera collision with walls and adjusts the camera's distance from the player accordingly
    void HandleCollision()
    {
        Vector3 desiredCameraPos = target.position - transform.forward * currentMaxDistance;
        RaycastHit hit;

        if (Physics.Raycast(target.position, (desiredCameraPos - target.position).normalized, out hit, currentMaxDistance, collisionLayers))
        {
            float adjustedDistance = hit.distance - collisionOffset;
            currentDistance = Mathf.Clamp(adjustedDistance, minDistanceFromTarget, currentMaxDistance);
        }
        else
        {
            currentDistance = Mathf.Lerp(currentDistance, currentMaxDistance, Time.deltaTime * 5f);
        }
    }

    void UpdateCameraPosition()
    {
        transform.position = target.position - transform.forward * currentDistance;
    }

    public void SetLockedTarget(Transform newLockedTarget)
    {
        lockedTarget = newLockedTarget;
        isLockedOn = (newLockedTarget != null);

        if (!isLockedOn)
        {
            // Get current rotation
            Vector3 currentEulerAngles = transform.eulerAngles;
            // When transitioning back to free movement, update _rotationX and _rotationY
            _rotationY = currentEulerAngles.y;
            _rotationX = currentEulerAngles.x;
            // Set the smooth transition starting rotation to the current one
            currentRotation = currentEulerAngles;
        }
    }
}
