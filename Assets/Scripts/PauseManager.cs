using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    // We make this 'static' so other scripts (like the gun) can easily check if the game is paused!
    public static bool GameIsPaused = false; 
    
    [Header("UI References")]
    public GameObject pauseMenuUI;

    void Update()
    {
        // Listen for the Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false); // Hide menu
        Time.timeScale = 1f; // Unfreeze time
        GameIsPaused = false;
        
        // Lock the cursor back to the center of the screen for shooting
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true); // Show menu
        Time.timeScale = 0f; // Freeze game!
        GameIsPaused = true;
        
        // Unlock the mouse cursor so we can click the buttons!
        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true;
    }

    public void OpenSettings()
    {
        Debug.Log("Loading Settings...");
    }

    public void ExitToMainMenu()
    {
        // TRAP WARNING: We MUST set time back to normal before changing scenes, 
        // otherwise the Main Menu will be frozen too!
        Time.timeScale = 1f; 
        SceneManager.LoadScene(0); // 0 is your MainMenu in the Build Settings
    }
}