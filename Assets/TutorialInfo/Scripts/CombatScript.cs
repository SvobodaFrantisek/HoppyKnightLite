using UnityEngine;
using UnityEngine.InputSystem;

public class CombatScript : MonoBehaviour
{
    [Header("Nastavení")]
    public float attackCooldown = 0.8f;   // Jak èasto mùže sekat (sekundy)
    public Animator animator;             // Odkaz na animátor

    // Interní promìnné
    private PlayerInput _playerInput;
    private InputAction _attackAction;
    private float _lastAttackTime;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        // Automaticky najdeme Animator, pokud není pøiøazený
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        // Najdeme akci "Attack" v Input Systemu
        // Ujistìte se, že máte v Input Actions akci pojmenovanou "Attack"
        _attackAction = _playerInput.actions.FindAction("Attack");
        _attackAction?.Enable();
    }

    void OnDisable()
    {
        _attackAction?.Disable();
    }

    void Update()
    {
        // Pokud hráè zmáèkl útok A ubìhl èas cooldownu
        if (_attackAction.triggered && Time.time >= _lastAttackTime + attackCooldown)
        {
            PerformSlash();
            _lastAttackTime = Time.time;
        }
    }

    void PerformSlash()
    {
        // Spustíme animaci seknutí
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }
}
