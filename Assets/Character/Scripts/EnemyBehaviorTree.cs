using UnityEngine;

/// <summary>
/// Behavior Tree cho Enemy - điều khiển AI cơ bản
/// Active Attacker: Chase + Attack
/// Waiting Attacker: Di chuyển lùi chậm
/// </summary>
public class EnemyBehaviorTree : MonoBehaviour
{
    private Enemy enemy;
    private Animator animator;
    private BehaviorTree tree;
    private Transform playerTransform;

    [Header("Behavior Tree")]
    public BehaviorTree behaviorTree;

    [Header("Behavior")]
    [Tooltip("Khoảng cách tối đa để phát hiện player")]
    public float detectionRange = 15f;

    [Tooltip("Khoảng cách tấn công")]
    public float attackRange = 2f;

    [Tooltip("Tốc độ di chuyển bình thường")]
    public float moveSpeed = 3f;

    [Tooltip("Tốc độ di chuyển lùi (rất chậm)")]
    public float retreatSpeed = 0.5f;

    [Tooltip("Thời gian chờ giữa các lần tấn công")]
    public float attackCooldown = 2f;

    private float lastAttackTime = 0f;
    private bool isMovingTowardPlayer = false;
    private float distanceToPlayer = float.MaxValue;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        animator = GetComponentInChildren<Animator>();
        playerTransform = FindPlayer();
        if (behaviorTree != null)
        {
            tree = behaviorTree;
        }
        else
        {
            // Create default tree
            BehaviorTreeBuilder builder = gameObject.AddComponent<BehaviorTreeBuilder>();
            tree = builder.CreateEnemyBehaviorTree();
            Destroy(builder); // Remove after use
        }
        tree.Initialize(this, playerTransform);
    }

    void Update()
    {
        // Đảm bảo không chạy AI khi đang knockback
        if (enemy.isKnockedBack)
        {
            // Có thể set animation idle hoặc không làm gì
            if (animator != null)
            {
                animator.SetBool("Run", false);
                animator.SetBool("BackWalk", false);
                animator.SetBool("Idle", true);
            }
            return;
        }

        if (tree != null)
        {
            tree.Tick();
        }
        else
        {
            // Fallback to old logic if no tree
            LegacyUpdate();
        }
    }

    void LegacyUpdate()
    {
        Transform playerTransform = FindPlayer();

        if (playerTransform == null)
        {
            // Không thấy player → idle
            SetAnimatorState("Idle");
            return;
        }

        distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Nếu quái không phải "active attacker" → di chuyển lùi chậm
        if (!EnemyAttackManager.IsActiveAttacker(enemy))
        {
            HandleRetreating(playerTransform);
            return;
        }

        // Active attacker logic
        if (distanceToPlayer > detectionRange)
        {
            SetAnimatorState("Idle");
        }
        else if (distanceToPlayer > attackRange)
        {
            // Chase
            MoveTowardPlayer(playerTransform);
        }
        else
        {
            // Attack
            HandleAttack(playerTransform);
        }
    }

    void MoveTowardPlayer(Transform playerTransform)
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0;

        // Xoay về phía player
        if (direction.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                5f * Time.deltaTime
            );
        }

        // Di chuyển
        if (enemy.rb != null)
        {
            enemy.rb.linearVelocity = new Vector3(
                direction.x * moveSpeed,
                enemy.rb.linearVelocity.y,
                direction.z * moveSpeed
            );
        }
        else
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        SetAnimatorState("Run");
        isMovingTowardPlayer = true;
    }

    void HandleRetreating(Transform playerTransform)
    {
        // Di chuyển LÙI chậm
        Vector3 direction = (transform.position - playerTransform.position).normalized;
        direction.y = 0;
        direction.Normalize();

        if (direction.magnitude < 0.1f)
        {
            direction = -transform.forward;
        }

        // Xoay gần với hướng di chuyển
        if (distanceToPlayer < detectionRange)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                3f * Time.deltaTime
            );
        }

        // Di chuyển lùi rất chậm
        if (enemy.rb != null)
        {
            enemy.rb.linearVelocity = new Vector3(
                direction.x * retreatSpeed,
                enemy.rb.linearVelocity.y,
                direction.z * retreatSpeed
            );
        }
        else
        {
            transform.position += direction * retreatSpeed * Time.deltaTime;
        }

        SetAnimatorState("BackWalk");
    }

    void HandleAttack(Transform playerTransform)
    {
        // Check cooldown
        if (Time.time - lastAttackTime < attackCooldown)
        {
            SetAnimatorState("Idle");
            return;
        }

        // Xoay về phía player
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Tấn công
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        lastAttackTime = Time.time;

        SetAnimatorState("Attack");

        // Gây damage
        Invoke(nameof(DealDamageToPlayer), 0.5f);
    }

    void DealDamageToPlayer()
    {
        Transform playerTransform = FindPlayer();
        if (playerTransform == null) return;

        float distanceNow = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceNow > attackRange + 1f) return; // Quá xa

        PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(15f, transform.position);
        }
    }

    public void SetAnimatorState(string stateName)
    {
        if (animator == null) return;

        animator.SetBool("Run", stateName == "Run");
        animator.SetBool("BackWalk", stateName == "BackWalk");
        animator.SetBool("Idle", stateName == "Idle");
    }

    Transform FindPlayer()
    {
        return GameObject.FindWithTag("Player")?.transform;
    }

    public float GetDistanceToPlayer()
    {
        return distanceToPlayer;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
