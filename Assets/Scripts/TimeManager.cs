using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing; // We added this!

public class TimeManager : MonoBehaviour
{
    [Header("Za Warudo Settings")]
    public float slowMotionScale = 0.2f; 
    public float maxEnergy = 100f;
    public float drainRate = 25f; 
    public float regenRate = 15f; 

    [Header("UI & Visuals")]
    public Slider energySlider;
    public PostProcessVolume postProcessingVolume; // Changed from GameObject to Volume
    public float effectTransitionSpeed = 5f; // How fast the visual effect fades in/out

    [Header("Audio")]
    public AudioClip activationSound;

    private float currentEnergy;
    private bool isSlowMoActive = false;
    private float normalFixedDeltaTime;

    void Start()
    {
        currentEnergy = maxEnergy;
        normalFixedDeltaTime = Time.fixedDeltaTime;

        if (energySlider != null)
        {
            energySlider.maxValue = maxEnergy;
            energySlider.value = currentEnergy;
        }

        // Ensure weight starts at 0
        if (postProcessingVolume != null)
        {
            postProcessingVolume.weight = 0f;
        }
    }

    void Update()
    {
        HandleInput();
        HandleEnergy();
        HandleVisualTransition(); // Added this!
    }

    void HandleVisualTransition()
    {
        if (postProcessingVolume == null) return;

        // Target weight is 1 if active, 0 if inactive
        float targetWeight = isSlowMoActive ? 1f : 0f;

        // Smoothly slide the weight towards the target using unscaled time (so slow-mo doesn't slow down the fade!)
        postProcessingVolume.weight = Mathf.MoveTowards(postProcessingVolume.weight, targetWeight, effectTransitionSpeed * Time.unscaledDeltaTime);
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isSlowMoActive)
                StopSlowMotion();
            else if (currentEnergy > 0)
                StartSlowMotion();
        }
    }

    void HandleEnergy()
    {
        if (isSlowMoActive)
        {
            currentEnergy -= drainRate * Time.unscaledDeltaTime;
            
            if (currentEnergy <= 0)
            {
                currentEnergy = 0;
                StopSlowMotion();
            }
        }
        else
        {
            if (currentEnergy < maxEnergy)
            {
                currentEnergy += regenRate * Time.unscaledDeltaTime;
                if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
            }
        }

        if (energySlider != null)
            energySlider.value = currentEnergy;
    }

    void StartSlowMotion()
    {
        isSlowMoActive = true;
        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = normalFixedDeltaTime * slowMotionScale; 

        if (activationSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(activationSound, 1f);
        }
    }

    void StopSlowMotion()
    {
        isSlowMoActive = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = normalFixedDeltaTime;
    }
}