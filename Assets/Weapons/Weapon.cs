using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Weapon : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Light muzzleLight;
    public float bulletForce = 50f;
    public float fireRate = 0.15f;

    [Header("Ammo Settings")]
    public int maxAmmo = 30;
    public float reloadTime = 1.5f;

    [Header("Polish (Audio & Visuals)")]
    public ParticleSystem muzzleFlash;
    public AudioClip shootSound;
    public AudioClip reloadSound;

    private int currentAmmo;
    private float nextTimeToFire = 0f;
    private bool isReloading = false;

    private CinemachineImpulseSource impulseSource;
    private WeaponSway weaponSway;

    void Start()
    {
        currentAmmo = maxAmmo;
        impulseSource = GetComponent<CinemachineImpulseSource>();
        weaponSway = GetComponent<WeaponSway>();
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
            // Vary pitch slightly so the gun doesn't sound like a machine repeating the exact same file
            float originalPitch = AudioManager.Instance.sfxSource.pitch;
            AudioManager.Instance.sfxSource.pitch = Random.Range(0.9f, 1.1f);
            AudioManager.Instance.PlaySFX(shootSound, 0.8f);
            AudioManager.Instance.sfxSource.pitch = originalPitch;
        }

        // 2. Spawn bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);
        }

        // 3. Recoil & Shake
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
        yield return new WaitForSeconds(0.05f); // Flash for just a split second
        muzzleLight.enabled = false;
    }
}