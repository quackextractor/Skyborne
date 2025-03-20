using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 2f;

    [FormerlySerializedAs("DashForce")] [SerializeField]
    private float dashForce = 60f;

    public float cooldown = 0.5f;
    public float moveLock = 0.5f;
    public int amountDash = 3;

    private float _moveTimestamp;

    // private bool isGrounded = true;
    // public float jumpForce = 10f;
    private Rigidbody _rb;

    private float _timestamp;
    private float _verticalRotation;

    private readonly float _verticalRotationLimit = 80f;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null) Debug.Log("no rigid");
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (_moveTimestamp < Time.time)
        {
            // Movement
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");
            var movement = new Vector3(horizontal, 0, vertical) * (moveSpeed * Time.deltaTime);
            transform.Translate(movement);
        }

        // Mouse Look
        var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        var mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        _verticalRotation -= mouseY;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -_verticalRotationLimit, _verticalRotationLimit);
        Camera.main.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
        transform.Rotate(0, mouseX, 0);


        /* //cloud perk jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        */

        if (_timestamp < Time.time && amountDash < 3)
        {
            amountDash += 1;
            _timestamp = Time.time + cooldown + moveLock;
        }

        if (amountDash > 0 && _moveTimestamp < Time.time)
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                Dash();

                _moveTimestamp = Time.time + moveLock;
            }
    }

    private void Dash()
    {
        var inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        Vector3 dashDirection;

        if (inputDirection.magnitude > 0.1f) // If player is moving
        {
            dashDirection = transform.TransformDirection(inputDirection.normalized);
            amountDash -= 1;
        }
        else
        {
            dashDirection = transform.forward;
        }


        _rb.velocity = Vector3.zero; // Reset velocity to avoid sliding
        _rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);
        Debug.Log("Dashing in: " + dashDirection);
    }
}