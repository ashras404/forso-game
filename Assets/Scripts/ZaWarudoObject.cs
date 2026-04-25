using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ZaWarudoObject : MonoBehaviour
{
    [Header("Za Warudo Impact Settings")]
    public float storedForceMultiplier = 1.5f; // Tweak this to make them fly harder when time resumes
    public float slowMoMassMultiplier = 20f;   // Makes it super heavy so it barely moves during slow-mo

    private Rigidbody rb;
    private Vector3 storedForce;
    private bool wasInSlowMo = false;
    private float originalMass;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalMass = rb.mass; // Remember the normal mass
    }

    void Update()
    {
        // Check if Time is slowed down by looking at the global timeScale
        bool isCurrentlySlowMo = Time.timeScale < 1f;

        // Triggered the exact frame slow-mo STARTS
        if (isCurrentlySlowMo && !wasInSlowMo)
        {
            rb.mass = originalMass * slowMoMassMultiplier;
        }
        // Triggered the exact frame slow-mo ENDS
        else if (!isCurrentlySlowMo && wasInSlowMo)
        {
            ReleaseStoredEnergy();
        }

        wasInSlowMo = isCurrentlySlowMo;
    }

    void OnCollisionEnter(Collision collision)
    {
        // If we are hit while in slow motion...
        if (Time.timeScale < 1f)
        {
            // Store the impact force (impulse) from the collision
            storedForce += collision.impulse;
        }
    }

    void ReleaseStoredEnergy()
    {
        // 1. Reset mass back to normal
        rb.mass = originalMass;

        // 2. Unleash all the stored up force at once!
        rb.AddForce(storedForce * storedForceMultiplier, ForceMode.Impulse);

        // 3. Clear the stored force for the next time
        storedForce = Vector3.zero;
    }
}