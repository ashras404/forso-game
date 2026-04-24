using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required to talk to the Slider UI
using UnityEngine.SceneManagement; // Required to restart the game

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI Elements")]
    public Slider healthSlider;

    void Start()
    {
        currentHealth = maxHealth;

        // Set up the UI Slider to match our health
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        // Update the UI instantly
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        // Did we die?
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player Died! Restarting Level...");
        // For now, if you die, we just instantly restart the current scene!
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}