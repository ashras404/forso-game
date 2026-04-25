using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    [Header("Destruction Settings")]
    public GameObject fracturedPrefab; // Drag your 8-piece cube here
    public float explosionForce = 400f;
    public float explosionRadius = 3f;
    public float pieceLifespan = 5f; // How long until the debris disappears

    public void Shatter()
    {
        // 1. Spawn the broken version at our exact position and rotation
        GameObject fracturedObj = Instantiate(fracturedPrefab, transform.position, transform.rotation);

        // 2. Find all the Rigidbodies (the 8 little pieces) inside the broken version
        Rigidbody[] pieces = fracturedObj.GetComponentsInChildren<Rigidbody>();

        // 3. Apply an explosive blast to every single piece!
        foreach (Rigidbody rb in pieces)
        {
            // The explosion originates slightly below the center so pieces blow upwards
            Vector3 explosionCenter = transform.position - new Vector3(0, 0.5f, 0);
            rb.AddExplosionForce(explosionForce, explosionCenter, explosionRadius);
        }

        // 4. Tell the fractured object to clean itself up after a few seconds
        // (We destroy the root parent object so we don't lag the game with 1,000 pieces on the floor)
        Destroy(fracturedObj, pieceLifespan);

        // 5. Destroy this solid box
        Destroy(gameObject);
    }
}