using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float acceleration = 20f;
    public float deceleration = 25f;

    [Header("Dash Settings")]
    public float dashForce = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;

    [Header("Camera & FOV Settings")]
    public CinemachineVirtualCamera virtualCamera;
    public float normalFOV = 75f;
    public float sprintFOV = 90f;
    public float fovSmoothSpeed = 8f;
    public float idleBobAmplitude = 0.05f;
    public float idleBobSpeed = 1.5f;

    [Header("Audio Settings")]
    public AudioClip[] footstepClips;
    public float stepInterval = 0.5f;
    public AudioClip dashSound;

    // Private State Variables
    private Rigidbody rb;
    private Camera mainCamera;
    private CinemachineBasicMultiChannelPerlin noise;

    private bool isDashing = false;
    private float dashTimer;
    private float dashCooldownTimer;
    private float stepTimer;
    private float idleTimer;

    #region Unity Core Methods
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main; // Cached for better performance

        // Initialize Cinemachine references
        if (virtualCamera != null)
        {
            noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            virtualCamera.m_Lens.FieldOfView = normalFOV;
        }
    }

    void Update()
    {
        HandleCameraNoise();
        AlignPlayerToCamera();
        HandleFOV();
        HandleDashInput();
        HandleFootsteps();
    }

    void FixedUpdate()
    {
        // Separate physics logic
        if (isDashing)
        {
            HandleDash();
        }
        else
        {
            HandleMovement();
        }
    }
    #endregion

    #region Movement Logic
    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        // Calculate movement relative to camera
        Vector3 camForward = mainCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = mainCamera.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDir = camForward * z + camRight * x;
        Vector3 targetVelocity = moveDir * speed;
        Vector3 currentVelocity = rb.velocity;

        // Apply acceleration force
        Vector3 velocityChange = targetVelocity - new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        rb.AddForce(velocityChange * acceleration, ForceMode.Acceleration);
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

        if (dashSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(dashSound, 0.8f);
    }

    void HandleDash()
    {
        dashTimer -= Time.fixedDeltaTime;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 camForward = mainCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = mainCamera.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 dashDir = camForward * z + camRight * x;

        // Default to forward if no input is pressed
        if (dashDir.magnitude < 0.1f)
            dashDir = camForward;

        dashDir.Normalize();
        rb.velocity = dashDir * dashForce;

        if (dashTimer <= 0f)
        {
            isDashing = false;
        }
    }
    #endregion

    #region Camera Logic
    void AlignPlayerToCamera()
    {
        Vector3 forward = mainCamera.transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude > 0.01f)
        {
            transform.forward = forward;
        }
    }

    void HandleFOV()
    {
        if (virtualCamera == null) return;

        bool isMoving = rb.velocity.magnitude > 0.1f;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float targetFOV = normalFOV;

        if (isMoving && isSprinting)
            targetFOV = sprintFOV;
        else if (isMoving)
            targetFOV = normalFOV + 5f;

        // Smoothly transition FOV
        float currentFOV = virtualCamera.m_Lens.FieldOfView;
        virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * fovSmoothSpeed);
    }

    void HandleCameraNoise()
    {
        if (noise == null) return;

        float speed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
        float target = 0f;

        if (speed > 0.1f)
        {
            // Head bob while moving
            target = Input.GetKey(KeyCode.LeftShift) ? 1.2f : 0.6f;
            noise.m_AmplitudeGain = Mathf.Lerp(noise.m_AmplitudeGain, target, Time.deltaTime * 5f);
        }
        else
        {
            // Subtle head bob while idle
            idleTimer += Time.deltaTime * idleBobSpeed;
            float idleOffset = Mathf.Sin(idleTimer) * idleBobAmplitude;
            noise.m_AmplitudeGain = Mathf.Lerp(noise.m_AmplitudeGain, idleOffset, Time.deltaTime * 2f);
        }
    }
    #endregion

    #region Audio Logic
    void HandleFootsteps()
    {
        if (isDashing) return;

        float speed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;

        if (speed > 0.1f)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = Input.GetKey(KeyCode.LeftShift) ? stepInterval * 0.6f : stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    void PlayFootstep()
    {
        if (footstepClips.Length == 0 || AudioManager.Instance == null) return;

        int index = Random.Range(0, footstepClips.Length);
        float originalPitch = AudioManager.Instance.sfxSource.pitch;

        // Randomize pitch slightly for variation
        AudioManager.Instance.sfxSource.pitch = Random.Range(0.9f, 1.1f);
        AudioManager.Instance.PlaySFX(footstepClips[index], 0.7f);
        AudioManager.Instance.sfxSource.pitch = originalPitch;
    }
    #endregion
}