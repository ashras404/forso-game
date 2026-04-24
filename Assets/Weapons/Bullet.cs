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
        // Add "InParent" so the bullet checks the parent Hinge for the script!
        Target target = collision.gameObject.GetComponentInParent<Target>();

        if (target != null)
        {
            target.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}