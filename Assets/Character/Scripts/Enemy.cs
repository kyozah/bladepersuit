using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("AI Settings")]
    public float detectionRange = 15f;
    public float attackRange = 2f;
    public float retreatDistance = 5f;
    public float moveSpeed = 5f;
    public float attackDelay = 1f; // Thời gian chuẩn bị attack

    [Header("Knockback - Velocity Based")]
    [Tooltip("Lực knockback (m/s)")]
    public float knockbackForce = 10f;

    [Tooltip("Lực đẩy lên trên")]
    public float knockbackUpwardForce = 2f;

    [Tooltip("Thời gian knockback kéo dài")]
    public float knockbackDuration = 0.3f;

    [Tooltip("Drag trong lúc knockback")]
    public float knockbackDrag = 8f;

    [Header("Knockback Direction")]
    [Tooltip("Dùng hướng player đang nhìn thay vì hướng từ player đến enemy")]
    public bool usePlayerForwardDirection = true;

    [Header("Debug")]
    public bool showDebugInfo = false;

    private Rigidbody rb;
    private Animator animator;
    private bool isKnockedBack = false;
    private float originalDrag;
    private Coroutine knockbackCoroutine;

    // AI variables
    private EnemyManager manager;
    private Transform player;
    private enum AIState { Idle, Chase, Attack, Retreat }
    private AIState currentState = AIState.Idle;
    private Vector3 retreatPosition;
    private float lastActionTime = 0f;
    private bool isAttacking = false;
    private Vector3 moveTarget;
    private bool shouldMove = false;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (rb == null)
        {
            Debug.LogError($"❌ Enemy '{gameObject.name}' MISSING RIGIDBODY!");
            return;
        }

        // ✅ ENSURE PROPER RIGIDBODY SETUP
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        originalDrag = rb.linearDamping;

        if (showDebugInfo)
        {
            Debug.Log($"🟢 Enemy '{gameObject.name}' initialized");
            Debug.Log($"  Use Player Forward Direction: {usePlayerForwardDirection}");
            Debug.Log($"  Gravity enabled: {rb.useGravity}");
            Debug.Log($"  Is Kinematic: {rb.isKinematic}");
        }
    }

    void Update()
    {
        if (isKnockedBack || isAttacking) return;

        UpdateAI();
    }

    void FixedUpdate()
    {
        // Đảm bảo Rigidbody không bị sleep
        if (rb != null && isKnockedBack)
        {
            rb.WakeUp();
        }

        // AI Movement
        if (shouldMove && !isKnockedBack && !isAttacking)
        {
            MoveTowards(moveTarget);
        }
        if (animator != null)
{
    animator.SetBool("IsMoving", shouldMove && !isAttacking && !isKnockedBack);
}   
 }

    void UpdateAI()
    {
        if (manager == null || player == null) return;

        if (Time.time - lastActionTime < 3f) return; // Delay 3s

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case AIState.Idle:
                if (manager.IsPlayerInZone() && distanceToPlayer <= detectionRange)
                {
                    currentState = AIState.Chase;
                    lastActionTime = Time.time;
                }
                break;

            case AIState.Chase:
                if (distanceToPlayer <= attackRange)
                {
                    if (manager.CanAttack(this))
                    {
                        StartAttack();
                        lastActionTime = Time.time;
                    }
                }
                else
                {
                    moveTarget = player.position;
                    shouldMove = true;
                }
                break;

            case AIState.Attack:
                // Attack logic handled in coroutine
                break;

            case AIState.Retreat:
                if (Vector3.Distance(transform.position, retreatPosition) < 1f)
                {
                    currentState = AIState.Idle;
                    lastActionTime = Time.time;
                    shouldMove = false;
                }
                else
                {
                    moveTarget = retreatPosition;
                    shouldMove = true;
                }
                break;
        }
    }

    void MoveTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        rb.MovePosition(transform.position + direction * moveSpeed * Time.fixedDeltaTime);
        if (player != null)
        {
            transform.LookAt(player.position);
        }
    }

    void StartAttack()
    {
        currentState = AIState.Attack;
        isAttacking = true;
        manager.StartAttack(this);

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        StartCoroutine(PerformAttack());
    }

    IEnumerator PerformAttack()
    {
        yield return new WaitForSeconds(attackDelay);

        // Attack logic: damage player
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(5f, transform.position);
                Debug.Log("Enemy attacked player for 5 damage");
            }
        }

        // After attack, retreat
        retreatPosition = transform.position + (transform.position - player.position).normalized * retreatDistance;
        currentState = AIState.Retreat;
        isAttacking = false;
        manager.EndAttack(this);
    }

    // ✅ THÊM overload để nhận player forward direction
    public void TakeDamage(float damage, Vector3 attackerPosition, Vector3 attackerForward)
    {
        if (rb == null)
        {
            Debug.LogError("❌ No Rigidbody!");
            return;
        }

        if (showDebugInfo)
        {
            Debug.Log($"\n💥 TakeDamage: {gameObject.name}");
            Debug.Log($"  Attacker forward: {attackerForward}");
        }

        // Trừ máu
        currentHealth -= damage;

        // Animation
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        // ✅✅✅ TÍNH HƯỚNG KNOCKBACK
        Vector3 knockbackDirection;

        if (usePlayerForwardDirection)
        {
            // ✅ DÙNG HƯỚNG PLAYER ĐANG NHÌN (forward)
            knockbackDirection = attackerForward;
            knockbackDirection.y = 0;
            knockbackDirection.Normalize();

            if (showDebugInfo)
            {
                Debug.Log($"  Using FORWARD direction: {knockbackDirection}");
            }
        }
        else
        {
            // ❌ Cách cũ: Từ player đến enemy (không dự đoán được)
            knockbackDirection = (transform.position - attackerPosition).normalized;
            knockbackDirection.y = 0;

            if (knockbackDirection.magnitude < 0.1f)
            {
                knockbackDirection = attackerForward;
            }

            knockbackDirection.Normalize();

            if (showDebugInfo)
            {
                Debug.Log($"  Using POSITION direction: {knockbackDirection}");
            }
        }

        // Apply knockback
        ApplyVelocityKnockback(knockbackDirection);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // ✅ Giữ lại overload cũ để tương thích
    public void TakeDamage(float damage, Vector3 attackerPosition)
    {
        // Fallback: tính forward từ position
        Vector3 direction = (transform.position - attackerPosition).normalized;
        TakeDamage(damage, attackerPosition, direction);
    }

    void ApplyVelocityKnockback(Vector3 direction)
    {
        if (rb == null) return;

        // Dừng coroutine cũ
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        // Wake up rigidbody
        rb.WakeUp();

        // Use AddForce to allow gravity to work naturally
        Vector3 knockbackForceVector = direction * knockbackForce;
        
        if (showDebugInfo)
        {
            Debug.Log($"⚡ Knockback force: {knockbackForceVector}");
            Debug.Log($"  Direction: {direction}");
            Debug.Log($"  Force: {knockbackForce} m/s");
        }

        // Apply knockback force and upward force separately
        rb.linearVelocity = new Vector3(knockbackForceVector.x, rb.linearVelocity.y, knockbackForceVector.z);
        rb.AddForce(Vector3.up * knockbackUpwardForce, ForceMode.VelocityChange);
        rb.linearDamping = knockbackDrag;

        isKnockedBack = true;

        // Debug
        StartCoroutine(DebugKnockback());

        // Reset state
        knockbackCoroutine = StartCoroutine(ResetKnockbackState());
    }

    IEnumerator DebugKnockback()
    {
        if (!showDebugInfo) yield break;

        Vector3 startPos = transform.position;
        yield return new WaitForSeconds(0.1f);

        float moved = Vector3.Distance(startPos, transform.position);

        if (moved < 0.05f)
        {
            Debug.LogError($"❌ NOT MOVING! Only {moved:F3}m");
        }
        else
        {
            Debug.Log($"✅ Moving! Distance: {moved:F2}m");
        }
    }

    IEnumerator ResetKnockbackState()
    {
        yield return new WaitForSeconds(knockbackDuration);

        if (rb != null)
        {
            rb.linearDamping = originalDrag;
        }

        isKnockedBack = false;
        knockbackCoroutine = null;
    }

    void Die()
    {
        Debug.Log($"💀 {gameObject.name} died");

        if (manager != null)
        {
            manager.RemoveEnemy(this);
        }

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        StopAllCoroutines();
        Destroy(gameObject, 2f);
    }

    public void SetManager(EnemyManager mgr)
    {
        manager = mgr;
        player = mgr.GetPlayer();
    }
}