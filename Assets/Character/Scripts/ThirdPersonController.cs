using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float rotationSpeed = 10f;

    [Header("Attack Settings")]
    [Tooltip("Tự động xoay về hướng camera khi bắt đầu tấn công")]
    public bool autoRotateTowardsCameraOnAttack = true;

    [Tooltip("Tốc độ xoay về hướng camera khi tấn công")]
    public float attackStartRotationSpeed = 15f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    [Header("References")]
    public ThirdPersonCamera cameraController;
    public Animator animator;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float gravity = -9.81f;

    // Input System
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private bool isSprinting;

    // References
    private AttackComboController attackController;
    private RollController rollController;
    private PlayerHealth playerHealth;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (controller == null)
        {
            Debug.LogError("❌ CharacterController not found!");
        }

        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<ThirdPersonCamera>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        attackController = GetComponent<AttackComboController>();
        if (attackController == null)
        {
            Debug.LogWarning("⚠ AttackComboController not found.");
        }
        else
        {
            attackController.OnAttackStart += HandleAttackStart;
        }

        rollController = GetComponent<RollController>();
        if (rollController == null)
        {
            Debug.LogWarning("⚠ RollController not found.");
        }

        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning("⚠ PlayerHealth not found.");
        }
    }

    void OnDestroy()
    {
        if (attackController != null)
        {
            attackController.OnAttackStart -= HandleAttackStart;
        }
    }

    void HandleAttackStart()
    {
        if (autoRotateTowardsCameraOnAttack && cameraController != null)
        {
            float cameraYaw = cameraController.GetCameraYaw();
            Vector3 cameraForward = Quaternion.Euler(0, cameraYaw, 0) * Vector3.forward;

            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                attackStartRotationSpeed * Time.deltaTime * 10f
            );

            Debug.DrawRay(transform.position + Vector3.up, cameraForward * 3f, Color.red, 1f);
        }
    }

    void Update()
    {
        ReadInput();
        HandleGroundCheck();
        HandleMovement();
        HandleGravity();
    }

    void ReadInput()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        isSprinting = inputActions.Player.Sprint.ReadValue<float>() > 0.5f;
    }

    void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    void HandleMovement()
    {
        bool isAttacking = attackController != null && attackController.IsAttacking();
        bool isRolling = rollController != null && rollController.IsRolling();

        // ✅ NEW: Check if in impact state
        bool isInImpact = playerHealth != null && playerHealth.IsInImpact();

        // ✅ NEW: Check if dead
        bool isDead = playerHealth != null && playerHealth.IsDead();

        // ✅ LOCK movement khi: attack, roll, impact, hoặc dead
        if (isAttacking || isRolling || isInImpact || isDead)
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", 0);
                animator.SetBool("IsMoving", false);
            }

            return; // ❌ KHÔNG di chuyển
        }

        Vector3 inputDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            float cameraYaw = cameraController != null ? cameraController.GetCameraYaw() : transform.eulerAngles.y;

            Vector3 cameraForward = Quaternion.Euler(0, cameraYaw, 0) * Vector3.forward;
            Vector3 cameraRight = Quaternion.Euler(0, cameraYaw, 0) * Vector3.right;

            Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

            Debug.DrawRay(transform.position + Vector3.up, moveDirection * 2f, Color.green);
            Debug.DrawRay(transform.position + Vector3.up, cameraForward * 2f, Color.blue);

            float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

            controller.Move(moveDirection * currentSpeed * Time.deltaTime);

            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            if (animator != null)
            {
                animator.SetFloat("Speed", currentSpeed);
                animator.SetBool("IsMoving", true);
            }
        }
        else
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", 0);
                animator.SetBool("IsMoving", false);
            }
        }
    }

    void HandleGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}