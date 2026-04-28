using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    #region Singleton & Global State
    public static UIManager Instance { get; private set; }
    
    public static bool GameIsPaused = false; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    #endregion

    #region UI Panels
    [Header("Interactive Control Center")]
    public GameObject mainMenuPanel;
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;
    public GameObject settingsPanel;
    public GameObject aboutPanel;
    public GameObject playerHUD; 
    #endregion

    #region Camera Parallax (NEW)
    [Header("Menu Camera Parallax")]
    [Tooltip("Assign your Main Menu camera here. Leave empty in gameplay scenes!")]
    public Transform menuCamera;
    public float parallaxMoveAmount = 0.1f;
    public float parallaxRotationAmount = 2f;
    public float parallaxSmoothSpeed = 5f;

    private Vector3 camStartPos;
    private Quaternion camStartRot;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        // Cache the starting position of the camera if one is assigned
        if (menuCamera != null)
        {
            camStartPos = menuCamera.localPosition;
            camStartRot = menuCamera.localRotation;
        }

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            ResetTimeAndCursor(true);
            SwitchPanel(mainMenuPanel);
        }
        else
        {
            ResetTimeAndCursor(false);
            SwitchPanel(null); 
        }
    }

    private void Update()
    {
        // Handle Camera Parallax
        if (menuCamera != null && menuCamera.gameObject.activeInHierarchy)
        {
            HandleCameraParallax();
        }

        // Handle Pausing
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (gameOverPanel != null && gameOverPanel.activeSelf) return;

                if (GameIsPaused) ResumeGame();
                else PauseGame();
            }
        }
    }
    #endregion

    #region Core Menu Functions
    public void PauseGame()
    {
        SwitchPanel(pauseMenuPanel);
        Time.timeScale = 0f;
        GameIsPaused = true;
        SetCursorState(true);
    }

    public void ResumeGame()
    {
        SwitchPanel(null); 
        Time.timeScale = 1f;
        GameIsPaused = false;
        SetCursorState(false);
    }

    public void TriggerGameOver()
    {
        SwitchPanel(gameOverPanel);
        Time.timeScale = 0f;
        GameIsPaused = true;
        SetCursorState(true);
    }
    #endregion

    #region Navigation Buttons
    public void PlayGame()
    {
        ResetTimeAndCursor(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void RetryLevel()
    {
        ResetTimeAndCursor(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitToMainMenu()
    {
        ResetTimeAndCursor(true);
        SceneManager.LoadScene(0); 
    }

    public void QuitApplication()
    {
        Debug.Log("Game is exiting...");
        Application.Quit(); 
    }
    #endregion

    #region Sub-Menus (Tabs)
    public void OpenSettings()
    {
        Debug.Log("Opening Settings...");
        SwitchPanel(settingsPanel);
    }

    public void OpenAbout()
    {
        Debug.Log("Opening About...");
        SwitchPanel(aboutPanel);
    }

    public void BackButton()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
            SwitchPanel(mainMenuPanel);
        else
            SwitchPanel(pauseMenuPanel);
    }
    #endregion

    #region Utilities & Math
    private void SwitchPanel(GameObject activePanel)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (aboutPanel != null) aboutPanel.SetActive(false);

        if (activePanel != null) activePanel.SetActive(true);

        if (playerHUD != null) playerHUD.SetActive(activePanel == null);
    }

    private void SetCursorState(bool isVisible)
    {
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isVisible;
    }

    private void ResetTimeAndCursor(bool inMenu)
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        SetCursorState(inMenu);
    }

    private void HandleCameraParallax()
    {
        float mouseX = Input.mousePosition.x / Screen.width - 0.5f;
        float mouseY = Input.mousePosition.y / Screen.height - 0.5f;

        Vector3 targetPos = camStartPos + new Vector3(mouseX * parallaxMoveAmount, mouseY * parallaxMoveAmount, 0f);
        
        Quaternion targetRot = Quaternion.Euler(
            -mouseY * parallaxRotationAmount,
            mouseX * parallaxRotationAmount,
            0f
        );

        // We use unscaledDeltaTime here so the parallax continues working even if the game is paused!
        menuCamera.localPosition = Vector3.Lerp(menuCamera.localPosition, targetPos, Time.unscaledDeltaTime * parallaxSmoothSpeed);
        menuCamera.localRotation = Quaternion.Slerp(menuCamera.localRotation, camStartRot * targetRot, Time.unscaledDeltaTime * parallaxSmoothSpeed);
    }
    #endregion
}