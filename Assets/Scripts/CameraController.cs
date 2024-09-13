using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAroundObject : MonoBehaviour
{
    public float mouseSensitivity = 3.0f;

    private float _rotationY;
    private float _rotationX;

    public Transform target;

    [Header("Camera Offset Settings")]
    public float distanceFromTarget = 3.0f;   // Distance from the player (Z offset)
    public float heightOffset = 1.0f;         // Height offset from the player's position (Y offset)
    public float horizontalOffset = 0.2f;     // Horizontal offset (X offset)

    private Vector3 currentRotation;
    public Vector3 smoothVelocity = Vector3.zero;

    public float smoothTime = 0.2f;

    public Vector2 _rotationXMinMax = new Vector2(-40, 40);

    public float RotationY => _rotationY;
    public float RotationX => _rotationX;

    void Update()
    {
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

        Vector3 offsetPosition = target.position - transform.forward * distanceFromTarget;
        offsetPosition.y += heightOffset;  // Add the height offset

        // Apply horizontal offset relative to the target's right vector
        offsetPosition += transform.right * horizontalOffset;

        transform.position = offsetPosition;
    }
}
