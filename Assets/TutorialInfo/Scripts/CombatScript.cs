using UnityEngine;
using UnityEngine.InputSystem;

public class CombatScript : MonoBehaviour
{
   
    public float attackCooldown = 0.8f;
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
    }

   

    public void TrailOn()
    {
        if (traileffect != null) traileffect.SetActive(true);
    }

    public void TrailOff()
    {
        if (traileffect != null) traileffect.SetActive(false);
    }
}
