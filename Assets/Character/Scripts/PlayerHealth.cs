using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Máu tối đa")]
    public float maxHealth = 100f;

    private float currentHealth;

    [Header("Damage Settings")]
    [Tooltip("Thời gian bất tử sau khi bị damage (I-frames)")]
    public float damageImmunityDuration = 1f;

    [Tooltip("Thời gian bị stun/impact (KHÔNG thể làm gì)")]
    public float impactDuration = 0.7f;

    [Tooltip("Layer khi đang bất tử")]
    public LayerMask invincibleLayer;

    [Header("Knockback")]
    [Tooltip("Có bị knockback khi nhận damage không")]
    public bool enableKnockback = true;

    [Tooltip("Lực knockback khi bị đánh")]
    public float knockbackForce = 5f;

    [Tooltip("Thời gian knockback")]
    public float knockbackDuration = 0.3f;

    [Header("Death Settings")]
    [Tooltip("Delay trước khi respawn/game over")]
    public float deathDelay = 3f;

    [Tooltip("Có respawn không?")]
    public bool autoRespawn = false;

    [Tooltip("Vị trí respawn")]
    public Transform respawnPoint;

    [Header("References")]
    public Animator animator;

    private CharacterController characterController;
    private bool isDead = false;
    private bool isInvincible = false;
    private bool isInImpact = false;
    private int originalLayer;

    // References to other systems
    private ThirdPersonController movementController;
    private AttackComboController attackController;
    private RollController rollController;

    void Start()
    {
        currentHealth = maxHealth;
        originalLayer = gameObject.layer;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        characterController = GetComponent<CharacterController>();
        movementController = GetComponent<ThirdPersonController>();
        attackController = GetComponent<AttackComboController>();
        rollController = GetComponent<RollController>();

        Debug.Log($"✅ PlayerHealth initialized. Max HP: {maxHealth}");
    }

    public void TakeDamage(float damage, Vector3 attackerPosition)
    {
        // Check nếu đã chết hoặc đang bất tử
        if (isDead || isInvincible)
        {
            Debug.Log("🛡️ Player is invincible or dead, no damage taken");
            return;
        }

        // Check nếu đang roll (I-frames)
        if (rollController != null && rollController.IsInvincible())
        {
            Debug.Log("🛡️ Player rolling with I-frames, no damage taken");
            return;
        }

        // Check nếu đang trong impact animation (tránh spam)
        if (isInImpact)
        {
            Debug.Log("⚠️ Already in impact, ignoring damage");
            return;
        }

        // Trừ máu
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"💔 Player took {damage} damage. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Start impact state
            StartCoroutine(ImpactState());

            // Interrupt tất cả actions
            InterruptActions();

            // Play impact animation
            PlayImpactAnimation();

            // Knockback
            if (enableKnockback)
            {
                ApplyKnockback(attackerPosition);
            }

            // Enable I-frames
            StartCoroutine(DamageImmunity());
        }
    }

    IEnumerator ImpactState()
    {
        isInImpact = true;
        Debug.Log("😵 IMPACT STATE - All inputs locked");

        yield return new WaitForSeconds(impactDuration);

        isInImpact = false;
        Debug.Log("✅ Impact ended - Inputs unlocked");
    }

    void InterruptActions()
    {
        if (attackController != null)
        {
            attackController.ForceResetCombo();
        }

        if (rollController != null)
        {
            rollController.ForceEndRoll();
        }
    }

    void PlayImpactAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isAttacking", false);
            animator.SetBool("isRolling", false);
            animator.SetInteger("attackIndex", 0);

            animator.SetTrigger("Impact");

            Debug.Log("💥 Playing Impact animation");
        }
    }

    void ApplyKnockback(Vector3 attackerPosition)
    {
        Vector3 knockbackDirection = (transform.position - attackerPosition).normalized;
        knockbackDirection.y = 0;

        if (knockbackDirection.magnitude < 0.1f)
        {
            knockbackDirection = -transform.forward;
        }

        knockbackDirection.Normalize();

        StartCoroutine(ApplyKnockbackCoroutine(knockbackDirection));

        Debug.Log($"⚡ Knockback applied! Direction: {knockbackDirection}");
    }

    IEnumerator ApplyKnockbackCoroutine(Vector3 direction)
    {
        float timer = knockbackDuration;
        float speed = knockbackForce / knockbackDuration;

        while (timer > 0)
        {
            if (characterController != null && characterController.enabled)
            {
                characterController.Move(direction * speed * Time.deltaTime);
            }
            else
            {
                transform.position += direction * speed * Time.deltaTime;
            }

            timer -= Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator DamageImmunity()
    {
        isInvincible = true;

        if (invincibleLayer.value != 0)
        {
            gameObject.layer = Mathf.RoundToInt(Mathf.Log(invincibleLayer.value, 2));
        }

        Debug.Log("🛡️ Damage immunity ENABLED");

        yield return new WaitForSeconds(damageImmunityDuration);

        isInvincible = false;
        gameObject.layer = originalLayer;

        Debug.Log("⚔️ Damage immunity DISABLED");
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log("💀 Player DIED");

        InterruptActions();

        if (animator != null)
        {
            animator.SetBool("isAttacking", false);
            animator.SetBool("isRolling", false);
            animator.SetInteger("attackIndex", 0);
            animator.SetTrigger("Death");
        }

        if (movementController != null)
        {
            movementController.enabled = false;
        }

        if (attackController != null)
        {
            attackController.enabled = false;
        }

        if (rollController != null)
        {
            rollController.enabled = false;
        }

        StartCoroutine(HandleDeath());
    }

    IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(deathDelay);

        if (autoRespawn)
        {
            Respawn();
        }
        else
        {
            Debug.Log("🎮 GAME OVER");
        }
    }

    void Respawn()
    {
        isDead = false;
        isInImpact = false;
        currentHealth = maxHealth;

        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;
        }

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        if (movementController != null)
        {
            movementController.enabled = true;
        }

        if (attackController != null)
        {
            attackController.enabled = true;
        }

        if (rollController != null)
        {
            rollController.enabled = true;
        }

        Debug.Log("✅ Player RESPAWNED");
    }

    // ===== PUBLIC METHODS =====

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

    public bool IsInImpact()
    {
        return isInImpact;
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        Debug.Log($"💚 Player healed {amount}. HP: {currentHealth}/{maxHealth}");
    }

    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;

        if (invincible && invincibleLayer.value != 0)
        {
            gameObject.layer = Mathf.RoundToInt(Mathf.Log(invincibleLayer.value, 2));
        }
        else
        {
            gameObject.layer = originalLayer;
        }
    }

    // ===== DEBUG =====

    void OnGUI()
    {
        if (isDead) return;

        float barWidth = 200f;
        float barHeight = 20f;
        float barX = 10f;
        float barY = 10f;

        GUI.Box(new Rect(barX, barY, barWidth, barHeight), "");

        float healthWidth = barWidth * (currentHealth / maxHealth);
        GUI.color = Color.red;
        GUI.Box(new Rect(barX, barY, healthWidth, barHeight), "");
        GUI.color = Color.white;

        GUI.Label(new Rect(barX + 5, barY + 2, barWidth, barHeight),
                  $"HP: {currentHealth:F0}/{maxHealth:F0}");

        if (isInImpact)
        {
            GUI.color = Color.yellow;
            GUI.Label(new Rect(barX, barY + 25, 200, 20), "IMPACT - LOCKED");
            GUI.color = Color.white;
        }
    }
}