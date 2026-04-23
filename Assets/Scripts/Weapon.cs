using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 50f;
    public float fireRate = 0.15f; // Time between shots

    [Header("Ammo Settings")]
    public int maxAmmo = 30;
    public float reloadTime = 1.5f;

    private int currentAmmo;
    private float nextTimeToFire = 0f;
    private bool isReloading = false;

    void Start()
    {
        currentAmmo = maxAmmo;
    }

    void Update()
    {
        // If we are currently reloading, ignore all other input
        if (isReloading)
            return;

        // Reload Input
        if (currentAmmo <= 0 || Input.GetKeyDown(KeyCode.R))
        {
            if (currentAmmo < maxAmmo)
            {
                StartCoroutine(Reload());
                return;
            }
        }

        // Shooting Input (Left Mouse Button)
        // Change to GetMouseButtonDown(0) if you want semi-auto instead of full-auto
        if (Input.GetMouseButton(0) && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        currentAmmo--;

        // 1. Spawn the bullet at the FirePoint's position and rotation
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // 2. Get the Rigidbody of the bullet and push it forward
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        // Wait for the reload time to finish (using unscaled time so it reloads normally in slow-mo)
        yield return new WaitForSecondsRealtime(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        Debug.Log("Reload Complete!");
    }
}