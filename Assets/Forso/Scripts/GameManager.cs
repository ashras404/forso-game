using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Singleton
    // This allows any script in the game to access the GameManager instantly!
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // Ensure only one GameManager ever exists in the scene
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Settings & References
    [Header("Win Settings")]
    [Tooltip("The UI Panel that activates when all enemies are defeated.")]
    public GameObject winMenuUI;
    
    [Tooltip("The build index for the Main Menu scene.")]
    public int mainMenuIndex = 0;

    private int enemiesRemaining;
    private bool gameEnded = false;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        // Automatically tally all enemies in the scene at startup
        enemiesRemaining = FindObjectsOfType<EnemyHealth>().Length;
        Debug.Log("Level Started. Enemies to kill: " + enemiesRemaining);
    }
    #endregion

    #region Core Game Logic
    public void EnemyDefeated()
    {
        if (gameEnded) return;

        enemiesRemaining--;
        Debug.Log("Enemy killed! Remaining: " + enemiesRemaining);

        if (enemiesRemaining <= 0)
        {
            WinGame();
        }
    }

    private void WinGame()
    {
        gameEnded = true;
        Debug.Log("All enemies defeated. You Win!");
        
        if (winMenuUI != null) winMenuUI.SetActive(true);

        Time.timeScale = 0f;
        UIManager.GameIsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    #endregion

    #region UI & Scene Management
    public void PlayAgain()
    {
        ResetTime();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        ResetTime();
        SceneManager.LoadScene(mainMenuIndex); 
    }

    private void ResetTime()
    {
        Time.timeScale = 1f;
        UIManager.GameIsPaused = false;
    }
    #endregion
}