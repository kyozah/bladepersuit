using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    private static EnemyManager instance;
    public static EnemyManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Object.FindFirstObjectByType<EnemyManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("EnemyManager");
                    instance = obj.AddComponent<EnemyManager>();
                }
            }
            return instance;
        }
    }

    private EnemyAI currentAttacker;
    private Queue<EnemyAI> attackQueue = new Queue<EnemyAI>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool CanAttack(EnemyAI enemy)
    {
        if (currentAttacker == null)
        {
            currentAttacker = enemy;
            return true;
        }
        else if (currentAttacker == enemy)
        {
            return true;
        }
        else
        {
            // Thêm vào queue nếu chưa có
            if (!attackQueue.Contains(enemy))
            {
                attackQueue.Enqueue(enemy);
            }
            return false;
        }
    }

    public void EndAttack(EnemyAI enemy)
    {
        if (currentAttacker == enemy)
        {
            currentAttacker = null;

            // Chuyển sang enemy tiếp theo
            if (attackQueue.Count > 0)
            {
                currentAttacker = attackQueue.Dequeue();
            }
        }
    }

    public void RemoveFromQueue(EnemyAI enemy)
    {
        if (currentAttacker == enemy)
        {
            EndAttack(enemy);
        }
        else
        {
            // Xóa khỏi queue
            Queue<EnemyAI> newQueue = new Queue<EnemyAI>();
            while (attackQueue.Count > 0)
            {
                EnemyAI e = attackQueue.Dequeue();
                if (e != enemy)
                {
                    newQueue.Enqueue(e);
                }
            }
            attackQueue = newQueue;
        }
    }
}