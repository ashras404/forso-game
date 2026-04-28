using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyBullet : MonoBehaviour
{
    #region Settings & Stats
    [Header("Bullet Stats")]
    [Tooltip("How much damage this bullet deals to the player.")]
    public float damage = 20f;
    
    [Tooltip("Normal flight speed.")]
    public float speed = 15f;
    
    [Tooltip("How fast it creeps forward when time is slowed.")]
    public float crawlSpeed = 1f; 
    
    [Tooltip("How long the bullet lives before automatically disappearing.")]
    public float lifetime = 5f;

    private Rigidbody rb;
    private Vector3 travelDirection;
    private bool isFrozen = false;
    #endregion

    #region Initialization
    public void Setup(Vector3 direction)
    {
        rb = GetComponent<Rigidbody>();
        travelDirection = direction.normalized;

        // Check if time is slowed down the exact moment it spawns
        if (Time.timeScale < 1f)
        {
            isFrozen = true;
            rb.velocity = travelDirection * crawlSpeed;
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
        // Detect if time has resumed while the bullet was crawling
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
        rb.velocity = travelDirection * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 1. Check if we hit the player
        PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damage);
        }

        // 2. Destroy the bullet regardless of what it hits (walls, floors, player, etc.)
        Destroy(gameObject); 
    }
    #endregion
}