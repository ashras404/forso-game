using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float acceleration = 20f;
    public float deceleration = 25f;


    [Header("Cinemachine")]
    public CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin noise;
    private CinemachineVirtualCamera vcam;

    [Header("FOV Kick")]
    public float normalFOV = 75f;
    public float sprintFOV = 90f;
    public float fovSmoothSpeed = 8f;

    [Header("Dash")]
    public float dashForce = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;

    private bool isDashing = false;
    private float dashTimer;
    private float dashCooldownTimer;
    private Rigidbody rb;



    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (virtualCamera != null)
            noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        vcam = virtualCamera;

        if (vcam != null)
        {
            vcam.m_Lens.FieldOfView = normalFOV;
        }
    }
    void Update()
    {
        HandleCameraNoise();
        AlignPlayerToCamera();
        HandleFOV();
        HandleDashInput();
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            HandleDash();
        }
        else
        {
            HandleMovement();
        }
    }

    void AlignPlayerToCamera()
    {
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude > 0.01f)
        {
            transform.forward = forward;
        }
    }

    void HandleDashInput()
    {
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) && dashCooldownTimer <= 0f)
        {
            StartDash();
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
    }

    void HandleDash()
    {
        dashTimer -= Time.fixedDeltaTime;

        // Get movement direction
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = Camera.main.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 dashDir = camForward * z + camRight * x;

        // If no input → dash forward
        if (dashDir.magnitude < 0.1f)
            dashDir = camForward;

        dashDir.Normalize();

        rb.velocity = dashDir * dashForce;

        if (dashTimer <= 0f)
        {
            isDashing = false;
        }
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        // Get camera forward direction (ignore vertical tilt)
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = Camera.main.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDir = camForward * z + camRight * x;

        Vector3 targetVelocity = moveDir * speed;
        Vector3 currentVelocity = rb.velocity;

        Vector3 velocityChange = targetVelocity - new Vector3(currentVelocity.x, 0f, currentVelocity.z);

        rb.AddForce(velocityChange * acceleration, ForceMode.Acceleration);
    }

    void HandleFOV()
    {
        if (vcam == null) return;

        bool isMoving = rb.velocity.magnitude > 0.1f;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        float targetFOV = normalFOV;

        // Slight boost when moving
        if (isMoving)
            targetFOV = normalFOV + 5f;

        // Bigger boost when sprinting
        if (isMoving && isSprinting)
            targetFOV = sprintFOV;

        float currentFOV = vcam.m_Lens.FieldOfView;

        vcam.m_Lens.FieldOfView = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * fovSmoothSpeed);
    }

    void HandleCameraNoise()
    {
        if (noise == null) return;

        float speed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;

        float target = 0f;

        if (speed > 0.1f)
        {
            if (Input.GetKey(KeyCode.LeftShift))
                target = 1.2f;
            else
                target = 0.6f;
        }

        noise.m_AmplitudeGain = Mathf.Lerp(noise.m_AmplitudeGain, target, Time.deltaTime * 5f);
    }
}