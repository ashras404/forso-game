using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine; // We added this to control the camera zoom!

public class WeaponSway : MonoBehaviour
{
    [Header("Aim Down Sights (ADS)")]
    public Vector3 aimPosition; // Type the numbers you found here!
    public float aimSpeed = 10f;
    
    [Header("Camera Zoom")]
    public CinemachineVirtualCamera vcam;
    public float normalFOV = 75f;
    public float aimFOV = 45f; // Zooms in when scoping

    [Header("Mouse Sway Settings")]
    public float swayMultiplier = 2f;
    public float maxSwayAmount = 4f;
    public float swaySmoothness = 5f;

    [Header("Movement Bob Settings")]
    public float walkBobSpeed = 14f;
    public float walkBobAmount = 0.03f;
    public float sprintBobSpeed = 18f;
    public float sprintBobAmount = 0.08f;
    public float bobSmoothness = 10f;

    private Vector3 hipPosition;
    private Quaternion startRotation;
    private float bobTimer;
    private bool isAiming;

    void Start()
    {
        // Remember where the gun normally sits
        hipPosition = transform.localPosition;
        startRotation = transform.localRotation;
    }

    void Update()
    {
        // 1 is the Right Mouse Button!
        isAiming = Input.GetMouseButton(1); 

        HandlePositionAndBob();
        HandleMouseSway();
        HandleCameraZoom();
    }

    void HandlePositionAndBob()
    {
        // 1. Decide where the gun should be (Hip or Centered)
        Vector3 targetPosition = isAiming ? aimPosition : hipPosition;

        // 2. Add movement bob ONLY if we are NOT aiming (keeps the scope steady)
        if (!isAiming)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
            {
                bool isSprinting = Input.GetKey(KeyCode.LeftShift);
                float speed = isSprinting ? sprintBobSpeed : walkBobSpeed;
                float amount = isSprinting ? sprintBobAmount : walkBobAmount;

                bobTimer += Time.unscaledDeltaTime * speed;
                targetPosition.y += Mathf.Sin(bobTimer) * amount;
                targetPosition.x += Mathf.Cos(bobTimer / 2f) * amount * 2f;
            }
            else
            {
                bobTimer = 0f;
            }
        }

        // 3. Smoothly move the gun to the final calculated position
        float smoothSpeed = isAiming ? aimSpeed : bobSmoothness;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.unscaledDeltaTime * smoothSpeed);
    }

    void HandleMouseSway()
    {
        // Reduce sway by half when aiming for better accuracy
        float currentSwayMultiplier = isAiming ? swayMultiplier / 2f : swayMultiplier;

        float mouseX = Input.GetAxisRaw("Mouse X") * currentSwayMultiplier;
        float mouseY = Input.GetAxisRaw("Mouse Y") * currentSwayMultiplier;

        mouseX = Mathf.Clamp(mouseX, -maxSwayAmount, maxSwayAmount);
        mouseY = Mathf.Clamp(mouseY, -maxSwayAmount, maxSwayAmount);

        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);
        Quaternion targetRotation = startRotation * rotationX * rotationY;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, swaySmoothness * Time.unscaledDeltaTime);
    }

    void HandleCameraZoom()
    {
        if (vcam == null) return;

        float targetFOV = isAiming ? aimFOV : normalFOV;
        vcam.m_Lens.FieldOfView = Mathf.Lerp(vcam.m_Lens.FieldOfView, targetFOV, Time.unscaledDeltaTime * aimSpeed);
    }
}