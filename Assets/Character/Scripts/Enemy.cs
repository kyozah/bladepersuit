using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    private float currentHealth;

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

        originalDrag = rb.linearDamping;

        if (showDebugInfo)
        {
            Debug.Log($"🟢 Enemy '{gameObject.name}' initialized");
            Debug.Log($"  Use Player Forward Direction: {usePlayerForwardDirection}");
        }
    }

    void FixedUpdate()
    {
        // Đảm bảo Rigidbody không bị sleep
        if (rb != null && isKnockedBack)
        {
            rb.WakeUp();
        }
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

        Vector3 knockbackVelocity = direction * knockbackForce;
        knockbackVelocity.y = knockbackUpwardForce;

        if (showDebugInfo)
        {
            Debug.Log($"⚡ Knockback velocity: {knockbackVelocity}");
            Debug.Log($"  Direction: {direction}");
            Debug.Log($"  Force: {knockbackForce} m/s");
        }

        // Set velocity
        rb.linearVelocity = knockbackVelocity;
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

    public bool IsKnockedBack()
    {
        return isKnockedBack;
    }
}