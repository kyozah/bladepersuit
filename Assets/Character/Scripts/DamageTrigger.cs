using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Damage gây ra khi player chạm vào")]
    public float damageAmount = 20f;

    [Tooltip("Cooldown giữa các lần damage (tránh spam damage)")]
    public float damageCooldown = 1f;

    [Tooltip("Có tự động damage liên tục khi player ở trong trigger không")]
    public bool continuousDamage = false;

    [Tooltip("Interval giữa các lần damage liên tục (nếu bật continuous)")]
    public float continuousDamageInterval = 1f;

    [Header("Visual Feedback")]
    [Tooltip("Màu của cube (optional)")]
    public Color damageColor = Color.red;

    [Tooltip("Flash khi gây damage")]
    public bool flashOnDamage = true;

    [Tooltip("Thời gian flash")]
    public float flashDuration = 0.2f;

    [Header("Debug")]
    public bool showDebugInfo = true;

    private float lastDamageTime = 0f;
    private Renderer cubeRenderer;
    private Color originalColor;
    private bool isFlashing = false;

    void Start()
    {
        // Get renderer để change color
        cubeRenderer = GetComponent<Renderer>();

        if (cubeRenderer != null)
        {
            originalColor = cubeRenderer.material.color;
            cubeRenderer.material.color = damageColor;
        }

        // Ensure có Collider và set as Trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError("❌ DamageTrigger: No Collider found! Add BoxCollider.");
        }

        if (showDebugInfo)
        {
            Debug.Log($"✅ DamageTrigger initialized: {damageAmount} damage, cooldown: {damageCooldown}s");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check nếu là Player
        if (other.CompareTag("Player"))
        {
            if (showDebugInfo)
            {
                Debug.Log($"💥 Player entered damage trigger: {gameObject.name}");
            }

            // Gây damage ngay lập tức
            DealDamageToPlayer(other.gameObject);
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Continuous damage nếu bật
        if (continuousDamage && other.CompareTag("Player"))
        {
            if (Time.time - lastDamageTime >= continuousDamageInterval)
            {
                DealDamageToPlayer(other.gameObject);
            }
        }
    }

    void DealDamageToPlayer(GameObject player)
    {
        // Check cooldown
        if (Time.time - lastDamageTime < damageCooldown)
        {
            if (showDebugInfo)
            {
                Debug.Log("⏱️ Damage on cooldown, skipping...");
            }
            return;
        }

        // Get PlayerHealth component
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            // Gây damage
            playerHealth.TakeDamage(damageAmount, transform.position);

            lastDamageTime = Time.time;

            if (showDebugInfo)
            {
                Debug.Log($"💔 Dealt {damageAmount} damage to Player. HP: {playerHealth.GetCurrentHealth()}/{playerHealth.GetMaxHealth()}");
            }

            // Visual feedback
            if (flashOnDamage && !isFlashing)
            {
                StartCoroutine(FlashEffect());
            }
        }
        else
        {
            Debug.LogError("❌ Player doesn't have PlayerHealth component!");
        }
    }

    System.Collections.IEnumerator FlashEffect()
    {
        if (cubeRenderer == null) yield break;

        isFlashing = true;

        // Flash to white
        cubeRenderer.material.color = Color.white;

        yield return new WaitForSeconds(flashDuration);

        // Back to damage color
        cubeRenderer.material.color = damageColor;

        isFlashing = false;
    }

    // ===== GIZMOS =====

    void OnDrawGizmos()
    {
        // Vẽ bounding box màu đỏ
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            if (col is BoxCollider)
            {
                BoxCollider box = (BoxCollider)col;
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider)
            {
                SphereCollider sphere = (SphereCollider)col;
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Vẽ text info
        Gizmos.color = Color.red;

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Vector3 labelPos = transform.position + Vector3.up * 2f;

#if UNITY_EDITOR
            UnityEditor.Handles.Label(labelPos, $"Damage: {damageAmount}\nCooldown: {damageCooldown}s");
#endif
        }
    }
}