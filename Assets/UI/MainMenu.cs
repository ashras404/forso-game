using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MainMenu : MonoBehaviour
{
    public GameObject menuPanel;
    public PlayerController player;
    public CinemachineVirtualCamera vcam;
    private CinemachinePOV pov;


    void Start()
    {
        // Disable player movement at start
        player.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pov = vcam.GetCinemachineComponent<CinemachinePOV>();

        if (pov != null)
        {
            pov.m_HorizontalAxis.m_MaxSpeed = 0f;
            pov.m_VerticalAxis.m_MaxSpeed = 0f;
        }
    }
    public void PlayGame()
    {
        Debug.Log("Play pressed");

        menuPanel.SetActive(false);
        // menuCam.enabled = false;

        player.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (pov != null)
        {
            pov.m_HorizontalAxis.m_MaxSpeed = 300f;
            pov.m_VerticalAxis.m_MaxSpeed = 300f;
        }
    }

}