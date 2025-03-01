using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
	[SerializeField] private float moveSpeed = 5f;
	[SerializeField] private float mouseSensitivity = 2f;

	private float verticalRotationLimit = 80f;
	private float verticalRotation = 0f;

	void Update()
	{
		// Movement
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");
		Vector3 movement = new Vector3(horizontal, 0, vertical) * (moveSpeed * Time.deltaTime);
		transform.Translate(movement);

		// Mouse Look
		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

		verticalRotation -= mouseY;
		verticalRotation = Mathf.Clamp(verticalRotation, -verticalRotationLimit, verticalRotationLimit);
		Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
		transform.Rotate(0, mouseX, 0);
	}
}