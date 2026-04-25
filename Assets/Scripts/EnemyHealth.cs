using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    public float health = 100f;
    public GameObject deathEffectPrefab; // We can use your FracturedCrate prefab here!

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log("Enemy hit! Health: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.EnemyDefeated();
        }
        if (deathEffectPrefab != null)
        {
            GameObject fractured = Instantiate(deathEffectPrefab, transform.position, transform.rotation);
            
            Rigidbody[] rbPieces = fractured.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rbPieces)
            {
                rb.AddExplosionForce(500f, transform.position, 5f);
            }
        }
        Destroy(gameObject);
    }
}

