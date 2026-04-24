using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyBullet : MonoBehaviour
{
    public float speed = 15f;
    public float crawlSpeed = 1f; // How fast it moves when time is frozen

    private Rigidbody rb;
    private Vector3 travelDirection;
    private bool isFrozen = false;

    public void Setup(Vector3 direction)
    {
        rb = GetComponent<Rigidbody>();
        travelDirection = direction.normalized;

        // Is Za Warudo active the moment it spawns?
        if (Time.timeScale < 1f)
        {
            isFrozen = true;
            rb.velocity = travelDirection * crawlSpeed;
        }
        else
        {
            // Time is normal!
            rb.velocity = travelDirection * speed;
            Destroy(gameObject, 5f); // Clean up after 5 seconds
        }
    }

    void Update()
    {
        // If the bullet is frozen, and time suddenly resumes...
        if (isFrozen && Time.timeScale >= 1f)
        {
            isFrozen = false;
            rb.velocity = travelDirection * speed; // Blast forward!
            Destroy(gameObject, 5f);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(20f);
        }
        Destroy(gameObject); 
    }
}