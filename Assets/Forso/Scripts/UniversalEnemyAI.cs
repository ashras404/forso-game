using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UniversalEnemyAI : MonoBehaviour
{
    #region AI Configurations (Dropdowns)
    public enum MovementStyle { FreeWander, Waypoints }
    public enum CombatStyle { Melee, RangedShooter }
    public enum AIState { Patrolling, Chasing, Attacking }

    [Header("--- AI Core Directives ---")]
    public MovementStyle movementStyle = MovementStyle.FreeWander;
    public CombatStyle combatStyle = CombatStyle.Melee;
    
    [Header("Current State (Read Only)")]
    public AIState currentState = AIState.Patrolling;
    #endregion

    #region Targeting & Senses
    [Header("--- Senses & Ranges ---")]
    [Tooltip("How close to spot the player.")]
    public float detectionRange = 15f;
    [Tooltip("How close to attack. (Use 2.5 for Melee, 10+ for Ranged)")]
    public float attackRange = 2.5f;
    [Tooltip("What layers block vision? (e.g., Walls)")]
    public LayerMask obstacleMask;
    private Transform player;
    #endregion

    #region Movement Settings
    [Header("--- Movement: Free Wander ---")]
    public float roamRadius = 15f;
    public float waitAtDestination = 2f;
    private float waitTimer;
    private Vector3 startingPosition;

    [Header("--- Movement: Waypoints ---")]
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;

    private NavMeshAgent agent;
    private float baseSpeed;
    #endregion

    #region Combat Settings
    [Header("--- Combat Base ---")]
    public float timeBetweenAttacks = 1.5f;
    private float attackTimer;

    [Header("Melee Settings")]
    public float meleeDamage = 20f;

    [Header("Ranged Settings")]
    public GameObject enemyBulletPrefab;
    public Transform firePoint;
    #endregion

    #region Visuals & Animation
    [Header("--- Visuals & Animation ---")]
    [Tooltip("Assign the visual mesh here if you want it to hover (like a drone). Leave empty if not!")]
    public Transform hoverModel;
    public float hoverIntensity = 0.2f;
    public float hoverSpeed = 2f;
    private float startYPos;
    #endregion

    #region Initialization
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        baseSpeed = agent.speed;
        startingPosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (hoverModel != null) startYPos = hoverModel.localPosition.y;

        // Initialize starting movement
        if (movementStyle == MovementStyle.FreeWander)
            PickNewRoamDestination();
        else if (movementStyle == MovementStyle.Waypoints && waypoints.Length > 0)
            agent.SetDestination(waypoints[0].position);
    }
    #endregion

    #region Main Logic Loop
    private void Update()
    {
        if (player == null || UIManager.GameIsPaused)
        {
            agent.isStopped = true;
            return;
        }

        // AAA Polish: Za Warudo Time Scaling!
        agent.speed = baseSpeed * Time.timeScale;
        if (Time.timeScale == 0f) return;

        UpdateState();
        ExecuteState();
        
        if (hoverModel != null) AnimateHover();
    }
    #endregion

    #region AI Brain
    private void UpdateState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Can we attack? (Must be in range AND have line of sight)
        if (distanceToPlayer <= attackRange && HasLineOfSight())
        {
            currentState = AIState.Attacking;
        }
        // Can we see them to chase?
        else if (distanceToPlayer <= detectionRange && HasLineOfSight())
        {
            currentState = AIState.Chasing;
        }
        // Lost them, go back to patrolling
        else if (distanceToPlayer > detectionRange * 1.5f || !HasLineOfSight()) 
        {
            currentState = AIState.Patrolling;
        }
    }

    private void ExecuteState()
    {
        switch (currentState)
        {
            case AIState.Patrolling:
                if (movementStyle == MovementStyle.FreeWander) HandleRoaming();
                else HandleWaypoints();
                break;
            case AIState.Chasing:
                HandleChasing();
                break;
            case AIState.Attacking:
                HandleAttacking();
                break;
        }
    }

    private bool HasLineOfSight()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Raycast checks if a wall is in the way
        if (Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask))
        {
            return false;
        }
        return true;
    }
    #endregion

    #region Movement Behaviors
    private void HandleRoaming()
    {
        agent.isStopped = false;

        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                PickNewRoamDestination();
                waitTimer = waitAtDestination;
            }
        }
    }

    private void PickNewRoamDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
        randomDirection += startingPosition;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, roamRadius, 1))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void HandleWaypoints()
    {
        agent.isStopped = false;
        if (waypoints.Length == 0) return;

        if (agent.remainingDistance < 0.5f && !agent.pathPending)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    private void HandleChasing()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }
    #endregion

    #region Combat Behaviors
    private void HandleAttacking()
    {
        agent.isStopped = true;

        // Look directly at the player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0; 
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToPlayer), Time.deltaTime * 5f);

        // Attack Timer
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            PerformAttack();
            attackTimer = timeBetweenAttacks;
        }
    }

    private void PerformAttack()
    {
        if (combatStyle == CombatStyle.Melee)
        {
            Debug.Log(gameObject.name + " performs Melee Attack!");
            if (PlayerHealth.Instance != null) PlayerHealth.Instance.TakeDamage(meleeDamage);
        }
        else if (combatStyle == CombatStyle.RangedShooter)
        {
            Debug.Log(gameObject.name + " shoots a projectile!");
            if (enemyBulletPrefab != null && firePoint != null)
            {
                Vector3 aimTarget = player.position + new Vector3(0, 1f, 0); // Aim at chest, not feet
                Vector3 shootDirection = (aimTarget - firePoint.position).normalized;

                GameObject orb = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.LookRotation(shootDirection));
                EnemyBullet bulletScript = orb.GetComponent<EnemyBullet>();
                if (bulletScript != null) bulletScript.Setup(shootDirection);
            }
        }
    }
    #endregion

    #region Utilities
    private void AnimateHover()
    {
        float bobbingOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverIntensity;
        hoverModel.localPosition = new Vector3(hoverModel.localPosition.x, startYPos + bobbingOffset, hoverModel.localPosition.z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    #endregion
}
