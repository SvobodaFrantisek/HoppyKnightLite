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
    // Portal / finish sekvence muze input zamknout, ale controller porad bezi kvuli gravitaci.
    public bool inputLocked = false;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dodgeAction;
    private CharacterController cc;
    private MovingIsland currentMovingIsland;
    private float animationSpeed;
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

    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;

        if (locked)
        {
            // Kdyz zacne finish sekvence, ukoncime aktivni dodge,
            // aby se hrac dal nehybal bokem behem animace.
            isDodging = false;
            dodgeTimer = 0f;

            if (animator != null)
                animator.ResetTrigger("Dodge");
        }
    }

    public void ResetMovementState()
    {
        velocity = Vector3.zero;
        jumpCount = 0;
        jumpTimeoutDelta = 0f;
        dodgeTimer = 0f;
        dodgeCooldownTimer = 0f;
        isDodging = false;
        isClimbing = false;
        wasClimbing = false;
        inputLocked = false;
        dodgeDirection = Vector3.zero;
        currentMovingIsland = null;
        animationSpeed = 0f;

        if (animator != null)
        {
            animator.ResetTrigger("Jump");
            animator.ResetTrigger("doublejump");
            animator.ResetTrigger("Dodge");
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsClimbing", false);
        }
    }

    void HandleMovement()
    {
        Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        Vector3 platformDisplacement = GetPlatformDisplacement();

        // Pri shopu nebo finish sekvenci nechceme brat movement input,
        // ale ostatni cast controlleru musi bezet dal.
        if (inputLocked || Cursor.lockState != CursorLockMode.Locked)
            moveInput = Vector2.zero;

        float inputMagnitude = Mathf.Clamp01(moveInput.magnitude);

        if (dodgeCooldownTimer > 0f)
            dodgeCooldownTimer -= Time.deltaTime;

        isClimbing = false;
        bool hitWall = false;
        Vector3 wallNormal = Vector3.zero;

        // Ray pred hracem hledame jen kvuli climbable stenam.
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
            // Kdyz hrac leze a najednou "preleze" hranu, lehce ho postrcime nahoru.
            velocity.y = jumpSpeed * 0.6f;
        }

        wasClimbing = isClimbing;

        if (isClimbing)
        {
            // Pri lezeni resetujeme skoky a pohyb resime zvlast od normalniho ground/air movementu.
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

            // Na stene povolime jen horizontalni posun po jeji hrane + vertikalni climb.
            Vector3 direction = transform.right * moveInput.x;
            Vector3 finalDirection = direction * climbSpeed;
            finalDirection.y = velocity.y;

            animationSpeed = inputMagnitude > 0.01f ? climbSpeed : 0f;
            cc.Move(platformDisplacement + finalDirection * Time.deltaTime);
            RefreshGroundedPlatform();
        }
        else
        {
            // Tady zacina bezny pohyb: zem / vzduch / dodge.
            if (cc.isGrounded)
            {
                jumpTimeoutDelta = 0.0f;
                jumpCount = 0;

                if (velocity.y < 0.0f)
                    velocity.y = -groundedStickForce;

                if (!inputLocked && jumpAction != null && jumpAction.WasPressedThisFrame())
                {
                    // Prvni skok ze zeme.
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

                if (!inputLocked && jumpAction != null && jumpAction.WasPressedThisFrame() && hasDoubleJump && jumpCount < 2)
                {
                    // Druhy skok ve vzduchu.
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

            // Pohyb je relativni ke kamere, ne ke svetovym osam.
            Vector3 moveDir = (camForward * moveInput.y + camRight * moveInput.x).normalized;

            if (!isDodging &&
                !inputLocked &&
                dodgeAction != null &&
                dodgeAction.WasPressedThisFrame() &&
                cc.isGrounded &&
                dodgeCooldownTimer <= 0f)
            {
                // Dodge je kratky burst do aktualniho movement smeru.
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
                // Po dobu dodge ignorujeme normalni movement a tlacime hrace jen po dodge smeru.
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
            animationSpeed = isDodging ? currentSpeed : inputMagnitude * currentSpeed;

            Vector3 finalMove = moveDir * currentSpeed;
            finalMove.y = velocity.y;

            cc.Move(platformDisplacement + finalMove * Time.deltaTime);
            RefreshGroundedPlatform();

            if ((cc.collisionFlags & CollisionFlags.Above) != 0)
            {
                if (velocity.y > 0)
                    velocity.y = -2f;
            }
        }
    }

    Vector3 GetPlatformDisplacement()
    {
        if (currentMovingIsland == null)
            return Vector3.zero;

        // Kdyz hrac aktivne leti nahoru po skoku, platforma ho uz nema tahat s sebou.
        if (velocity.y > 0.05f)
            return Vector3.zero;

        return currentMovingIsland.FrameDelta;
    }

    void RefreshGroundedPlatform()
    {
        if (!cc.enabled)
        {
            currentMovingIsland = null;
            return;
        }

        if (!cc.isGrounded && velocity.y > 0.05f)
        {
            currentMovingIsland = null;
            return;
        }

        Vector3 castOrigin = transform.TransformPoint(cc.center) + Vector3.up * 0.05f;
        float castRadius = Mathf.Max(0.05f, cc.radius - cc.skinWidth);
        float castDistance = Mathf.Max(0.3f, (cc.height * 0.5f) - castRadius + cc.skinWidth + 0.25f);

        if (Physics.SphereCast(castOrigin, castRadius, Vector3.down, out RaycastHit hit, castDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider != cc)
            {
                currentMovingIsland = hit.collider.GetComponentInParent<MovingIsland>();
                return;
            }
        }

        currentMovingIsland = null;
    }

    void HandleAnimations()
    {
        if (animator == null)
            return;

        if (inputLocked)
        {
            // Behem finish sekvence nechceme, aby locomotion parametry prepisovaly Win animaci.
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsGrounded", true);
            animator.SetBool("IsClimbing", false);
            return;
        }

        animator.SetFloat("Speed", animationSpeed);
        animator.SetBool("IsGrounded", cc.isGrounded);
        animator.SetBool("IsClimbing", isClimbing);
    }
}
