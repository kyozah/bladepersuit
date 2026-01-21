using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public int maxEnemies = 5;
    public float spawnRadius = 10f;
    public float spawnHeight = 0f;

    [Header("Attack Management")]
    public float attackCooldown = 3f; // Delay giữa các attack

    private List<Enemy> enemies = new List<Enemy>();
    private bool playerInZone = false;
    private Transform player;
    private float lastAttackTime = -Mathf.Infinity;
    private Enemy currentAttackingEnemy = null;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found! Make sure player has tag 'Player'");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            SpawnEnemies();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            // Có thể despawn enemies nếu muốn, nhưng theo yêu cầu, giữ lại
        }
    }

    void SpawnEnemies()
    {
        for (int i = enemies.Count; i < maxEnemies; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.SetManager(this);
                enemies.Add(enemy);
            }
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, spawnHeight, randomCircle.y);
        return spawnPos;
    }

    public bool CanAttack(Enemy enemy)
    {
        if (Time.time - lastAttackTime < attackCooldown) return false;
        if (currentAttackingEnemy != null && currentAttackingEnemy != enemy) return false;
        return true;
    }

    public void StartAttack(Enemy enemy)
    {
        currentAttackingEnemy = enemy;
        lastAttackTime = Time.time;
    }

    public void EndAttack(Enemy enemy)
    {
        if (currentAttackingEnemy == enemy)
        {
            currentAttackingEnemy = null;
        }
    }

    public Transform GetPlayer()
    {
        return player;
    }

    public bool IsPlayerInZone()
    {
        return playerInZone;
    }

    public void RemoveEnemy(Enemy enemy)
    {
        enemies.Remove(enemy);
        if (currentAttackingEnemy == enemy)
        {
            currentAttackingEnemy = null;
        }
    }
}