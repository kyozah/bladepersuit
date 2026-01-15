using UnityEngine;

public abstract class ActionNode : BTNode
{
    protected EnemyBehaviorTree ai;
    protected Transform player;

    public void SetContext(EnemyBehaviorTree ai, Transform player)
    {
        this.ai = ai;
        this.player = player;
    }
}

[CreateAssetMenu(fileName = "MoveToPlayer", menuName = "AI/Behavior Tree/Action/MoveToPlayer")]
public class MoveToPlayer : ActionNode
{
    public override NodeState Tick()
    {
        if (player == null) return NodeState.Failure;

        Vector3 direction = (player.position - ai.transform.position).normalized;
        direction.y = 0;

        // Rotate towards player
        if (direction.magnitude > 0.1f)
        {
            ai.transform.rotation = Quaternion.Slerp(
                ai.transform.rotation,
                Quaternion.LookRotation(direction),
                5f * Time.deltaTime
            );
        }

        // Move
        if (ai.GetComponent<Enemy>().rb != null)
        {
            ai.GetComponent<Enemy>().rb.linearVelocity = new Vector3(
                direction.x * ai.moveSpeed,
                ai.GetComponent<Enemy>().rb.linearVelocity.y,
                direction.z * ai.moveSpeed
            );
        }

        ai.SetAnimatorState("Run");
        return NodeState.Running; // Keep running until close enough
    }
}

[CreateAssetMenu(fileName = "AttackPlayer", menuName = "AI/Behavior Tree/Action/AttackPlayer")]
public class AttackPlayer : ActionNode
{
    private float lastAttackTime = 0f;

    public override NodeState Tick()
    {
        if (Time.time - lastAttackTime < ai.attackCooldown)
            return NodeState.Running;

        ai.SetAnimatorState("Attack");
        lastAttackTime = Time.time;
        // Damage is handled by animation event or trigger
        return NodeState.Success;
    }
}

[CreateAssetMenu(fileName = "RetreatFromPlayer", menuName = "AI/Behavior Tree/Action/RetreatFromPlayer")]
public class RetreatFromPlayer : ActionNode
{
    public override NodeState Tick()
    {
        if (player == null) return NodeState.Failure;

        Vector3 direction = (ai.transform.position - player.position).normalized;
        direction.y = 0;

        // Slight rotation towards player
        if (direction.magnitude > 0.1f)
        {
            ai.transform.rotation = Quaternion.Slerp(
                ai.transform.rotation,
                Quaternion.LookRotation(direction),
                3f * Time.deltaTime
            );
        }

        // Move away slowly
        if (ai.GetComponent<Enemy>().rb != null)
        {
            ai.GetComponent<Enemy>().rb.linearVelocity = new Vector3(
                direction.x * ai.retreatSpeed,
                ai.GetComponent<Enemy>().rb.linearVelocity.y,
                direction.z * ai.retreatSpeed
            );
        }

        ai.SetAnimatorState("BackWalk");
        return NodeState.Running;
    }
}

[CreateAssetMenu(fileName = "Idle", menuName = "AI/Behavior Tree/Action/Idle")]
public class Idle : ActionNode
{
    public override NodeState Tick()
    {
        ai.SetAnimatorState("Idle");
        return NodeState.Success;
    }
}