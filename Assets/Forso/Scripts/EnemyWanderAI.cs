using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyWanderAI : MonoBehaviour
{
    #region AI States
    public enum AIState { Roaming, Chasing, Attacking }
    [Header("Current State")]
    public AIState currentState = AIState.Roaming;
    #endregion

    #region Settings
    [Header("Roaming Settings")]
    [Tooltip("How far the enemy will wander from its starting point.")]
    public float roamRadius = 15f;
    [Tooltip("How long it waits at a spot before wandering again.")]
    public float waitAtDestination = 2f;
    private float waitTimer;
    private Vector3 startingPosition;

    [Header("Targeting Settings")]
    [Tooltip("How close you must be for it to spot you.")]
    public float detectionRange = 10f;
    [Tooltip("How close it needs to be to hit you.")]
    public float attackRange = 2.5f;

    [Header("Attack Settings")]
    [Tooltip("Damage dealt per hit.")]
    public float attackDamage = 20f;
    [Tooltip("Seconds between each attack.")]
    public float timeBetweenAttacks = 1.5f;
    private float attackTimer;
    #endregion

    #region References
    private NavMeshAgent agent;
    private Transform player;
    private float baseSpeed;
    #endregion

    #region Initialization
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        baseSpeed = agent.speed; // Remember its normal speed for slow-mo math
        startingPosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // Give it a random starting destination right away
        PickNewRoamDestination();
    }
    #endregion

    #region Main Logic Loop
    private void Update()
    {
        // 1. Safety Checks (Don't run if game is paused or player is missing)
        if (player == null || UIManager.GameIsPaused)
        {
            agent.isStopped = true;
            return;
        }

        // 2. AAA Polish: Scale agent speed based on Time.timeScale (Za Warudo Support!)
        agent.speed = baseSpeed * Time.timeScale;

        // If time is completely frozen, stop calculating AI to save performance
        if (Time.timeScale == 0f) return;

        // 3. Distance Math
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 4. The State Machine Brain
        switch (currentState)
        {
            case AIState.Roaming:
                HandleRoaming(distanceToPlayer);
                break;
            case AIState.Chasing:
                HandleChasing(distanceToPlayer);
                break;
            case AIState.Attacking:
                HandleAttacking(distanceToPlayer);
                break;
        }
    }
    #endregion

    #region State Behaviors
    private void HandleRoaming(float distanceToPlayer)
    {
        agent.isStopped = false;

        // Did we spot the player?
        if (distanceToPlayer <= detectionRange)
        {
            currentState = AIState.Chasing;
            return;
        }

        // Have we reached our random destination?
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                PickNewRoamDestination();
                waitTimer = waitAtDestination; // Reset timer for the next stop
            }
        }
    }

    private void HandleChasing(float distanceToPlayer)
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);

        // Are we close enough to attack?
        if (distanceToPlayer <= attackRange)
        {
            currentState = AIState.Attacking;
        }
        // Did the player run away and escape?
        else if (distanceToPlayer > detectionRange * 1.5f) // Adds a little buffer so it doesn't instantly forget you
        {
            currentState = AIState.Roaming;
            PickNewRoamDestination();
        }
    }

    private void HandleAttacking(float distanceToPlayer)
    {
        // Stop moving and look at the player!
        agent.isStopped = true;

        Vector3 lookPos = player.position - transform.position;
        lookPos.y = 0; // Don't tilt up/down
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos), Time.deltaTime * 5f);

        // Attack Timer
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            PerformAttack();
            attackTimer = timeBetweenAttacks;
        }

        // If the player backs away, chase them again!
        if (distanceToPlayer > attackRange)
        {
            currentState = AIState.Chasing;
        }
    }
    #endregion

    #region Actions
    private void PickNewRoamDestination()
    {
        // Pick a random point in a sphere around the start position
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
        randomDirection += startingPosition;

        // Ask the NavMesh to find the closest valid walking spot to that random point
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, roamRadius, 1))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void PerformAttack()
    {
        Debug.Log("Enemy swings at the player for " + attackDamage + " damage!");
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.TakeDamage(attackDamage);
        }
    }

    // This draws colorful rings in the editor so you can easily see its ranges!
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    #endregion
}