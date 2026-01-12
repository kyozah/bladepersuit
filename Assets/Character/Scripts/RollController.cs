using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class RollController : MonoBehaviour
{
    [Header("Roll Settings")]
    [Tooltip("Khoảng cách roll")]
    public float rollDistance = 4f;

    [Tooltip("Thời gian roll animation")]
    public float rollDuration = 0.6f;

    [Tooltip("Cooldown giữa các lần roll (seconds)")]
    public float rollCooldown = 1f;

    [Header("I-Frames (Invincibility)")]
    [Tooltip("Thời điểm bắt đầu i-frames (normalized time 0-1)")]
    public float iFrameStart = 0.2f;

    [Tooltip("Thời điểm kết thúc i-frames (normalized time 0-1)")]
    public float iFrameEnd = 0.7f;

    [Tooltip("Layer để ignore damage khi rolling")]
    public LayerMask invincibleLayer;

    [Header("Stamina (Optional)")]
    [Tooltip("Stamina cost cho roll")]
    public float staminaCost = 20f;

    [Tooltip("Reference đến stamina system (nếu có)")]
    public bool useStamina = false;

    [Header("References")]
    public Animator animator;
    public Transform cameraTransform;

    private CharacterController characterController;
    private Rigidbody rb;
    private bool isRolling = false;
    private bool canRoll = true;
    private float rollTimer = 0f;
    private Vector3 rollDirection;
    private float rollSpeed;
    private bool isInvincible = false;
    private int originalLayer;

    // Input System
    private PlayerInputActions inputActions;
    private Vector2 moveInput;

    // References to other systems
    private AttackComboController attackController;
    private PlayerHealth playerHealth;

    // Coroutines
    private Coroutine iFramesCoroutine;
    private Coroutine cooldownCoroutine;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Roll.performed += OnRollInput;
    }

    void OnDisable()
    {
        inputActions.Player.Roll.performed -= OnRollInput;
        inputActions.Player.Disable();
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        attackController = GetComponent<AttackComboController>();
        playerHealth = GetComponent<PlayerHealth>();

        originalLayer = gameObject.layer;

        Debug.Log("✅ RollController initialized");
    }

    void Update()
    {
        ReadInput();
        HandleRollMovement();
    }

    void ReadInput()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
    }

    void OnRollInput(InputAction.CallbackContext context)
    {
        if (canRoll && !isRolling)
        {
            // ✅ CHECK: Không thể roll khi đang attack
            if (attackController != null && attackController.IsAttacking())
            {
                Debug.Log("❌ Cannot roll during attack!");
                return;
            }

            // ✅ NEW: Không thể roll khi đang impact
            if (playerHealth != null && playerHealth.IsInImpact())
            {
                Debug.Log("❌ Cannot roll during impact!");
                return;
            }

            // ✅ NEW: Không thể roll khi đã chết
            if (playerHealth != null && playerHealth.IsDead())
            {
                Debug.Log("❌ Cannot roll when dead!");
                return;
            }

            // Check stamina
            if (useStamina && !HasEnoughStamina())
            {
                Debug.Log("❌ Not enough stamina to roll");
                return;
            }

            StartRoll();
        }
    }

    void StartRoll()
    {
        // Tính hướng roll
        CalculateRollDirection();

        // Consume stamina
        if (useStamina)
        {
            ConsumeStamina(staminaCost);
        }

        // Set animator
        animator.SetTrigger("Roll");
        animator.SetBool("isRolling", true);

        // Set states
        isRolling = true;
        canRoll = false;
        rollTimer = rollDuration;
        rollSpeed = rollDistance / rollDuration;

        // Xoay player về hướng roll
        if (rollDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(rollDirection);
        }

        // Start i-frames & cooldown
        if (iFramesCoroutine != null) StopCoroutine(iFramesCoroutine);
        iFramesCoroutine = StartCoroutine(HandleIFrames());

        if (cooldownCoroutine != null) StopCoroutine(cooldownCoroutine);
        cooldownCoroutine = StartCoroutine(RollCooldown());

        Debug.Log($"⚡ Roll started! Direction: {rollDirection}, Speed: {rollSpeed}");
    }

    void CalculateRollDirection()
    {
        if (moveInput.magnitude > 0.1f)
        {
            float cameraYaw = cameraTransform.eulerAngles.y;
            Vector3 cameraForward = Quaternion.Euler(0, cameraYaw, 0) * Vector3.forward;
            Vector3 cameraRight = Quaternion.Euler(0, cameraYaw, 0) * Vector3.right;

            rollDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;
        }
        else
        {
            rollDirection = transform.forward;
        }

        rollDirection.y = 0;
        rollDirection.Normalize();
    }

    void HandleRollMovement()
    {
        if (!isRolling) return;

        rollTimer -= Time.deltaTime;

        if (rollTimer > 0)
        {
            Vector3 movement = rollDirection * rollSpeed * Time.deltaTime;

            if (characterController != null)
            {
                characterController.Move(movement);
            }
            else if (rb != null)
            {
                rb.MovePosition(rb.position + movement);
            }
            else
            {
                transform.position += movement;
            }
        }
        else
        {
            EndRoll();
        }
    }

    IEnumerator HandleIFrames()
    {
        yield return new WaitForSeconds(rollDuration * iFrameStart);

        EnableInvincibility();
        Debug.Log("🛡️ I-Frames ENABLED");

        float iFrameDuration = rollDuration * (iFrameEnd - iFrameStart);
        yield return new WaitForSeconds(iFrameDuration);

        DisableInvincibility();
        Debug.Log("⚔️ I-Frames DISABLED");
    }

    void EnableInvincibility()
    {
        isInvincible = true;

        if (invincibleLayer.value != 0)
        {
            gameObject.layer = Mathf.RoundToInt(Mathf.Log(invincibleLayer.value, 2));
        }
    }

    void DisableInvincibility()
    {
        isInvincible = false;
        gameObject.layer = originalLayer;
    }

    IEnumerator RollCooldown()
    {
        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
        Debug.Log("✅ Roll ready!");
    }

    void EndRoll()
    {
        isRolling = false;
        animator.SetBool("isRolling", false);

        Debug.Log("🛑 Roll ended");
    }

    // ✅ NEW: Force end roll (khi bị interrupt bởi damage/death)
    public void ForceEndRoll()
    {
        if (!isRolling) return;

        // Stop all coroutines
        if (iFramesCoroutine != null)
        {
            StopCoroutine(iFramesCoroutine);
            iFramesCoroutine = null;
        }

        // Disable i-frames immediately
        DisableInvincibility();

        // End roll state
        isRolling = false;
        rollTimer = 0f;

        // Reset animator
        animator.SetBool("isRolling", false);

        Debug.Log("⚠️ FORCED ROLL END (interrupted)");
    }

    // ===== STAMINA METHODS =====

    bool HasEnoughStamina()
    {
        return true;
    }

    void ConsumeStamina(float amount)
    {
        Debug.Log($"Consumed {amount} stamina");
    }

    // ===== PUBLIC METHODS =====

    public bool IsRolling()
    {
        return isRolling;
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

    public bool CanRoll()
    {
        return canRoll && !isRolling;
    }

    // ===== DEBUG =====

    void OnDrawGizmos()
    {
        if (isRolling && rollDirection != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position + Vector3.up, rollDirection * 3f);

            if (isInvincible)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position + Vector3.up, 1f);
            }
        }
    }
}