using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponHitbox : MonoBehaviour
{
    [Header("Damage")]
    public float damage = 20f;

    [Header("Hit Detection")]
    [Tooltip("Delay trước khi hitbox active")]
    public float activationDelay = 0.05f;

    [Tooltip("Đẩy enemies đang overlap trước khi enable")]
    public bool pushOverlappingEnemies = true;

    [Tooltip("Lực đẩy enemies overlap")]
    public float pushForce = 15f;

    private bool canDealDamage = false;
    private List<Collider> hitEnemies = new List<Collider>();
    private Coroutine enableCoroutine;

    void Start()
    {
        Debug.Log($"WeaponHitbox initialized on {gameObject.name}");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!canDealDamage) return;
        if (hitEnemies.Contains(other)) return;

        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"⚔️ Hit {other.gameObject.name}");

            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                // ✅✅✅ GỬI PLAYER FORWARD DIRECTION
                Vector3 playerForward = transform.root.forward;

                enemy.TakeDamage(damage, transform.root.position, playerForward);

                hitEnemies.Add(other);
                Debug.Log($"✅ Damage dealt! Knockback direction: {playerForward}");
            }
            else
            {
                Debug.LogError($"❌ No Enemy component!");
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!canDealDamage) return;
        if (hitEnemies.Contains(other)) return;

        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"⚔️ Hit (TriggerStay) {other.gameObject.name}");

            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                // ✅ GỬI PLAYER FORWARD
                Vector3 playerForward = transform.root.forward;

                enemy.TakeDamage(damage, transform.root.position, playerForward);
                hitEnemies.Add(other);
                Debug.Log($"✅ Damage dealt via TriggerStay!");
            }
        }
    }

    public void EnableDamage()
    {
        hitEnemies.Clear();

        if (enableCoroutine != null)
        {
            StopCoroutine(enableCoroutine);
        }

        enableCoroutine = StartCoroutine(EnableDamageDelayed());
    }

    IEnumerator EnableDamageDelayed()
    {
        // Push overlapping enemies
        if (pushOverlappingEnemies)
        {
            PushOverlappingEnemies();
        }

        // Delay
        if (activationDelay > 0)
        {
            yield return new WaitForSeconds(activationDelay);
        }

        // Enable
        canDealDamage = true;
        Debug.Log("🗡️ Weapon damage ENABLED");

        // Check enemies trong hitbox
        CheckForEnemiesInHitbox();
    }

    void CheckForEnemiesInHitbox()
    {
        Collider hitboxCollider = GetComponent<Collider>();
        if (hitboxCollider == null) return;

        Collider[] overlapping = Physics.OverlapBox(
            hitboxCollider.bounds.center,
            hitboxCollider.bounds.extents,
            transform.rotation
        );

        foreach (Collider col in overlapping)
        {
            if (col.CompareTag("Enemy") && !hitEnemies.Contains(col))
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    Debug.Log($"⚔️ Found enemy in hitbox: {col.gameObject.name}");

                    // ✅ GỬI PLAYER FORWARD
                    Vector3 playerForward = transform.root.forward;

                    enemy.TakeDamage(damage, transform.root.position, playerForward);
                    hitEnemies.Add(col);
                }
            }
        }
    }

    void PushOverlappingEnemies()
    {
        Collider hitboxCollider = GetComponent<Collider>();
        if (hitboxCollider == null) return;

        Collider[] overlapping = Physics.OverlapBox(
            hitboxCollider.bounds.center,
            hitboxCollider.bounds.extents,
            transform.rotation
        );

        int pushedCount = 0;

        foreach (Collider col in overlapping)
        {
            if (col.CompareTag("Enemy"))
            {
                Rigidbody enemyRb = col.GetComponent<Rigidbody>();

                if (enemyRb != null)
                {
                    // ✅ SỬ DỤNG PLAYER FORWARD cho push direction
                    Vector3 pushDirection = transform.root.forward;
                    pushDirection.y = 0;
                    pushDirection.Normalize();

                    Vector3 pushVelocity = pushDirection * pushForce;
                    pushVelocity.y = 1f;

                    enemyRb.WakeUp();
                    enemyRb.linearVelocity = pushVelocity;

                    pushedCount++;
                    Debug.LogWarning($"⚠️ Pushed: {col.gameObject.name} in direction: {pushDirection}");
                }
            }
        }

        if (pushedCount > 0)
        {
            Debug.Log($"⚡ Pushed {pushedCount} enemies in player forward direction");
        }
    }

    public void DisableDamage()
    {
        canDealDamage = false;

        if (enableCoroutine != null)
        {
            StopCoroutine(enableCoroutine);
            enableCoroutine = null;
        }

        Debug.Log("🛡️ Weapon damage DISABLED");
    }
}