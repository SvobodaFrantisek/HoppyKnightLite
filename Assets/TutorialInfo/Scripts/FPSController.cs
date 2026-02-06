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
    public float groundedStickForce = 5f; // Zvýšeno pro lepší stabilitu na zemi
    public float jumpTimeout = 0.5f; // Ochrana proti dvojitému zmáčknutí

    [Header("Camera Settings (3rd Person)")]
    public Transform playerCamera;
    public Transform cameraTarget;
    public float cameraDistanceFromTarget = 5f;
    public float mouseSensitivity = 1f;
    public Vector2 pitchLimits = new Vector2(-40f, 70f);

    [Header("Animation")]
    public Animator animator;

    // Interní proměnné
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private CharacterController _cc;
    private Vector3 _velocity;
    private float _cameraYaw;
    private float _cameraPitch;

    // Časovač pro skok
    private float _jumpTimeoutDelta;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();

        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (cameraTarget == null) cameraTarget = transform;
    }

    void OnEnable()
    {
        _moveAction = _playerInput.actions.FindAction("Move");
        _lookAction = _playerInput.actions.FindAction("Look");
        _jumpAction = _playerInput.actions.FindAction("Jump");

        _moveAction?.Enable();
        _lookAction?.Enable();
        _jumpAction?.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        _moveAction?.Disable();
        _lookAction?.Disable();
        _jumpAction?.Disable();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        HandleMovement();
        HandleAnimations();
    }

    void LateUpdate()
    {
        HandleCameraOrbit();
    }

    void HandleMovement()
    {
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();

        // --- Gravitace a Skok ---
        if (_cc.isGrounded)
        {
            // Reset časovače timeoutu
            _jumpTimeoutDelta = 0.0f;

            // Udržení na zemi (negativní rychlost)
            if (_velocity.y < 0.0f)
            {
                _velocity.y = -groundedStickForce;
            }

            // Skok - POUZE když jsme na zemi
            if (_jumpAction.triggered)
            {
                _velocity.y = jumpSpeed;

                // Spuštění animace skoku
                if (animator != null) animator.SetTrigger("Jump");

                // Nastavíme timeout, aby nešlo skákat okamžitě znovu
                _jumpTimeoutDelta = jumpTimeout;
            }
        }
        else
        {
            // Jsme ve vzduchu - aplikujeme gravitaci
            _velocity.y -= gravity * Time.deltaTime;

            // Odečítáme časovač (pro jistotu, kdybychom chtěli přidat logiku později)
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }

        // --- Pohyb ---
        Vector3 camForward = playerCamera.forward;
        Vector3 camRight = playerCamera.right;
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

        if ((_cc.collisionFlags & CollisionFlags.Above) != 0)
        {
            // Pokud letíme nahoru a narazíme, okamžitě to zrušíme
            if (_velocity.y > 0)
            {
                _velocity.y = -2f; // Nastavíme malou rychlost dolů, abychom se odlepili
            }
        }

    }

        void HandleAnimations()
        {
            if (animator == null) return;

            float horizontalSpeed = new Vector3(_cc.velocity.x, 0, _cc.velocity.z).magnitude;
            animator.SetFloat("Speed", horizontalSpeed);
            animator.SetBool("IsGrounded", _cc.isGrounded);
        }

        void HandleCameraOrbit()
        {
            if (playerCamera == null || cameraTarget == null) return;

            Vector2 lookInput = _lookAction.ReadValue<Vector2>();
            _cameraYaw += lookInput.x * mouseSensitivity;
            _cameraPitch -= lookInput.y * mouseSensitivity;
            _cameraPitch = Mathf.Clamp(_cameraPitch, pitchLimits.x, pitchLimits.y);

            Quaternion camRotation = Quaternion.Euler(_cameraPitch, _cameraYaw, 0);
            Vector3 camPosition = cameraTarget.position + camRotation * new Vector3(0, 0, -cameraDistanceFromTarget);

            playerCamera.rotation = camRotation;
            playerCamera.position = camPosition;
        }
    }

