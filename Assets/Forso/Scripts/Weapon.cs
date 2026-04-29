using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    #region References
    [Header("Core References")]
    public Camera fpsCamera; 
    public Transform firePoint; 
    public GameObject bulletPrefab;
    #endregion

    #region Stats
    [Header("Weapon Stats")]
    public float damage = 20f;
    public float fireRate = 10f;
    public float maxRange = 100f; 

    [Header("Ammo Settings")]
    public int maxAmmo = 30;
    public float reloadTime = 1.5f;
    private int currentAmmo;
    private bool isReloading = false;

    private float nextTimeToFire = 0f;
    #endregion

    #region Audio & Visuals
    [Header("Feedback")]
    public AudioClip shootSound;
    public AudioClip reloadSound; // Added for the reload sequence
    public ParticleSystem muzzleFlash;
    #endregion

    private void Start()
    {
        // Start the level with a full magazine
        currentAmmo = maxAmmo;
    }

    private void OnEnable()
    {
        // Safety check: if you switch weapons while reloading, reset the flag
        isReloading = false;
    }

    private void Update()
    {
        if (UIManager.GameIsPaused) return;

        // 1. Prevent all actions if currently reloading
        if (isReloading) return;

        // 2. Manual Reload Check (Pressing R)
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
            return;
        }

        // 3. Shooting Logic
        if (Input.GetMouseButton(0) && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            
            if (currentAmmo > 0)
            {
                Shoot();
            }
            else
            {
                // Auto-reload if the player tries to fire an empty gun
                StartCoroutine(Reload());
            }
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        if (AudioManager.Instance != null && reloadSound != null)
        {
            AudioManager.Instance.PlaySFX(reloadSound, 1f);
        }

        // Wait for the duration of the reload (works with Za Warudo/TimeScale!)
        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
    }

    private void Shoot()
    {
        currentAmmo--; // Subtract one bullet per shot
        
        if (muzzleFlash != null) muzzleFlash.Play();
        if (AudioManager.Instance != null && shootSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(shootSound, 1f, 0.9f, 1.1f);
        }

        Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, maxRange))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100f); 
        }

        Vector3 directionToTarget = targetPoint - firePoint.position;

        if (bulletPrefab != null)
        {
            GameObject currentBullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            currentBullet.transform.forward = directionToTarget.normalized;

            Bullet bulletScript = currentBullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.Setup(directionToTarget.normalized, 50f);
            }
        }
    }
}