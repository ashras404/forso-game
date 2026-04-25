using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI Elements")]
    public Slider healthSlider;
    
    // --- NEW: Slot for the Game Over screen ---
    public GameObject gameOverUI; 

    void Start()
    {
        currentHealth = maxHealth;
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (healthSlider != null) healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // 1. Show the Game Over screen!
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        // 2. Freeze the game
        Time.timeScale = 0f;

        // 3. We reuse your PauseManager logic here to stop the gun from firing while dead!
        PauseManager.GameIsPaused = true;

        // 4. Unlock the mouse cursor so you can click Retry or Menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}