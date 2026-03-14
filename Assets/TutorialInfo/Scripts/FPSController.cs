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

    [Header("Dodge Settings")]
    public float dodgeDistance = 2.5f;
    public float dodgeDuration = 0.18f;
    public float dodgeCooldown = 0.6f;

    private bool wasClimbing = false;

    [Header("Animation")]
    public Animator animator;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dodgeAction;
    private CharacterController cc;
    private Vector3 velocity;

    private float jumpTimeoutDelta;
    private float dodgeTimer;
    private float dodgeCooldownTimer;
    private bool isDodging;
    private Vector3 dodgeDirection;

    private float DodgeSpeed => dodgeDuration > 0f ? dodgeDistance / dodgeDuration : 0f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        moveAction = playerInput.actions.FindAction("Move");
        jumpAction = playerInput.actions.FindAction("Jump");
        dodgeAction = playerInput.actions.FindAction("Sprint");

        moveAction?.Enable();
        jumpAction?.Enable();
        dodgeAction?.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
        dodgeAction?.Disable();

        if (animator != null)
        {
            animator.ResetTrigger("Dodge");
        }

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
        Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

        if (Cursor.lockState != CursorLockMode.Locked)
            moveInput = Vector2.zero;

        if (dodgeCooldownTimer > 0f)
            dodgeCooldownTimer -= Time.deltaTime;

        isClimbing = false;
        bool hitWall = false;
        Vector3 wallNormal = Vector3.zero;

        Vector3 wallCheckRay = transform.position + Vector3.up * 0.5f;
        Debug.DrawRay(wallCheckRay, transform.forward * wallCheckDistance, Color.red);

        if (Physics.Raycast(wallCheckRay, transform.forward, out RaycastHit hit, wallCheckDistance))
        {
            if (hit.collider.CompareTag("Climbable"))
            {
                hitWall = true;
                wallNormal = hit.normal;

                if (moveInput.y > 0 && !cc.isGrounded)
                    isClimbing = true;
            }
        }

        if (wasClimbing && !hitWall && moveInput.y > 0)
        {
            velocity.y = jumpSpeed * 0.6f;
        }

        wasClimbing = isClimbing;

        if (isClimbing)
        {
            jumpCount = 0;

            if (wallNormal != Vector3.zero)
            {
                Vector3 lookDir = -wallNormal;
                lookDir.y = 0f;

                if (lookDir != Vector3.zero)
                {
                    Quaternion targetWallRotation = Quaternion.LookRotation(lookDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetWallRotation, rotationSpeed * Time.deltaTime);
                }
            }

            velocity.y = climbSpeed;

            Vector3 direction = transform.right * moveInput.x;
            Vector3 finalDirection = direction * climbSpeed;
            finalDirection.y = velocity.y;

            cc.Move(finalDirection * Time.deltaTime);
        }
        else
        {
            if (cc.isGrounded)
            {
                jumpTimeoutDelta = 0.0f;
                jumpCount = 0;

                if (velocity.y < 0.0f)
                    velocity.y = -groundedStickForce;

                if (jumpAction != null && jumpAction.WasPressedThisFrame())
                {
                    velocity.y = jumpSpeed;
                    jumpCount++;

                    if (animator != null)
                        animator.SetTrigger("Jump");

                    jumpTimeoutDelta = jumpTimeout;
                }
            }
            else
            {
                velocity.y -= gravity * Time.deltaTime;

                if (jumpTimeoutDelta >= 0.0f)
                    jumpTimeoutDelta -= Time.deltaTime;

                if (jumpAction != null && jumpAction.WasPressedThisFrame() && hasDoubleJump && jumpCount < 2)
                {
                    velocity.y = jumpSpeed;
                    jumpCount++;

                    if (animator != null)
                        animator.SetTrigger("doublejump");
                }
            }

            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = (camForward * moveInput.y + camRight * moveInput.x).normalized;

            if (!isDodging &&
                dodgeAction != null &&
                dodgeAction.WasPressedThisFrame() &&
                cc.isGrounded &&
                dodgeCooldownTimer <= 0f)
            {
                isDodging = true;
                dodgeTimer = dodgeDuration;
                dodgeCooldownTimer = dodgeCooldown;

                dodgeDirection = moveDir != Vector3.zero ? moveDir : transform.forward;
                dodgeDirection.y = 0f;
                dodgeDirection.Normalize();

                if (animator != null)
                {
                    animator.ResetTrigger("Dodge");
                    animator.SetTrigger("Dodge");
                }
            }

            if (isDodging)
            {
                dodgeTimer -= Time.deltaTime;
                moveDir = dodgeDirection;

                if (dodgeTimer <= 0f)
                {
                    isDodging = false;

                    if (animator != null)
                        animator.ResetTrigger("Dodge");
                }
            }

            if (moveDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            float currentSpeed = isDodging ? DodgeSpeed : moveSpeed;

            Vector3 finalMove = moveDir * currentSpeed;
            finalMove.y = velocity.y;

            cc.Move(finalMove * Time.deltaTime);

            if ((cc.collisionFlags & CollisionFlags.Above) != 0)
            {
                if (velocity.y > 0)
                    velocity.y = -2f;
            }
        }
    }

    void HandleAnimations()
    {
        if (animator == null)
            return;

        float horizontalSpeed = new Vector3(cc.velocity.x, 0, cc.velocity.z).magnitude;

        animator.SetFloat("Speed", horizontalSpeed);
        animator.SetBool("IsGrounded", cc.isGrounded);
        animator.SetBool("IsClimbing", isClimbing);
    }
}