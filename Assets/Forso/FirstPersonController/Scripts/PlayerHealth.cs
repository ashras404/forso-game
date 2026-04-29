using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    #region Singleton
    // This allows the EnemyWanderAI to damage you without dragging and dropping references!
    public static PlayerHealth Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    #endregion

    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;
    
    [Header("UI Elements")]
    public Slider healthSlider;
    // Notice: gameOverUI is GONE! Our UIManager handles that now.

    [Header("Audio Feedback (Optional)")]
    public AudioClip hurtSound;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        // Apply damage and prevent health from dropping below zero
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        UpdateHealthUI();

        // Play a grunt/impact sound with slightly randomized pitch
        if (hurtSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(hurtSound, 1f, 0.9f, 1.1f);
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    private void Die()
    {
        isDead = true;

        // The UIManager instantly pops up the Game Over panel, freezes time, and unlocks the mouse!
        if (UIManager.Instance != null)
        {
            UIManager.Instance.TriggerGameOver();
        }
    }
}