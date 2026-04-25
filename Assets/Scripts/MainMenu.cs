using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // We need this to change levels!

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // This loads the next scene in your Build Settings queue (Level 1)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenSettings()
    {
        // We will build the actual pop-up panel for this later
        Debug.Log("Settings Menu Triggered!");
    }

    public void OpenAbout()
    {
        // We will build the actual pop-up panel for this later
        Debug.Log("About Menu Triggered!");
    }

    public void QuitGame()
    {
        Debug.Log("Game is exiting...");
        
        // This actually closes the application when you build the final game (.exe)
        Application.Quit(); 
    }
}