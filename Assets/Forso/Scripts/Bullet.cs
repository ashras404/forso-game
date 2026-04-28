using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    #region Properties & Settings
    [Header("Bullet Stats")]
    public float damage = 20f;
    [Tooltip("How fast the bullet creeps forward when time is slowed.")]
    public float crawlSpeed = 2f;
    [Tooltip("How long the bullet lives before automatically destroying itself.")]
    public float lifetime = 3f;

    private Rigidbody rb;
    private Vector3 storedDirection;
    private float storedForce;
    private bool isFrozen = false;
    #endregion

    #region Initialization
    // Called by the Gun script the exact moment the bullet spawns
    public void Setup(Vector3 direction, float force)
    {
        rb = GetComponent<Rigidbody>();
        storedDirection = direction;
        storedForce = force;

        if (Time.timeScale < 1f)
        {
            isFrozen = true;
            rb.velocity = storedDirection * crawlSpeed;
        }
        else
        {
            FireNormally();
        }
    }
    #endregion

    #region Unity Lifecycle
    private void Update()
    {
        // Detect if time has resumed while the bullet is frozen
        if (isFrozen && Time.timeScale >= 1f)
        {
            isFrozen = false;
            FireNormally();
        }
    }
    #endregion

    #region Firing & Collision Logic
    private void FireNormally()
    {
        rb.velocity = Vector3.zero; // Reset velocity in case it was crawling
        rb.AddForce(storedDirection * storedForce, ForceMode.Impulse);
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject hitObj = collision.gameObject;

        // 1. Check for Combat Entities
        EnemyHealth enemy = hitObj.GetComponent<EnemyHealth>();
        if (enemy != null) enemy.TakeDamage(damage);
        
        Destroy(gameObject);
    }
    #endregion
}