using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Weapon : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 50f;
    public float fireRate = 0.15f; 

    [Header("Ammo Settings")]
    public int maxAmmo = 30;
    public float reloadTime = 1.5f;

    [Header("Polish (Audio & Visuals)")]
    public ParticleSystem muzzleFlash;
    public Light muzzleLight;
    public AudioClip shootSound;
    public AudioClip reloadSound;

    private int currentAmmo;
    private float nextTimeToFire = 0f;
    private bool isReloading = false;

    private CinemachineImpulseSource impulseSource;
    private WeaponSway weaponSway;
    private Camera mainCam; // Added to find the true screen center!

    void Start()
    {
        currentAmmo = maxAmmo;
        impulseSource = GetComponent<CinemachineImpulseSource>();
        weaponSway = GetComponent<WeaponSway>();
        mainCam = Camera.main; // Grab the player's main camera
    }

    void Update()
    {
        if (isReloading) return;

        if (currentAmmo <= 0 || Input.GetKeyDown(KeyCode.R))
        {
            if (currentAmmo < maxAmmo)
            {
                StartCoroutine(Reload());
                return;
            }
        }

        if (Input.GetMouseButton(0) && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        currentAmmo--;

        // 1. Visuals & Audio
        if (muzzleFlash != null) muzzleFlash.Play();
        if (muzzleLight != null) StartCoroutine(FlashLightRoutine());
        
        if (shootSound != null && AudioManager.Instance != null)
        {
            float originalPitch = AudioManager.Instance.sfxSource.pitch;
            AudioManager.Instance.sfxSource.pitch = Random.Range(0.9f, 1.1f);
            AudioManager.Instance.PlaySFX(shootSound, 0.8f);
            AudioManager.Instance.sfxSource.pitch = originalPitch;
        }

        // --- THE FIX: TRUE AIM MATH ---
        
        // Find the exact center of the screen
        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;

        // If the invisible laser hits something, that's our target.
        // If it hits the sky/nothing, pick a point super far away.
        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100); 
        }

        // Calculate the exact direction from the gun barrel to the target point
        Vector3 trueDirection = (targetPoint - firePoint.position).normalized;

        // ------------------------------

        // 2. Spawn bullet and push it in the True Direction!
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(trueDirection));
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(trueDirection * bulletForce, ForceMode.Impulse);
        }

        // 3. Recoil & Shake (Visual only now, won't mess up bullets!)
        if (impulseSource != null) impulseSource.GenerateImpulse();
        if (weaponSway != null) weaponSway.TriggerRecoil();
    }

    IEnumerator Reload()
    {
        isReloading = true;
        
        if (reloadSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(reloadSound, 1f);
        }

        yield return new WaitForSecondsRealtime(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    IEnumerator FlashLightRoutine()
    {
        muzzleLight.enabled = true;
        yield return new WaitForSeconds(0.05f); 
        muzzleLight.enabled = false;
    }
}