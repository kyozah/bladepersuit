using UnityEngine;
using System.Collections;

public class EnemyAI : Enemy
{
    public float moveSpeed = 3.5f;
    public float minDistance = 1.5f;
    public float maxDistance = 3f;
    public float attackCooldown = 2f;
    public float maxOrbitX = 2.5f;
    public float maxOrbitZ = 2.5f;
    public EnemySpawner spawner;

    private Transform player;
    private bool canAttack = true;
    private bool isAttacking = false;
    private float lastAttackTime;
    private Vector3 currentTarget;

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        currentTarget = GetRandomTarget();
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.mass = 1f;
            rb.linearDamping = 0f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    void FixedUpdate()
    {
        if (player == null || IsKnockedBack()) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > maxDistance)
        {
            MoveTowards(player.position);
        }
        else if (dist < minDistance)
        {
            Vector3 away = (transform.position - player.position).normalized * (maxOrbitX + maxOrbitZ) / 2f;
            MoveTowards(player.position + away);
        }
        else
        {
            if (Vector3.Distance(transform.position, currentTarget) < 0.5f)
                currentTarget = GetRandomTarget();
            MoveTowards(currentTarget);
            if (canAttack && Time.time - lastAttackTime > attackCooldown)
                TryAttack();
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && dist >= minDistance && dist <= maxDistance)
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
    }

    void TryAttack()
    {
        if (!EnemyManager.Instance.CanAttack(this)) return;
        isAttacking = true;
        lastAttackTime = Time.time;
        StartCoroutine(EndAttack());
    }

    IEnumerator EndAttack()
    {
        yield return new WaitForSeconds(1f);
        isAttacking = false;
        EnemyManager.Instance.EndAttack(this);
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    Vector3 GetRandomTarget()
    {
        float x = Random.Range(-maxOrbitX, maxOrbitX);
        float z = Random.Range(-maxOrbitZ, maxOrbitZ);
        return player.position + new Vector3(x, 0, z);
    }

    void MoveTowards(Vector3 target)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 dir = (target - transform.position).normalized;
            rb.linearVelocity = new Vector3(dir.x * moveSpeed, rb.linearVelocity.y, dir.z * moveSpeed);
            if (dir != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
            }
        }
    }

    protected override void Die()
    {
        base.Die();
        EnemyManager.Instance.RemoveFromQueue(this);
        // spawner?.NotifyEnemyDeath();
    }
}