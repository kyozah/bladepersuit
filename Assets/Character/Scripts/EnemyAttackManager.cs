using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton - Quản lý tối đa 2-3 quái có thể tấn công cùng lúc
/// Các quái khác sẽ chờ cho đến khi quái tấn công được giết hoặc hết cooldown
/// </summary>
public class EnemyAttackManager : MonoBehaviour
{
    [Header("Attack Control")]
    [Tooltip("Số lượng quái tối đa được phép tấn công cùng lúc")]
    public int maxActiveAttackers = 2;

    private static EnemyAttackManager instance;
    private static List<Enemy> activeAttackers = new List<Enemy>();
    private static List<Enemy> allEnemies = new List<Enemy>();
    private static HashSet<Enemy> waitingEnemies = new HashSet<Enemy>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Gọi khi Enemy spawn
    /// </summary>
    public static void RegisterEnemy(Enemy enemy)
    {
        if (instance == null) return;

        if (!allEnemies.Contains(enemy))
        {
            allEnemies.Add(enemy);
            waitingEnemies.Add(enemy);

            instance.UpdateActiveAttackers();
        }
    }

    /// <summary>
    /// Gọi khi Enemy bị giết
    /// </summary>
    public static void UnregisterEnemy(Enemy enemy)
    {
        if (instance == null) return;

        allEnemies.Remove(enemy);
        activeAttackers.Remove(enemy);
        waitingEnemies.Remove(enemy);

        instance.UpdateActiveAttackers();
    }

    /// <summary>
    /// Check xem quái này có phải "active attacker" không
    /// </summary>
    public static bool IsActiveAttacker(Enemy enemy)
    {
        return activeAttackers.Contains(enemy);
    }

    /// <summary>
    /// Cập nhật danh sách quái tấn công
    /// </summary>
    void UpdateActiveAttackers()
    {
        // Loại bỏ quái chết khỏi active list
        activeAttackers.RemoveAll(e => e == null);

        // Nếu chưa đủ active attackers, thêm từ waiting list
        while (activeAttackers.Count < maxActiveAttackers && waitingEnemies.Count > 0)
        {
            Enemy nextAttacker = GetNearestWaitingEnemy();
            if (nextAttacker != null)
            {
                activeAttackers.Add(nextAttacker);
                waitingEnemies.Remove(nextAttacker);
            }
            else
            {
                break;
            }
        }

        DebugAttackerStatus();
    }

    /// <summary>
    /// Lấy quái gần player nhất từ danh sách waiting
    /// </summary>
    Enemy GetNearestWaitingEnemy()
    {
        Transform playerTransform = FindPlayer();
        if (playerTransform == null) return null;

        Enemy nearest = null;
        float minDistance = float.MaxValue;

        foreach (Enemy enemy in waitingEnemies)
        {
            if (enemy == null) continue;

            float distance = Vector3.Distance(enemy.transform.position, playerTransform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    Transform FindPlayer()
    {
        return GameObject.FindWithTag("Player")?.transform;
    }

    void DebugAttackerStatus()
    {
        // Tùy chọn: Thêm debug
        // Debug.Log($"Active Attackers: {activeAttackers.Count}/{maxActiveAttackers}, Waiting: {waitingEnemies.Count}");
    }

    // Getter cho debug
    public static int GetActiveAttackerCount()
    {
        return activeAttackers.Count;
    }

    public static int GetWaitingEnemyCount()
    {
        return waitingEnemies.Count;
    }
}
