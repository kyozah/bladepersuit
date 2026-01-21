using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class AttackComboController : MonoBehaviour
{
    private Animator animator;
    private int currentCombo = 0;
    private bool canReceiveInput = false;
    private bool isExecutingAttack = false;

    [Header("Weapon Hitbox")]
    public WeaponHitbox weaponHitbox;

    [Header("Attack Movement")]
    public float dashDistance = 1.5f;
    public float dashDuration = 0.2f;

    private CharacterController characterController;
    private Rigidbody rb;
    private bool isDashing = false;
    private Vector3 dashDirection;
    private float dashTimer;
    private float dashSpeed;

    // Input System
    private PlayerInputActions inputActions;

    // References
    private RollController rollController;
    private PlayerHealth playerHealth;

    public event Action OnAttackStart;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Attack.performed += OnAttackInput;
    }

    void OnDisable()
    {
        inputActions.Player.Attack.performed -= OnAttackInput;
        inputActions.Player.Disable();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();

        if (weaponHitbox == null)
        {
            weaponHitbox = GetComponentInChildren<WeaponHitbox>();
            if (weaponHitbox != null)
            {
                Debug.Log($"✅ WeaponHitbox found: {weaponHitbox.gameObject.name}");
            }
            else
            {
                Debug.LogError("❌ WeaponHitbox NOT FOUND!");
            }
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

    void Update()
    {
        HandleDash();
    }

    void OnAttackInput(InputAction.CallbackContext context)
    {
        HandleAttackLogic();
    }

    void HandleAttackLogic()
    {
        // ✅ CHECK: Không thể attack khi đang roll
        if (rollController != null && rollController.IsRolling())
        {
            return;
        }

        // ✅ FIXED: Check IsInImpact thay vì IsStunned
        if (playerHealth != null && (playerHealth.IsInImpact() || playerHealth.IsDead()))
        {
            return;
        }

        // Slash 1: Chỉ khi KHÔNG đang attack
        if (currentCombo == 0 && !isExecutingAttack)
        {
            StartCombo(1);
        }
        // Slash 2: Khi đang combo 1 VÀ có thể nhận input
        else if (currentCombo == 1 && canReceiveInput)
        {
            canReceiveInput = false;
            StartCombo(2);
        }
        // Slash 3: Khi đang combo 2 VÀ có thể nhận input
        else if (currentCombo == 2 && canReceiveInput)
        {
            canReceiveInput = false;
            StartCombo(3);
        }
    }

    void HandleDash()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer > 0)
            {
                Vector3 movement = dashDirection * dashSpeed * Time.deltaTime;

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
                isDashing = false;
            }
        }
    }

    void StartCombo(int comboIndex)
    {
        currentCombo = comboIndex;
        canReceiveInput = false;
        isExecutingAttack = true;

        animator.SetBool("isAttacking", true);
        animator.SetInteger("attackIndex", comboIndex);
        Invoke(nameof(ClearAttackIndex), 0.1f);

        OnAttackStart?.Invoke();

        Debug.Log($"⚔️ Started Combo {comboIndex} - MOVEMENT LOCKED");
    }

    void ClearAttackIndex()
    {
        animator.SetInteger("attackIndex", 0);
    }

    // ===== ANIMATION EVENTS =====

    public void EnableNextInput()
    {
        canReceiveInput = true;
        Debug.Log("✅ Can receive next input (but still locked movement)");
    }

    public void DisableNextInput()
    {
        canReceiveInput = false;

        if (currentCombo != 3)
        {
            ResetCombo();
        }

        Debug.Log("🛑 Input disabled");
    }

    public void FinishCombo()
    {
        Debug.Log("🏁 Combo finished - UNLOCKING MOVEMENT");
        ResetCombo();
    }

    void ResetCombo()
    {
        currentCombo = 0;
        canReceiveInput = false;
        isExecutingAttack = false;

        animator.SetBool("isAttacking", false);
        animator.SetInteger("attackIndex", 0);

        Debug.Log("🔓 Movement UNLOCKED");
    }

    // ✅ PUBLIC: Force reset khi bị interrupt (impact, stun, etc)
    public void ForceResetCombo()
    {
        // Cancel combo ngay lập tức
        currentCombo = 0;
        canReceiveInput = false;
        isExecutingAttack = false;
        isDashing = false;

        // Reset animator states
        animator.SetBool("isAttacking", false);
        animator.SetInteger("attackIndex", 0);

        // Disable weapon hitbox
        if (weaponHitbox != null)
        {
            weaponHitbox.DisableDamage();
        }

        // Cancel any pending invokes
        CancelInvoke(nameof(ClearAttackIndex));

        Debug.Log("⚠️ FORCED COMBO RESET (interrupted)");
    }

    public void EnableWeaponDamage()
    {
        Debug.Log("📣 EnableWeaponDamage()");
        if (weaponHitbox != null)
        {
            weaponHitbox.EnableDamage();
        }
        else
        {
            Debug.LogError("❌ WeaponHitbox is NULL!");
        }
    }

    public void DisableWeaponDamage()
    {
        Debug.Log("📣 DisableWeaponDamage()");
        if (weaponHitbox != null)
        {
            weaponHitbox.DisableDamage();
        }
    }

    public void DashForward()
    {
        dashDirection = transform.forward;
        dashDirection.y = 0;
        dashDirection.Normalize();

        dashSpeed = dashDistance / dashDuration;

        isDashing = true;
        dashTimer = dashDuration;

        Debug.Log($"⚡ Dash forward! Direction: {dashDirection}");
    }

    public bool IsAttacking()
    {
        return isExecutingAttack;
    }
}