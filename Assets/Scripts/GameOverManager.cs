using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public void RetryLevel()
    {
        Time.timeScale = 1f; 
        PauseManager.GameIsPaused = false; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f; 
        PauseManager.GameIsPaused = false; 
        
        // Load the Main Menu (Scene 0)
        SceneManager.LoadScene(0); 
    }
}