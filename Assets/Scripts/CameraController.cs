using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAroundObject : MonoBehaviour
{
    public float mouseSensitivity = 3.0f;

    private float _rotationY;
    private float _rotationX;

    public Transform target;

    public float distanceFromTarget = 3.0f;

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

        // Substract forward vector of the GameObject to point its forward vector to the target
        transform.position = target.position - transform.forward * distanceFromTarget;
    }
}
