using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAroundObject : MonoBehaviour
{
    public float mouseSensitivity = 3.0f;
    public Transform target;  // Player to follow
    public Transform lockedTarget;  // Enemy to lock onto when in lock-on mode

    [Header("Camera Distance Settings")]
    public float distanceFromTarget = 4.0f;
    public float minDistanceFromTarget = 1.0f;
    public float maxDistanceFromTarget = 4.0f;
    public float collisionOffset = 0.2f;  // Offset to prevent camera clipping through walls

    [Header("Rotation Settings")]
    private float _rotationY;
    private float _rotationX;
    public Vector2 rotationXMinMax = new Vector2(-40, 40);  // Limit camera vertical rotation
    public float smoothTime = 0.2f;
    private Vector3 currentRotation;
    private Vector3 smoothVelocity = Vector3.zero;

    [Header("Collision Settings")]
    public LayerMask collisionLayers;
    private float currentDistance;

    [Header("Lock-On Settings")]
    private bool isLockedOn = false;  // Whether the camera is in lock-on mode
    public float cameraLockSpeed = 3f;  // Speed of the camera when rotating around the locked enemy
    public float cameraDrag = 1f;  // Drag to slow down camera movement in lock-on mode
    public float lockOnDistance = 10f; // Distance to maintain when locked on

    void Start()
    {
        currentDistance = maxDistanceFromTarget;
    }

    void Update()
    {
        if (!isLockedOn)
        {
            HandleFreeMovement();
        }
        else
        {
            HandleLockOnMovement();
        }

        HandleCollision();
        UpdateCameraPosition();
    }

    // Handles normal free camera movement
    void HandleFreeMovement()
    {
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

        // Calculate direction from player to the locked enemy
        Vector3 directionToTarget = (lockedTarget.position - target.position).normalized;

        // Calculate a position directly behind the player and rotate it to always look at the locked enemy
        Vector3 cameraOffset = target.position - lockedTarget.position;
        cameraOffset.y = 0;  // Flatten the vertical movement to keep the camera level

        // Determine the current position of the camera based on player movement and locked target
        Vector3 desiredCameraPosition = target.position - cameraOffset.normalized * lockOnDistance;

        // Perform smooth rotation to always look at the locked target
        Quaternion targetRotation = Quaternion.LookRotation(lockedTarget.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * cameraLockSpeed);

        // Get the current distance between the player and the locked target
        float distanceToTarget = Vector3.Distance(target.position, lockedTarget.position);

        // Clamp the vertical movement of the camera and limit the proximity
        currentDistance = Mathf.Lerp(currentDistance, lockOnDistance, Time.deltaTime * cameraLockSpeed);

        // Set the camera's new position (orbit around the player, looking at the locked target)
        transform.position = Vector3.Lerp(transform.position, desiredCameraPosition, Time.deltaTime * cameraLockSpeed);
        
        // Optional: Clamp the vertical movement of the camera to prevent odd angles
        _rotationX = Mathf.Clamp(_rotationX, rotationXMinMax.x, rotationXMinMax.y);
    }



    // Handles camera collision with walls and adjusts the camera's distance from the player accordingly
    void HandleCollision()
    {
        Vector3 desiredCameraPos = target.position - transform.forward * maxDistanceFromTarget;
        RaycastHit hit;

        if (Physics.Raycast(target.position, (desiredCameraPos - target.position).normalized, out hit, maxDistanceFromTarget, collisionLayers))
        {
            float adjustedDistance = hit.distance - collisionOffset;
            currentDistance = Mathf.Clamp(adjustedDistance, minDistanceFromTarget, maxDistanceFromTarget);
        }
        else
        {
            currentDistance = Mathf.Lerp(currentDistance, maxDistanceFromTarget, Time.deltaTime * 5f);
        }
    }

    // Updates the camera position based on the current distance and target
    void UpdateCameraPosition()
    {
        transform.position = target.position - transform.forward * currentDistance;
    }

    public void SetLockedTarget(Transform newLockedTarget)
    {
        lockedTarget = newLockedTarget;
        isLockedOn = (newLockedTarget != null);
    }
}
