using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class TimeManager : MonoBehaviour
{
    #region Settings & References
    [Header("Time Control Settings")]
    [Tooltip("The time scale when slow motion is active (e.g., 0.2 is 20% speed).")]
    public float slowMotionScale = 0.2f;

    [Header("Visuals & UI")]
    [Tooltip("The Post Processing Volume used for the slow-motion visual effect.")]
    public PostProcessVolume postProcessingVolume;
    
    [Tooltip("How fast the visual effect fades in and out in real-time seconds.")]
    public float effectTransitionSpeed = 5f;

    [Header("Audio")]
    [Tooltip("Sound played when time manipulation is activated.")]
    public AudioClip activationSound;

    private bool isSlowMoActive = false;
    private float normalFixedDeltaTime;
    #endregion

    #region Initialization
    private void Start()
    {
        // Cache the default physics step so we can restore it later
        normalFixedDeltaTime = Time.fixedDeltaTime;

        if (postProcessingVolume != null)
        {
            postProcessingVolume.weight = 0f;
        }
    }
    #endregion

    #region Unity Lifecycle
    private void Update()
    {
        HandleInput();
        HandleVisualTransition();
    }
    #endregion

    #region Time Logic
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isSlowMoActive)
            {
                StopSlowMotion();
            }
            else
            {
                StartSlowMotion();
            }
        }
    }

    private void StartSlowMotion()
    {
        isSlowMoActive = true;
        Time.timeScale = slowMotionScale;
        
        // Scale fixedDeltaTime so physics calculations remain smooth in slow-mo
        Time.fixedDeltaTime = normalFixedDeltaTime * slowMotionScale; 

        if (activationSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(activationSound, 1f);
        }
    }

    private void StopSlowMotion()
    {
        isSlowMoActive = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = normalFixedDeltaTime;
    }
    #endregion

    #region Visual Effects
    private void HandleVisualTransition()
    {
        if (postProcessingVolume == null) return;

        float targetWeight = isSlowMoActive ? 1f : 0f;
        
        // MoveTowards using unscaledDeltaTime ensures the fade animation runs at normal speed 
        // even while the rest of the game is in slow motion!
        postProcessingVolume.weight = Mathf.MoveTowards(
            postProcessingVolume.weight, 
            targetWeight, 
            effectTransitionSpeed * Time.unscaledDeltaTime
        );
    }
    #endregion
}