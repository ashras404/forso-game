using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCameraMotion : MonoBehaviour
{
    public float moveAmount = 0.1f;     // position offset
    public float rotationAmount = 2f;   // tilt amount
    public float smoothSpeed = 5f;
    

    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        startPos = transform.localPosition;
        startRot = transform.localRotation;
    }

    void Update()
    {
        float mouseX = Input.mousePosition.x / Screen.width - 0.5f;
        float mouseY = Input.mousePosition.y / Screen.height - 0.5f;

        // Position offset (subtle drift)
        Vector3 targetPos = startPos + new Vector3(mouseX * moveAmount, mouseY * moveAmount, 0f);

        // Rotation (tilt)
        Quaternion targetRot = Quaternion.Euler(
            -mouseY * rotationAmount,
            mouseX * rotationAmount,
            0f
        );

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * smoothSpeed);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, startRot * targetRot, Time.deltaTime * smoothSpeed);
    }
}