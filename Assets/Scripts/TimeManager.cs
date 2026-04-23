using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    [Header("Za Warudo Settings")]
    public float slowMotionScale = 0.2f; // How slow the world gets (0.2 = 20% speed)
    public float maxEnergy = 100f;
    public float drainRate = 25f; // Drains fully in 4 seconds
    public float regenRate = 15f; 

    [Header("UI Reference")]
    public Slider energySlider;

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
    }

    void Update()
    {
        HandleInput();
        HandleEnergy();
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
            // Use unscaledDeltaTime so the ability drains in real-time, not slow-mo time
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
        // Adjust fixedDeltaTime so physics stay smooth in slow motion
        Time.fixedDeltaTime = normalFixedDeltaTime * slowMotionScale; 
    }

    void StopSlowMotion()
    {
        isSlowMoActive = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = normalFixedDeltaTime;
    }
}
