using UnityEngine;

public class SimpleRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField, Tooltip("Rotation speed in degrees per second.")]
    private float rotationSpeed = 100f;

    [Header("Rotation Axes")]
    [SerializeField, Tooltip("Rotate around the X axis.")]
    private bool rotateX;

    [SerializeField, Tooltip("Rotate around the Y axis.")]
    private bool rotateY;

    [SerializeField, Tooltip("Rotate around the Z axis.")]
    private bool rotateZ;

    void Update()
    {
        Vector3 rotationAxis = new Vector3(
            rotateX ? 1f : 0f,
            rotateY ? 1f : 0f,
            rotateZ ? 1f : 0f
        );

        transform.Rotate(rotationAxis * (rotationSpeed * Time.deltaTime));
    }
}