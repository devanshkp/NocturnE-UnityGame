using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAroundObject : MonoBehaviour
{
    public float mouseSensitivity = 3.0f;

    private float _rotationY;
    private float _rotationX;

    public Transform target;
    public float distanceFromTarget = 4.0f;   // Desired distance from the player (Z offset)
    public float minDistanceFromTarget = 1.0f; // Minimum distance to prevent the camera from getting too close
    public float maxDistanceFromTarget = 4.0f; // Max distance to avoid zooming too far away
    public float collisionOffset = 0.2f; // Offset to avoid clipping into walls

    private Vector3 currentRotation;
    public Vector3 smoothVelocity = Vector3.zero;

    public float smoothTime = 0.2f;

    public Vector2 _rotationXMinMax = new Vector2(-40, 40);

    public LayerMask collisionLayers; // Layers to detect for collision

    private float currentDistance; // Store the current distance of the camera

    public float RotationY => _rotationY;
    public float RotationX => _rotationX;

    void Start()
    {
        // Initialize the current distance to the max distance (desired distance)
        currentDistance = maxDistanceFromTarget;
    }

    void Update()
    {
        // Handle camera rotation based on mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        _rotationY += mouseX;
        _rotationX -= mouseY;

        // Apply clamping for x rotation 
        _rotationX = Mathf.Clamp(_rotationX, _rotationXMinMax.x, _rotationXMinMax.y);

        Vector3 nextRotation = new Vector3(_rotationX, _rotationY);

        // Apply damping between rotation changes
        currentRotation = Vector3.SmoothDamp(currentRotation, nextRotation, ref smoothVelocity, smoothTime);
        transform.localEulerAngles = currentRotation;

        Vector3 desiredCameraPos = target.position - transform.forward * maxDistanceFromTarget;

        // Perform collision detection using raycasting
        RaycastHit hit;
        if (Physics.Raycast(target.position, (desiredCameraPos - target.position).normalized, out hit, maxDistanceFromTarget, collisionLayers)){
            float adjustedDistance = hit.distance - collisionOffset;
            // Immediately adjust to prevent clipping
            currentDistance = Mathf.Clamp(adjustedDistance, minDistanceFromTarget, maxDistanceFromTarget);
        }
        else{
            // If no collision detected, smoothly move the camera back to the max distance
            currentDistance = Mathf.Lerp(currentDistance, maxDistanceFromTarget, Time.deltaTime * 5f);
        }

        transform.position = target.position - transform.forward * currentDistance;
    }
}
