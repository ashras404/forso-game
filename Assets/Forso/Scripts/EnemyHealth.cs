using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    #region Settings & Stats
    [Header("Health Stats")]
    [Tooltip("Maximum health of this specific enemy type.")]
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;
    #endregion

    #region Combat Polish (AAA Features)
    [Header("Combat Feedback")]
    [Tooltip("Optional: Sound to play when the enemy takes damage.")]
    public AudioClip hitSound;
    
    [Tooltip("Optional: Sound to play the exact moment the enemy dies.")]
    public AudioClip deathSound;

    [Tooltip("Optional: Particle effect to spawn where the bullet hits (e.g., blood splat or sparks).")]
    public GameObject hitEffectPrefab;
    #endregion

    #region Death & Animation
    [Header("Death Settings")]
    [Tooltip("The animator controller for the enemy.")]
    public Animator animator;
    
    [Tooltip("Trigger parameter name in the Animator to play the death animation.")]
    public string deathTriggerName = "Die";
    
    [Tooltip("Time to wait for the death animation to play before exploding into debris.")]
    public float delayBeforeDestruction = 0f;
    
    [Tooltip("The fractured debris prefab to spawn upon death.")]
    public GameObject deathEffectPrefab;
    #endregion

    #region Initialization
    private void Start()
    {
        // Set health to full at the start of the level
        currentHealth = maxHealth;
    }
    #endregion

    #region Combat Logic
    // We added 'hitPoint' so you can optionally spawn sparks exactly where the bullet hit!
    public void TakeDamage(float damage, Vector3 hitPoint = default)
    {
        if (isDead) return; 

        // Apply damage and ensure it never drops into weird negative numbers
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        PlayHitFeedback(hitPoint);

        Debug.Log(gameObject.name + " took " + damage + " damage! Health: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void PlayHitFeedback(Vector3 hitPos)
    {
        // 1. Play Hit Audio
        if (hitSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(hitSound, 0.8f);
        }

        // 2. Play Hit Visuals (Sparks/Blood)
        if (hitEffectPrefab != null && hitPos != default)
        {
            Instantiate(hitEffectPrefab, hitPos, Quaternion.identity);
        }
    }
    #endregion

    #region Death & Cleanup
    private void Die()
    {
        isDead = true;

        // Instantly notify the GameManager Singleton to update the kill count!
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyDefeated();
        }

        // Play Death Audio
        if (deathSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(deathSound, 1f);
        }

        // Trigger Death Animation
        if (animator != null)
        {
            animator.SetTrigger(deathTriggerName);
        }

        // Start the destruction timer
        StartCoroutine(DestructionRoutine());
    }

    private IEnumerator DestructionRoutine()
    {
        // Pause the script here to let the death animation play out
        if (delayBeforeDestruction > 0f)
        {
            yield return new WaitForSeconds(delayBeforeDestruction);
        }

        // Explode into the fractured prefab
        if (deathEffectPrefab != null)
        {
            GameObject fractured = Instantiate(deathEffectPrefab, transform.position, transform.rotation);
            
            Rigidbody[] rbPieces = fractured.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rbPieces)
            {
                // We originate the explosion slightly below the center so pieces blow outwards and UP
                rb.AddExplosionForce(500f, transform.position + (Vector3.down * 0.5f), 5f);
            }
        }

        // Erase the enemy from the scene entirely
        Destroy(gameObject);
    }
    #endregion
}