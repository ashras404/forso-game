using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingPickup : MonoBehaviour
{
    [Header("Animation Settings")]
    public float spinSpeed = 90f;
    public float bobAmplitude = 0.5f; // How high it bobs
    public float bobSpeed = 2f;       // How fast it bobs

    private float startY;

    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        // Spin the object around its Y axis
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);

        // Make the object bob up and down smoothly
        float newY = startY + Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}