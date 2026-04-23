using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Needed for TextMeshPro UI

public class PlayerInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 3f; // How close you need to be to pick it up

    [Header("References")]
    public GameObject interactUI; // Drag your "Press E" text here
    public GameObject playerGun;  // Drag the gun attached to your camera here

    private Camera mainCamera;
    private GameObject currentTarget;

    void Start()
    {
        mainCamera = Camera.main;
        
        // Ensure UI and Gun are hidden when the game starts
        if (interactUI != null) interactUI.SetActive(false);
        if (playerGun != null) playerGun.SetActive(false);
    }

    void Update()
    {
        HandleRaycast();
    }

    void HandleRaycast()
    {
        // Draw an invisible line from the exact center of the camera forward
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;

        // If the raycast hits something within our interact range...
        if (Physics.Raycast(ray, out hit, interactRange))
        {
            // Check if the object we hit has the tag we created
            if (hit.collider.CompareTag("WeaponPickup"))
            {
                currentTarget = hit.collider.gameObject;
                interactUI.SetActive(true); // Show the UI

                // If the player presses E while looking at it
                if (Input.GetKeyDown(KeyCode.E))
                {
                    PickUpWeapon();
                }
                
                return; // Stop the code here so the UI stays on
            }
        }

        // If we look away or are too far, hide the UI and forget the target
        currentTarget = null;
        interactUI.SetActive(false);
    }

    void PickUpWeapon()
    {
        // 1. Activate the gun in our hands
        playerGun.SetActive(true);

        // 2. Hide the UI text
        interactUI.SetActive(false);

        // 3. Destroy the floating gun in the world
        Destroy(currentTarget);
    }
}