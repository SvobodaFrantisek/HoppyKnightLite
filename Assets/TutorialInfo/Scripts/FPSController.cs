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
    private bool wasClimbing = false;

    [Header("Animation")]
    public Animator animator;

    // Interní proměnné
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private CharacterController cc;
    private Vector3 velocity;

    // Časovač pro skok
    private float jumpTimeoutDelta;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        moveAction = playerInput.actions.FindAction("Move");
        jumpAction = playerInput.actions.FindAction("Jump");

        moveAction?.Enable();
        jumpAction?.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        moveAction?.Disable();
        jumpAction?.Disable();

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
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        // Pojistka pro shop: Zastavení pohybu, pokud je odemčená myš
        if (Cursor.lockState != CursorLockMode.Locked) moveInput = Vector2.zero;

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
                wallNormal = hit.normal; // Uložíme si úhel zdi

                if (moveInput.y > 0 && !cc.isGrounded)
                {
                    isClimbing = true;
                }
            }
        }

        // --- AUTOMATICKÝ VÝŠVIH ---
        // Pokud jsme lezli, ale teď už zeď nevidíme a pořád držíme W
        if (wasClimbing && !hitWall && moveInput.y > 0)
        {
            velocity.y = jumpSpeed * 0.6f; // Vymrštění nahoru
        }

        wasClimbing = isClimbing; // Uložení stavu pro další snímek

        if (isClimbing)
        {
            jumpCount = 0;

            // --- PŘIDÁNO: NATOČENÍ ČELEM KE ZDI ---
            if (wallNormal != Vector3.zero)
            {
                Vector3 lookDir = -wallNormal;
                lookDir.y = 0; // Zabráníme naklánění nahoru/dolů
                Quaternion targetWallRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetWallRotation, rotationSpeed * Time.deltaTime);
            }
            

            velocity.y = climbSpeed;
            Vector3 direction = transform.right * moveInput.x;
            Vector3 finalDirection = direction * climbSpeed;

            finalDirection.y = velocity.y;

            cc.Move(finalDirection * Time.deltaTime);
            if (jumpAction.triggered)
            {
                
            }
        }
        else
        {
            // --- Gravitace a Skok ---
            if (cc.isGrounded)
            {
                jumpTimeoutDelta = 0.0f;
                jumpCount = 0;

                if (velocity.y < 0.0f)
                {
                    velocity.y = -groundedStickForce;
                }

                if (jumpAction.triggered)
                {
                    velocity.y = jumpSpeed;
                    jumpCount++;
                    if (animator != null) animator.SetTrigger("Jump");
                    jumpTimeoutDelta = jumpTimeout;
                }
            }
            else
            {
                velocity.y -= gravity * Time.deltaTime;

                if (jumpTimeoutDelta >= 0.0f)
                {
                    jumpTimeoutDelta -= Time.deltaTime;
                }

                if (jumpAction.triggered && hasDoubleJump && jumpCount < 2)
                {
                    velocity.y = jumpSpeed;
                    jumpCount++;
                    if (animator != null) animator.SetTrigger("doublejump");
                }
            }

            // --- Pohyb napojený na Cinemachine ---
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
            finalMove.y = velocity.y;

            cc.Move(finalMove * Time.deltaTime);

            // Odraz od stropu
            if ((cc.collisionFlags & CollisionFlags.Above) != 0)
            {
                if (velocity.y > 0)
                {
                    velocity.y = -2f;
                }
            }
        }
    }

    void HandleAnimations()
    {
        if (animator == null) return;

        float horizontalSpeed = new Vector3(cc.velocity.x, 0, cc.velocity.z).magnitude;
        animator.SetFloat("Speed", horizontalSpeed);
        animator.SetBool("IsGrounded", cc.isGrounded);
        animator.SetBool("IsClimbing", isClimbing);
    }
}