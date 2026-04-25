using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Target Settings")]
    public float maxHealth = 20f;
    public float fallSpeed = 10f; 
    public float popUpTime = 3f;  

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float moveDistance = 3f; 

    private float currentHealth;
    private Vector3 startPos;
    private Quaternion startRot;
    private bool isDown = false;

    void Start()
    {
        currentHealth = maxHealth;
        startPos = transform.position;
        startRot = transform.rotation;
    }

    void Update()
    {
        if (!isDown)
        {
            // CHANGED: Now using Time.time so it slows down when Za Warudo is active!
            float offset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;
            transform.position = startPos + (transform.right * offset);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDown) return;

        currentHealth -= amount;
        
        if (currentHealth <= 0)
        {
            StartCoroutine(FallAndReset());
        }
    }

    IEnumerator FallAndReset()
    {
        isDown = true;
        
        Quaternion flatRotation = startRot * Quaternion.Euler(90f, 0f, 0f);

        float t = 0f;
        while (t < 1f)
        {
            // CHANGED: Now using Time.deltaTime so the falling animation is also in slow-mo!
            t += Time.deltaTime * fallSpeed; 
            transform.rotation = Quaternion.Slerp(startRot, flatRotation, t);
            yield return null;
        }

        // WaitForSeconds automatically respects timeScale, so this will take longer in slow-mo too!
        yield return new WaitForSeconds(popUpTime);

        transform.rotation = startRot;
        currentHealth = maxHealth;
        isDown = false;
    }
}