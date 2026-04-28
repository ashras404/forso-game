using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ZaWarudoObject : MonoBehaviour
{
    #region Settings
    [Header("Za Warudo Impact Settings")]
    [Tooltip("Multiplier for the accumulated force released when time resumes.")]
    public float storedForceMultiplier = 1.5f; 
    
    [Tooltip("Makes the object super heavy during slow-mo so it barely moves.")]
    public float slowMoMassMultiplier = 20f;  
    
    [Tooltip("Maximum kinetic energy it can store. Prevents the physics engine from glitching!")]
    public float maxStoredForce = 500f;
    #endregion

    #region State Variables
    private Rigidbody rb;
    private Vector3 storedForce;
    private bool wasInSlowMo = false;
    private float originalMass;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalMass = rb.mass; 
    }

    private void Update()
    {
        HandleTimeState();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only store kinetic energy if we are currently frozen in time
        if (wasInSlowMo)
        {
            StoreKineticEnergy(collision.impulse);
        }
    }
    #endregion

    #region Time & Physics Logic
    private void HandleTimeState()
    {
        bool isCurrentlySlowMo = Time.timeScale < 1f;

        // Triggered the exact frame slow-mo STARTS
        if (isCurrentlySlowMo && !wasInSlowMo)
        {
            EnterSlowMo();
        }
        // Triggered the exact frame slow-mo ENDS
        else if (!isCurrentlySlowMo && wasInSlowMo)
        {
            ExitSlowMo();
        }

        wasInSlowMo = isCurrentlySlowMo;
    }

    private void EnterSlowMo()
    {
        rb.mass = originalMass * slowMoMassMultiplier;
    }

    private void ExitSlowMo()
    {
        // 1. Reset mass back to normal immediately
        rb.mass = originalMass;

        // 2. Only run the physics math if the object was actually hit while frozen!
        if (storedForce.sqrMagnitude > 0.1f) 
        {
            ReleaseStoredEnergy();
        }
    }

    private void StoreKineticEnergy(Vector3 impulse)
    {
        storedForce += impulse;

        // Clamp the force so we don't accidentally launch the object at warp speed
        if (storedForce.magnitude > maxStoredForce)
        {
            storedForce = storedForce.normalized * maxStoredForce;
        }
    }

    private void ReleaseStoredEnergy()
    {
        // Unleash the clamped, multiplied force
        rb.AddForce(storedForce * storedForceMultiplier, ForceMode.Impulse);

        // Clear the stored force for the next time freeze
        storedForce = Vector3.zero;
    }
    #endregion
}