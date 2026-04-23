using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class WeaponSway : MonoBehaviour
{
    [Header("Aim Down Sights (ADS)")]
    public Vector3 aimPosition; 
    public float aimSpeed = 10f;
    
    [Header("Camera Zoom")]
    public CinemachineVirtualCamera vcam;
    public float normalFOV = 75f;
    public float aimFOV = 45f; 

    [Header("Recoil Settings")]
    public float recoilKickback = 0.2f; // How far the gun pushes toward you
    public float recoilRotation = 10f;  // How far the barrel kicks up
    public float recoilRecoverSpeed = 8f; // How fast it settles back

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

    // Recoil state
    private float currentRecoilZ;
    private float currentRecoilRotX;

    void Start()
    {
        hipPosition = transform.localPosition;
        startRotation = transform.localRotation;
    }

    void Update()
    {
        isAiming = Input.GetMouseButton(1); 

        // Smoothly return recoil values back to 0 over time
        currentRecoilZ = Mathf.Lerp(currentRecoilZ, 0f, Time.unscaledDeltaTime * recoilRecoverSpeed);
        currentRecoilRotX = Mathf.Lerp(currentRecoilRotX, 0f, Time.unscaledDeltaTime * recoilRecoverSpeed);

        HandlePositionAndBob();
        HandleMouseSway();
        HandleCameraZoom();
    }

    // Called by the Weapon script every time a bullet fires
    public void TriggerRecoil()
    {
        currentRecoilZ -= recoilKickback;
        currentRecoilRotX -= recoilRotation;
    }

    void HandlePositionAndBob()
    {
        Vector3 targetPosition = isAiming ? aimPosition : hipPosition;

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

        // Apply the recoil pushback on the Z axis
        targetPosition.z += currentRecoilZ;

        float smoothSpeed = isAiming ? aimSpeed : bobSmoothness;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.unscaledDeltaTime * smoothSpeed);
    }

    void HandleMouseSway()
    {
        float currentSwayMultiplier = isAiming ? swayMultiplier / 2f : swayMultiplier;

        float mouseX = Input.GetAxisRaw("Mouse X") * currentSwayMultiplier;
        float mouseY = Input.GetAxisRaw("Mouse Y") * currentSwayMultiplier;

        mouseX = Mathf.Clamp(mouseX, -maxSwayAmount, maxSwayAmount);
        mouseY = Mathf.Clamp(mouseY, -maxSwayAmount, maxSwayAmount);

        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);
        
        // Add the recoil upward tilt to the target rotation
        Quaternion recoilRot = Quaternion.Euler(currentRecoilRotX, 0f, 0f);
        Quaternion targetRotation = startRotation * rotationX * rotationY * recoilRot;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, swaySmoothness * Time.unscaledDeltaTime);
    }

    void HandleCameraZoom()
    {
        if (vcam == null) return;
        float targetFOV = isAiming ? aimFOV : normalFOV;
        vcam.m_Lens.FieldOfView = Mathf.Lerp(vcam.m_Lens.FieldOfView, targetFOV, Time.unscaledDeltaTime * aimSpeed);
    }
}