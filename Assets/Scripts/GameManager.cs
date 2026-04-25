using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Win Settings")]
    public GameObject winMenuUI;
    private int enemiesRemaining;

    void Start()
    {
        // 1. Automatically count how many enemies are in the scene when we hit Play!
        // This means you can drag in 1 drone or 50 drones, and it will always work.
        enemiesRemaining = FindObjectsOfType<EnemyHealth>().Length;
        Debug.Log("Level Started. Enemies to kill: " + enemiesRemaining);
    }

    public void EnemyDefeated()
    {
        // 2. Subtract 1 from the total whenever an enemy dies
        enemiesRemaining--;
        Debug.Log("Enemy killed! Remaining: " + enemiesRemaining);

        // 3. Did we kill the last one?
        if (enemiesRemaining <= 0)
        {
            WinGame();
        }
    }

    void WinGame()
    {
        Debug.Log("All enemies defeated. You Win!");
        
        // Show the Win Screen
        if (winMenuUI != null) winMenuUI.SetActive(true);

        // Freeze the game and stop the gun from firing
        Time.timeScale = 0f;
        PauseManager.GameIsPaused = true;

        // Unlock the mouse so you can click the buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // We can reuse the same button functions you used for Game Over!
    public void PlayAgain()
    {
        Time.timeScale = 1f;
        PauseManager.GameIsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        PauseManager.GameIsPaused = false;
        SceneManager.LoadScene(0); // Loads Main Menu
    }
}