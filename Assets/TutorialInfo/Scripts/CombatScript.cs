using UnityEngine;
using UnityEngine.InputSystem;

public class CombatScript : MonoBehaviour
{
    [Header("Nastavení útoku")]
    public float attackCooldown = 0.8f;
    public Animator animator;

    [Header("VFX")]
    public GameObject traileffect; // empty object se TrailRendererem (na špièce meèe)

    private PlayerInput _playerInput;
    private InputAction _attackAction;
    private float _lastAttackTime;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        // pro jistotu a je trail na zaèátku vypnutý
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

    // === Tohle volej z Animation Eventù v animaci Attack ===

    public void TrailOn()
    {
        if (traileffect != null) traileffect.SetActive(true);
    }

    public void TrailOff()
    {
        if (traileffect != null) traileffect.SetActive(false);
    }
}
