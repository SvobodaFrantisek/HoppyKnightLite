using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float waypointTolerance = 0.4f;

    [Header("Combat")]
    public int maxHealth = 3;
    public float detectionRange = 8f;
    public float attackRange = 1.6f;
    public int attackDamage = 10;
    public float attackCooldown = 1f;

    [Header("Animation")]
    public Animator animator;
    public string speedParameter = "Speed";
    public string attackTrigger = "Attack";
    public string getHitTrigger = "GetHit";
    public float hitStunDuration = 0.2f;

    private NavMeshAgent agent;
    private Transform player;
    private GameManager gameManager;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private int currentHealth;
    private int currentPointIndex;
    private float nextAttackTime;
    private float hitStunEndTime;
    private bool isDead;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        gameManager = FindAnyObjectByType<GameManager>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        currentHealth = maxHealth;
    }

    void Start()
    {
        if (agent != null)
        {
            agent.autoBraking = false;
        }

        MoveToNextPatrolPoint();
    }

    void Update()
    {
        if (isDead || player == null || agent == null)
        {
            UpdateAnimatorSpeed(0f);
            return;
        }

        if (IsStunned())
        {
            agent.isStopped = true;
            UpdateAnimatorSpeed(0f);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer(distanceToPlayer);
        }
        else
        {
            Patrol();
        }

        UpdateAnimatorSpeed(agent.velocity.magnitude);
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
        {
            return;
        }

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        hitStunEndTime = Time.time + hitStunDuration;
        nextAttackTime = Mathf.Max(nextAttackTime, hitStunEndTime);
        agent.ResetPath();
        agent.isStopped = true;
        TriggerGetHitAnimation();
    }

    public void ResetEnemy()
    {
        if (isDead || agent == null)
        {
            return;
        }

        currentPointIndex = 0;
        nextAttackTime = 0f;
        transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        agent.Warp(spawnPosition);
        agent.ResetPath();
        agent.isStopped = false;
        agent.stoppingDistance = 0f;
        hitStunEndTime = 0f;
        ResetAnimatorState();
        MoveToNextPatrolPoint();
    }

    void Patrol()
    {
        agent.isStopped = false;
        agent.stoppingDistance = 0f;

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            agent.ResetPath();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
        {
            MoveToNextPatrolPoint();
        }
    }

    void ChasePlayer(float distanceToPlayer)
    {
        if (distanceToPlayer > attackRange)
        {
            agent.isStopped = false;
            agent.stoppingDistance = attackRange * 0.8f;
            agent.SetDestination(player.position);
            return;
        }

        agent.isStopped = true;
        FacePlayer();

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            TriggerAttackAnimation();

            if (gameManager != null)
            {
                gameManager.DamagePlayer(attackDamage);
            }
        }
    }

    void MoveToNextPatrolPoint()
    {
        if (agent == null || patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        agent.SetDestination(patrolPoints[currentPointIndex].position);
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
    }

    void FacePlayer()
    {
        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, agent.angularSpeed * Time.deltaTime);
    }

    void Die()
    {
        isDead = true;
        agent.ResetPath();
        agent.isStopped = true;
        ResetAnimatorState();
        gameObject.SetActive(false);
    }

    void UpdateAnimatorSpeed(float speed)
    {
        if (animator != null && !string.IsNullOrEmpty(speedParameter))
        {
            animator.SetFloat(speedParameter, speed);
        }
    }

    void TriggerAttackAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(attackTrigger))
        {
            animator.ResetTrigger(attackTrigger);
            animator.SetTrigger(attackTrigger);
        }
    }

    void TriggerGetHitAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(getHitTrigger))
        {
            animator.ResetTrigger(getHitTrigger);
            animator.SetTrigger(getHitTrigger);
        }
    }

    void ResetAnimatorState()
    {
        UpdateAnimatorSpeed(0f);

        if (animator != null && !string.IsNullOrEmpty(attackTrigger))
        {
            animator.ResetTrigger(attackTrigger);
        }

        if (animator != null && !string.IsNullOrEmpty(getHitTrigger))
        {
            animator.ResetTrigger(getHitTrigger);
        }
    }

    bool IsStunned()
    {
        return Time.time < hitStunEndTime;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
