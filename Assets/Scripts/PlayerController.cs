using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")] [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField] private float mouseSensitivity = 2f;

    [FormerlySerializedAs("DashForce")] [Header("Dash Settings")] [SerializeField]
    private float dashForce = 60f;

    [SerializeField] private float cooldown = 0.5f;
    [SerializeField] private float moveLock = 0.5f;
    [SerializeField] private int amountDash = 3;
    [SerializeField] private int maxDash = 3;
    [SerializeField] private float iFrames = 0.1f;

    [Header("Speed Particles")] [Tooltip("Particle system to emit when moving fast.")] [SerializeField]
    private ParticleSystem speedParticles;

    [Tooltip("Start emitting particles when speed exceeds this value.")] [SerializeField]

    [Header("Dash slider handle")]
    public GameObject sliderParent;
    private Slider[] sliders;

    private float speedThreshold = 10f;

    private float[] dashTimers;

    private readonly float _verticalRotationLimit = 80f;
    private float _dashRefillTimestamp;
    private ParticleSystem.EmissionModule _emissionModule;

    private float _moveTimestamp;

    private Rigidbody _rb;
    private float _verticalRotation;

    private void Start()
    {
       sliders =  sliderParent.GetComponentsInChildren<Slider>();
        dashTimers = new float[maxDash];
        foreach (Slider slider in sliders) {
            int i = 0;
            dashTimers[i] = 0f;
            slider.maxValue = cooldown + moveLock;
            slider.value = 0f;
            i++;
        }
        
        _rb = GetComponent<Rigidbody>();
        if (_rb == null) Debug.LogError("PlayerController: no Rigidbody found on this GameObject!");

        // Try to auto-find the ParticleSystem if none assigned
        if (speedParticles == null)
            speedParticles = GetComponentInChildren<ParticleSystem>();

        if (speedParticles == null)
            Debug.LogError("PlayerController: no ParticleSystem assigned or in children!");

        // Cache and disable emission initially
        _emissionModule = speedParticles.emission;
        _emissionModule.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleDashRefill();
        HandleDashInput();
        ToggleSpeedParticles();
    }

    private void HandleMovement()
    {
        if (Time.time < _moveTimestamp) return;

        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");
        var move = new Vector3(h, 0, v) * (moveSpeed * Time.deltaTime);
        transform.Translate(move, Space.Self);
    }

    private void HandleMouseLook()
    {
        var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        var mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        _verticalRotation -= mouseY;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -_verticalRotationLimit, _verticalRotationLimit);

        if (Camera.main) Camera.main.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
        transform.Rotate(0, mouseX, 0);
    }

   


    private void HandleDashRefill()
    {
    
        for (int i = 0; i < maxDash; i++)
        {
            float remaining = dashTimers[i] - Time.time;

            if (remaining <= 0f)
            {
                amountDash++;
                sliders[i].value = 0f;
            }
            else
            {
                sliders[i].value = remaining;
            }
    }
}


private void HandleDashInput()
    {
        if (amountDash < 0 || Time.time < _moveTimestamp) return;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(Dash());
            _moveTimestamp = Time.time + moveLock;
        }
    }

    private IEnumerator Dash()
    {
       
        this.gameObject.GetComponent<Target>().enabled = false;

        for (int i = 0; i < maxDash; i++)
        {
            if (Time.time >= dashTimers[i])
            {
                dashTimers[i] = Time.time + cooldown + moveLock;

                amountDash--;
               
                var inputDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                var dashDir = inputDir.magnitude > 0.1f
                    ? transform.TransformDirection(inputDir.normalized)
                    : transform.forward;
                _rb.velocity = Vector3.zero;
                _rb.AddForce(dashDir * dashForce, ForceMode.Impulse);
                break;
            }
        }
        yield return new WaitForSeconds(iFrames);
        this.gameObject.GetComponent<Target>().enabled = true;

    }

    private void ToggleSpeedParticles()
    {
        var currentSpeed = _rb.velocity.magnitude;

        if (currentSpeed > speedThreshold)
        {
            if (!_emissionModule.enabled)
                _emissionModule.enabled = true;

            // Enhance visibility by increasing emission range and sensitivity
            var emissionRate =
                Mathf.Lerp(50f, 300f, Mathf.Clamp01((currentSpeed - speedThreshold) / (moveSpeed * 1.5f)));
            _emissionModule.rateOverTime = new ParticleSystem.MinMaxCurve(emissionRate);
        }
        else
        {
            if (_emissionModule.enabled)
                _emissionModule.enabled = false;
        }
    }

}