using UnityEngine;
using System.Collections;
using System.Collections.Generic;

<<<<<<< Updated upstream
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
=======
public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int maxEnemies = 10;
    public int totalWaves = 3;
    public int enemiesPerWave = 3;
    public float waveDelay = 2f;
    public Vector3 triggerSize = new Vector3(10f, 5f, 10f);

    private bool playerInRange = false;
    private int currentWave = 0;
    private int enemiesSpawned = 0;
    private int enemiesAlive = 0;
    private Coroutine spawnCoroutine;

    void Start()
    {
        BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = triggerSize;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !playerInRange)
        {
            playerInRange = true;
            StartSpawning();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void StartSpawning()
    {
        if (spawnCoroutine != null) return;
        spawnCoroutine = StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        while (currentWave < totalWaves && enemiesSpawned < maxEnemies)
        {
            int toSpawn = Mathf.Min(enemiesPerWave, maxEnemies - enemiesSpawned);
            for (int i = 0; i < toSpawn; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(0.5f);
            }
            currentWave++;
            enemiesSpawned += toSpawn;
            if (currentWave < totalWaves)
                yield return new WaitForSeconds(waveDelay);
        }
>>>>>>> Stashed changes
    }

    void SpawnEnemy()
    {
<<<<<<< Updated upstream
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
=======
        Vector3 pos = transform.position;
        pos.y += 5f;
        GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);

        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        enemiesAlive++;
        Enemy e = enemy.GetComponent<Enemy>();
        if (e != null) e.OnDeath += () => enemiesAlive--;
        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null) ai.spawner = this;
    }

    public void NotifyEnemyDeath()
    {
        enemiesAlive--;
>>>>>>> Stashed changes
    }

    void OnDrawGizmos()
    {
<<<<<<< Updated upstream
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
=======
        Gizmos.color = playerInRange ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, triggerSize);
    }
}
>>>>>>> Stashed changes
