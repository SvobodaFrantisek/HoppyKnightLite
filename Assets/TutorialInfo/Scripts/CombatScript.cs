using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CombatScript : MonoBehaviour
{
    public float attackCooldown = 0.8f;
    public int attackDamage = 1;
    public float attackDistance = 1.3f;
    public float attackRadius = 0.8f;
    public Transform attackOrigin;
    public LayerMask attackLayers = ~0;
    public Animator animator;

    public GameObject traileffect;

    private PlayerInput _playerInput;
    private InputAction _attackAction;
    private float _lastAttackTime;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (traileffect != null) traileffect.SetActive(false);
    }

    void OnEnable()
    {
        _attackAction = _playerInput.actions.FindAction("Attack");
        _attackAction?.Enable();
    }

    void OnDisable()
    {
        _attackAction?.Disable();
    }

    void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;
        if (_attackAction != null && _attackAction.triggered && CanAttack())
        {
            Attack();
        }
    }

    bool CanAttack()
    {
        return Time.time >= _lastAttackTime + attackCooldown;
    }

    void Attack()
    {
        _lastAttackTime = Time.time;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        PerformAttackHit();
    }

    void PerformAttackHit()
    {
        Vector3 origin = attackOrigin != null ? attackOrigin.position : transform.position + Vector3.up;
        Vector3 hitCenter = origin + transform.forward * attackDistance;
        Collider[] hits = Physics.OverlapSphere(hitCenter, attackRadius, attackLayers, QueryTriggerInteraction.Ignore);
        HashSet<EnemyController> damagedEnemies = new HashSet<EnemyController>();

        foreach (Collider hit in hits)
        {
            EnemyController enemy = hit.GetComponentInParent<EnemyController>();

            if (enemy != null && damagedEnemies.Add(enemy))
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }

    public void TrailOn()
    {
        if (traileffect != null) traileffect.SetActive(true);
    }

    public void TrailOff()
    {
        if (traileffect != null) traileffect.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 origin = attackOrigin != null ? attackOrigin.position : transform.position + Vector3.up;
        Vector3 hitCenter = origin + transform.forward * attackDistance;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitCenter, attackRadius);
    }
}
