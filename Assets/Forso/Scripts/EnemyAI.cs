using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum AIState { Patrol, Chase, Attack }

    #region Settings & References
    [Header("Current State")]
    public AIState currentState = AIState.Patrol;

    [Header("Targeting & Senses")]
    [Tooltip("If left empty, the script will auto-find the Player tag.")]
    public Transform player;
    public float chaseRange = 15f;
    public float attackRange = 8f;
    [Tooltip("What layers block the drone's vision? (e.g., Default, Walls)")]
    public LayerMask obstacleMask;

    [Header("Combat")]
    public GameObject enemyBulletPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    private float nextFireTime = 0f;

    [Header("Drone Animation")]
    public Transform droneModel;
    public float hoverIntensity = 0.2f; 
    public float hoverSpeed = 2f;
    private float startYPos;

    [Header("Patrol Settings")]
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;

    private NavMeshAgent agent;
    #endregion

    #region Initialization
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Auto-find player if you forget to drag them into the slot
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (droneModel != null) startYPos = droneModel.localPosition.y;
        if (waypoints.Length > 0) agent.SetDestination(waypoints[0].position);
    }
    #endregion

    #region Unity Lifecycle
    private void Update()
    {
        if (player == null) return;

        UpdateState();
        ExecuteState();
        AnimateHover();
    }
    #endregion

    #region AI Brain
    private void UpdateState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 1. Are we close enough to attack AND can we clearly see the player?
        if (distanceToPlayer <= attackRange && HasLineOfSight())
        {
            currentState = AIState.Attack;
        }
        // 2. We can't attack, but are we close enough to hear/chase them?
        else if (distanceToPlayer <= chaseRange)
        {
            currentState = AIState.Chase;
        }
        // 3. Player is gone, go back to guard duty
        else
        {
            currentState = AIState.Patrol;
        }
    }

    private void ExecuteState()
    {
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
    }

    private bool HasLineOfSight()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Shoot an invisible laser at the player. If it hits a wall first, return false.
        if (Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask))
        {
            return false; 
        }
        return true; 
    }
    #endregion

    #region Actions
    private void Patrol()
    {
        agent.isStopped = false;

        if (waypoints.Length == 0) return;

        if (agent.remainingDistance < 0.5f && !agent.pathPending)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    private void Chase()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    private void Attack()
    {
        agent.isStopped = true;

        // Smoothly rotate to look at the player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0; // Prevent the drone from tilting up/down weirdly
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        // Handle Shooting
        if (Time.timeScale >= 1f && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    private void Shoot()
    {
        if (enemyBulletPrefab == null || firePoint == null) return;

        Vector3 aimTarget = player.position + new Vector3(0, 1f, 0);
        Vector3 shootDirection = (aimTarget - firePoint.position).normalized;

        GameObject orb = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.LookRotation(shootDirection));
        
        EnemyBullet bulletScript = orb.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            bulletScript.Setup(shootDirection);
        }
    }
    #endregion

    #region Animation & Debugging
    private void AnimateHover()
    {
        if (droneModel == null) return;
        
        // Much faster performance: pure local math bobbing instead of a physics Raycast
        float bobbingOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverIntensity;
        droneModel.localPosition = new Vector3(droneModel.localPosition.x, startYPos + bobbingOffset, droneModel.localPosition.z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    #endregion
}