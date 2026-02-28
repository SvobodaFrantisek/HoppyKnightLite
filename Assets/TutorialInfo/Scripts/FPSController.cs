using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 10f;
    public float jumpSpeed = 8f;
    public float gravity = 20f;
    public float groundedStickForce = 5f;
    public float jumpTimeout = 0.5f;

    public bool hasDoubleJump = false;
    public int jumpCount = 0;

    public float climbSpeed = 4f;
    public float wallCheckDistance = 0.5f;
    public bool isClimbing = false;

    // Přidáno pro detekci výšvihu
    private bool _wasClimbing = false;

    [Header("Animation")]
    public Animator animator;

    // Interní proměnné
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private CharacterController _cc;
    private Vector3 _velocity;

    // Časovač pro skok
    private float _jumpTimeoutDelta;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();

        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        _moveAction = _playerInput.actions.FindAction("Move");
        _jumpAction = _playerInput.actions.FindAction("Jump");

        _moveAction?.Enable();
        _jumpAction?.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        _moveAction?.Disable();
        _jumpAction?.Disable();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        HandleMovement();
        HandleAnimations();
    }

    void HandleMovement()
    {
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();

        // Pojistka pro shop: Zastavení pohybu, pokud je odemčená myš
        if (Cursor.lockState != CursorLockMode.Locked) moveInput = Vector2.zero;

        isClimbing = false;
        bool hitWall = false; // Pojistka pro výšvih

        Vector3 wallCheckRay = transform.position + Vector3.up * 0.7f;
        Debug.DrawRay(wallCheckRay, transform.forward * wallCheckDistance, Color.red);

        if (Physics.Raycast(wallCheckRay, transform.forward, out RaycastHit hit, wallCheckDistance))
        {
            if (hit.collider.CompareTag("Climbable"))
            {
                hitWall = true;
                if (moveInput.y > 0 && !_cc.isGrounded)
                {
                    isClimbing = true;
                }
            }
        }

        // --- AUTOMATICKÝ VÝŠVIH ---
        // Pokud jsme lezli, ale teď už zeď nevidíme a pořád držíme W
        if (_wasClimbing && !hitWall && moveInput.y > 0)
        {
            _velocity.y = jumpSpeed * 0.7f; // Vymrštění nahoru
        }

        _wasClimbing = isClimbing; // Uložení stavu pro další snímek

        if (isClimbing)
        {
            jumpCount = 0;
            _velocity.y = climbSpeed;
            Vector3 direction = transform.right * moveInput.x;
            Vector3 finalDirection = direction * climbSpeed;

            finalDirection.y = _velocity.y;

            _cc.Move(finalDirection * Time.deltaTime);
            if (_jumpAction.triggered)
            {
                isClimbing = false;
                _velocity.y = jumpSpeed;
            }
        }
        else
        {
            // --- Gravitace a Skok ---
            if (_cc.isGrounded)
            {
                _jumpTimeoutDelta = 0.0f;
                jumpCount = 0;

                if (_velocity.y < 0.0f)
                {
                    _velocity.y = -groundedStickForce;
                }

                if (_jumpAction.triggered)
                {
                    _velocity.y = jumpSpeed;
                    jumpCount++;
                    if (animator != null) animator.SetTrigger("Jump");
                    _jumpTimeoutDelta = jumpTimeout;
                }
            }
            else
            {
                _velocity.y -= gravity * Time.deltaTime;

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }

                if (_jumpAction.triggered && hasDoubleJump && jumpCount < 2)
                {
                    _velocity.y = jumpSpeed;
                    jumpCount++;
                    if (animator != null) animator.SetTrigger("doublejump");
                }
            }

            // --- Pohyb napojený na Cinemachine ---
            // (Přesunuto sem, aby se to nepralo s pohybem při lezení)
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;

            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = (camForward * moveInput.y + camRight * moveInput.x).normalized;

            if (moveDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            Vector3 finalMove = moveDir * moveSpeed;
            finalMove.y = _velocity.y;

            _cc.Move(finalMove * Time.deltaTime);

            // Odraz od stropu
            if ((_cc.collisionFlags & CollisionFlags.Above) != 0)
            {
                if (_velocity.y > 0)
                {
                    _velocity.y = -2f;
                }
            }
        }
    }

    void HandleAnimations()
    {
        if (animator == null) return;

        float horizontalSpeed = new Vector3(_cc.velocity.x, 0, _cc.velocity.z).magnitude;
        animator.SetFloat("Speed", horizontalSpeed);
        animator.SetBool("IsGrounded", _cc.isGrounded);
        animator.SetBool("IsClimbing", isClimbing);
    }
}