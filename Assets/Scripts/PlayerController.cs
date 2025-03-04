using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;


public class PlayerController : MonoBehaviour
{
	[SerializeField] private float moveSpeed = 5f;
	[SerializeField] private float mouseSensitivity = 2f;
	[FormerlySerializedAs("DashForce")] [SerializeField] private float dashForce = 60f;

	private float _verticalRotationLimit = 80f;
	private float _verticalRotation = 0f;
   // private bool isGrounded = true;
   // public float jumpForce = 10f;
    Rigidbody _rb;

    float _timestamp = 0f;
    float _moveTimestamp = 0f;
    public float cooldown = 0.5f;


    void Start()
    {

        _rb = GetComponent<Rigidbody>();
        if (_rb == null) {
            Debug.Log("no rigid");
        }
        Cursor.lockState = CursorLockMode.Locked;

    }
    void Update()
    { 
       if (_moveTimestamp < Time.time){
            // Movement
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 movement = new Vector3(horizontal, 0, vertical) * (moveSpeed * Time.deltaTime);
            transform.Translate(movement);
        }
        // Mouse Look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

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


        if (_timestamp < Time.time)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                Dash();
                _timestamp = Time.time + cooldown;
                _moveTimestamp = Time.time + cooldown;

            }

        }
       
     

    }

	void Dash() {

        Vector3 inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        Vector3 dashDirection;

        if (inputDirection.magnitude > 0.1f) // If player is moving
        {
            dashDirection = transform.TransformDirection(inputDirection.normalized);
        }
        else
        {
            dashDirection = transform.forward; 
        }

        //dashDirection.Normalize();

      //  rb.velocity = Vector3.zero; // Reset velocity to avoid sliding
        _rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);
        Debug.Log("Dashing in: " + dashDirection);
    
}
}