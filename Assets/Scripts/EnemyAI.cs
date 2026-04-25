using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    // The three states our brain can be in
    public enum AIState { Patrol, Chase, Attack }

    [Header("Current AI State")]
    public AIState currentState = AIState.Patrol;

    [Header("Targeting")]
    public Transform player;
    public float chaseRange = 15f;
    public float attackRange = 8f;

    [Header("Shooting")]
    public GameObject enemyBulletPrefab;
    public Transform firePoint;
    public float fireRate = 2f; // Shoots every 2 seconds
    private float nextFireTime = 0f;

    [Header("Flying Settings")]
    public Transform droneModel;
    public float hoverHeight = 3f; // How high off the ground it should fly
    public float hoverSpeed = 2f;

    [Header("Patrol Settings")]
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (waypoints.Length > 0) agent.SetDestination(waypoints[0].position);
    }

    void Update()
    {
        // If we haven't linked the player yet, do nothing so we don't crash
        if (player == null) return;

        // Calculate the exact distance between the drone and the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 1. THE BRAIN: Decide which state we should be in based on distance
        if (distanceToPlayer <= attackRange)
        {
            currentState = AIState.Attack;
        }
        else if (distanceToPlayer <= chaseRange)
        {
            currentState = AIState.Chase;
        }
        else
        {
            currentState = AIState.Patrol;
        }

        // 2. THE BODY: Execute the action for our current state
        switch (currentState)
        {
            case AIState.Patrol:
                Patrol();
                break;
            case AIState.Chase:
                Chase();
                break;
            case AIState.Attack:
                Attack();
                break;
        }

        Vector3 currentPos = transform.position;

        // We use a Raycast to find exactly how far the ground is below us
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f))
        {
            float bobbing = Mathf.Sin(Time.time * hoverSpeed) * 0.2f;
            float finalHoverY = hoverHeight + bobbing;
            droneModel.localPosition = new Vector3(0, finalHoverY, 0);
        }
    }

    void Patrol()
    {
        agent.isStopped = false; // Make sure the drone is allowed to walk

        if (waypoints.Length == 0) return;

        if (agent.remainingDistance < 0.5f && !agent.pathPending)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    void Chase()
    {
        agent.isStopped = false;
        // Ignore waypoints, walk directly to wherever the player is right now!
        agent.SetDestination(player.position);
    }

    void Attack()
    {
        agent.isStopped = true;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToPlayer), Time.deltaTime * 5f);
        if (Time.timeScale >= 1f)
        {
            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                Shoot();
            }
        }
    }

    void Shoot()
    {
        if (enemyBulletPrefab == null || firePoint == null) return;

        // Aim slightly up so it targets the player's chest, not their feet on the floor
        Vector3 aimTarget = player.position + new Vector3(0, 1f, 0);
        Vector3 shootDirection = aimTarget - firePoint.position;

        // Spawn the orb
        GameObject orb = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.LookRotation(shootDirection));

        // Tell the orb to fly!
        EnemyBullet bulletScript = orb.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            bulletScript.Setup(shootDirection);
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}