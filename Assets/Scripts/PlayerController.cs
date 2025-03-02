using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
	[SerializeField] private float moveSpeed = 5f;
	[SerializeField] private float mouseSensitivity = 2f;
	[SerializeField] private float DashForce = 60f;

	private float verticalRotationLimit = 80f;
	private float verticalRotation = 0f;
   // private bool isGrounded = true;
   // public float jumpForce = 10f;
    Rigidbody rb;

    float timestamp = 0f;
    float MoveTimestamp = 0f;
    public float cooldown = 0.5f;


    void Start()
    {

        rb = GetComponent<Rigidbody>();
        if (rb == null) {
            Debug.Log("no rigid");
        }
    }
    void Update()
    {
        if (MoveTimestamp < Time.time)
        {
            // Movement
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 movement = new Vector3(horizontal, 0, vertical) * (moveSpeed * Time.deltaTime);
            transform.Translate(movement);

            Cursor.lockState = CursorLockMode.Locked;
        }
        // Mouse Look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalRotationLimit, verticalRotationLimit);
        Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        transform.Rotate(0, mouseX, 0);



        /* //cloud perk jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
		*/
        if (timestamp < Time.time)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                Dash();
                timestamp = Time.time + cooldown;
                MoveTimestamp = Time.time + cooldown-0.25f;

            }

        }
       
     

    }

	void Dash() {

        rb.AddForce(transform.forward * 10f,ForceMode.Impulse);
        Debug.Log("afa");
    }
}