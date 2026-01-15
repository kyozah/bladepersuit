using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Đặt trên các "spawn block" trong map
/// Khi player tiếp cận, sẽ spawn quái lần lượt (không spawn toàn bộ cùng lúc)
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Prefab quái để spawn")]
    public GameObject enemyPrefab;

    [Tooltip("Số lượng quái sẽ spawn")]
    public int enemiesToSpawn = 5;

    [Tooltip("Thời gian chờ giữa mỗi lần spawn")]
    public float spawnInterval = 1.5f;

    [Tooltip("Khoảng cách trigger spawn (player tiếp cận)")]
    public float activationDistance = 20f;

    [Header("Spawn Points")]
    [Tooltip("Nếu để trống, sẽ spawn xung quanh block")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Debug")]
    public bool showDebugInfo = false;

    private bool hasSpawned = false;
    private bool isSpawning = false;
    private int spawnedCount = 0;

    void Update()
    {
        if (hasSpawned) return;

        Transform playerTransform = FindPlayer();
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer < activationDistance && !isSpawning)
        {
            StartCoroutine(SpawnEnemiesSequence());
        }
    }

    IEnumerator SpawnEnemiesSequence()
    {
        isSpawning = true;

        if (showDebugInfo)
            Debug.Log($"🟢 [Spawner] Starting spawn sequence: {enemiesToSpawn} enemies");

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy();
            spawnedCount++;

            if (showDebugInfo)
                Debug.Log($"🟢 [Spawner] Spawned enemy {spawnedCount}/{enemiesToSpawn}");

            if (i < enemiesToSpawn - 1) // Không wait sau lần cuối
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        hasSpawned = true;
        isSpawning = false;

        if (showDebugInfo)
            Debug.Log($"✅ [Spawner] All enemies spawned!");
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("❌ Enemy prefab not assigned!");
            return;
        }

        Vector3 spawnPos = GetSpawnPosition();

        GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        if (showDebugInfo)
            Debug.Log($"✅ Enemy spawned at {spawnPos}");
    }

    Vector3 GetSpawnPosition()
    {
        if (spawnPoints.Count > 0)
        {
            // Dùng spawn point nếu có
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            return spawnPoint.position;
        }

        // Nếu không, spawn xung quanh block
        Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(2f, 5f);
        return transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
    }

    Transform FindPlayer()
    {
        return GameObject.FindWithTag("Player")?.transform;
    }

    void OnDrawGizmos()
    {
        // Vẽ activation distance
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationDistance);

        // Vẽ spawn points
        if (spawnPoints.Count > 0)
        {
            Gizmos.color = Color.blue;
            foreach (Transform sp in spawnPoints)
            {
                if (sp != null)
                    Gizmos.DrawWireSphere(sp.position, 0.5f);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        GUI.color = Color.green;

        if (spawnPoints.Count > 0)
        {
            foreach (Transform sp in spawnPoints)
            {
                if (sp != null)
                {
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(sp.position + Vector3.up, "Spawn Point");
#endif
                }
            }
        }
    }
}
