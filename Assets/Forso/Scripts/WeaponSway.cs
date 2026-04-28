using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class WeaponSway : MonoBehaviour
{
    #region Settings & References
    [Header("Aim Down Sights (ADS)")]
    public Vector3 aimPosition;
    [Tooltip("How quickly the gun snaps to the center of the screen.")]
    public float aimSpeed = 10f;

    [Header("Camera Zoom")]
    public CinemachineVirtualCamera vcam;
    public float normalFOV = 75f;
    public float aimFOV = 45f;

    [Header("Recoil Settings")]
    public float recoilKickback = 0.2f; 
    public float recoilRotation = 10f;  
    public float recoilRecoverSpeed = 8f; 

    [Header("Mouse Sway & Roll (AAA Polish)")]
    public float swayMultiplier = 2f;
    public float maxSwayAmount = 4f;
    [Tooltip("How much the gun tilts sideways when you whip the mouse quickly.")]
    public float mouseRollAmount = 2f; 
    public float swaySmoothness = 5f;

    [Header("Movement Bob & Tilt (AAA Polish)")]
    public float walkBobSpeed = 14f;
    public float walkBobAmount = 0.03f;
    public float sprintBobSpeed = 18f;
    public float sprintBobAmount = 0.08f;
    [Tooltip("How much the gun leans when strafing left or right.")]
    public float strafeTiltAmount = 4f; 
    public float bobSmoothness = 10f;
    #endregion

    #region State Variables
    private Vector3 hipPosition;
    private Quaternion startRotation;
    private float bobTimer;
    private bool isAiming;

    // Recoil state
    private float currentRecoilZ;
    private float currentRecoilRotX;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        hipPosition = transform.localPosition;
        startRotation = transform.localRotation;
    }

    private void Update()
    {
        // Freeze all procedural animation if the game is paused!
        if (UIManager.GameIsPaused) return;

        isAiming = Input.GetMouseButton(1);

        // Smoothly recover recoil over time
        currentRecoilZ = Mathf.Lerp(currentRecoilZ, 0f, Time.unscaledDeltaTime * recoilRecoverSpeed);
        currentRecoilRotX = Mathf.Lerp(currentRecoilRotX, 0f, Time.unscaledDeltaTime * recoilRecoverSpeed);

        HandlePositionAndBob();
        HandleSwayAndTilt();
        HandleCameraZoom();
    }
    #endregion

    #region Recoil Trigger
    // Called by the Weapon script every time a bullet fires
    public void TriggerRecoil()
    {
        // Maintains perfect recoil feel even during slow-motion abilities
        float timeMultiplier = Time.timeScale < 1f ? Time.timeScale : 1f;
        currentRecoilZ -= recoilKickback * timeMultiplier;
        currentRecoilRotX -= recoilRotation * timeMultiplier;
    }
    #endregion

    #region Procedural Animation (Position)
    private void HandlePositionAndBob()
    {
        Vector3 targetPosition = isAiming ? aimPosition : hipPosition;

        if (!isAiming)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // Are we moving?
            if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
            {
                bool isSprinting = Input.GetKey(KeyCode.LeftShift);
                float speed = isSprinting ? sprintBobSpeed : walkBobSpeed;
                float amount = isSprinting ? sprintBobAmount : walkBobAmount;

                bobTimer += Time.unscaledDeltaTime * speed;
                targetPosition.y += Mathf.Sin(bobTimer) * amount;
                
                // The figure-8 movement pattern
                targetPosition.x += Mathf.Cos(bobTimer / 2f) * amount * 2f;
            }
            else
            {
                // Smoothly reset the timer so the gun settles back to center
                bobTimer = 0f;
            }
        }

        // Apply the recoil pushback on the Z axis
        targetPosition.z += currentRecoilZ;

        float smoothSpeed = isAiming ? aimSpeed : bobSmoothness;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.unscaledDeltaTime * smoothSpeed);
    }
    #endregion

    #region Procedural Animation (Rotation)
    private void HandleSwayAndTilt()
    {
        float currentSwayMultiplier = isAiming ? swayMultiplier / 2f : swayMultiplier;

        float mouseX = Input.GetAxisRaw("Mouse X") * currentSwayMultiplier;
        float mouseY = Input.GetAxisRaw("Mouse Y") * currentSwayMultiplier;
        float horizontalMove = Input.GetAxisRaw("Horizontal");

        // Clamp the mouse movement so the gun doesn't break your wrists
        mouseX = Mathf.Clamp(mouseX, -maxSwayAmount, maxSwayAmount);
        mouseY = Mathf.Clamp(mouseY, -maxSwayAmount, maxSwayAmount);

        // Base sway from looking around
        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

        // AAA POLISH: Add Z-axis tilt based on strafing and fast mouse movements
        float targetTilt = 0f;
        if (!isAiming)
        {
            targetTilt += horizontalMove * -strafeTiltAmount; // Leans into your walk
            targetTilt += mouseX * -mouseRollAmount;          // Leans into your turn
        }
        Quaternion rollZ = Quaternion.AngleAxis(targetTilt, Vector3.forward);

        // Add the recoil upward tilt
        Quaternion recoilRot = Quaternion.Euler(currentRecoilRotX, 0f, 0f);
        
        // Combine all rotations together
        Quaternion targetRotation = startRotation * rotationX * rotationY * rollZ * recoilRot;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, swaySmoothness * Time.unscaledDeltaTime);
    }
    #endregion

    #region FOV Adjustments
    private void HandleCameraZoom()
    {
        if (vcam == null) return;
        
        float targetFOV = isAiming ? aimFOV : normalFOV;
        vcam.m_Lens.FieldOfView = Mathf.Lerp(vcam.m_Lens.FieldOfView, targetFOV, Time.unscaledDeltaTime * aimSpeed);
    }
    #endregion
}