using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f; // How long before the bullet deletes itself
    public float damage = 20f;

    void Start()
    {
        // Destroy this object after 'lifeTime' seconds
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Here is where we will eventually deal damage to enemies
        // Example: collision.gameObject.GetComponent<Enemy>().TakeDamage(damage);

        // Destroy the bullet when it hits something
        //Destroy(gameObject);
    }
}