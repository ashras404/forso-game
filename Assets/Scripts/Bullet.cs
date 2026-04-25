using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Stats")]
    public float damage = 20f;
    public float crawlSpeed = 2f; // How fast it creeps forward in slow-mo

    private Rigidbody rb;
    private Vector3 storedDirection;
    private float storedForce;
    private bool isFrozen = false;

    // The Gun calls this the exact moment the bullet spawns
    public void Setup(Vector3 direction, float force)
    {
        rb = GetComponent<Rigidbody>();
        storedDirection = direction;
        storedForce = force;

        // Is Za Warudo active?
        if (Time.timeScale < 1f)
        {
            isFrozen = true;
            // Set velocity to a slow crawl instead of full force
            rb.velocity = storedDirection * crawlSpeed;
        }
        else
        {
            // Time is normal! Blast forward instantly
            rb.AddForce(storedDirection * storedForce, ForceMode.Impulse);
            Destroy(gameObject, 3f); // Destroy after 3 seconds
        }
    }

    void Update()
    {
        // If we are currently hanging in the air...
        if (isFrozen)
        {
            // ...and time suddenly goes back to normal
            if (Time.timeScale >= 1f)
            {
                isFrozen = false;

                // Stop the crawling speed
                rb.velocity = Vector3.zero;

                // BOOM! Blast forward with the original force
                rb.AddForce(storedDirection * storedForce, ForceMode.Impulse);

                // Start the self-destruct timer now that it's flying
                Destroy(gameObject, 3f);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // --- NEW: Damage the Drone ---
        EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        // Keep your other collision checks (for the cardboard Targets and Destructibles)
        Target target = collision.gameObject.GetComponentInParent<Target>();
        if (target != null) target.TakeDamage(damage);

        Destructible destructible = collision.gameObject.GetComponent<Destructible>();
        if (destructible != null) destructible.Shatter();

        Destroy(gameObject);
    }
}