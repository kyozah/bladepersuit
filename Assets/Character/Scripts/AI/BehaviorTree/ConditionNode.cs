using UnityEngine;

public abstract class ConditionNode : BTNode
{
    protected EnemyBehaviorTree ai;
    protected Transform player;

    public void SetContext(EnemyBehaviorTree ai, Transform player)
    {
        this.ai = ai;
        this.player = player;
    }
}

[CreateAssetMenu(fileName = "PlayerInDetectionRange", menuName = "AI/Behavior Tree/Condition/PlayerInDetectionRange")]
public class PlayerInDetectionRange : ConditionNode
{
    public override NodeState Tick()
    {
        if (player == null) return NodeState.Failure;
        float distance = Vector3.Distance(ai.transform.position, player.position);
        return distance <= ai.detectionRange ? NodeState.Success : NodeState.Failure;
    }
}

[CreateAssetMenu(fileName = "PlayerInAttackRange", menuName = "AI/Behavior Tree/Condition/PlayerInAttackRange")]
public class PlayerInAttackRange : ConditionNode
{
    public override NodeState Tick()
    {
        if (player == null) return NodeState.Failure;
        float distance = Vector3.Distance(ai.transform.position, player.position);
        return distance <= ai.attackRange ? NodeState.Success : NodeState.Failure;
    }
}

[CreateAssetMenu(fileName = "IsActiveAttacker", menuName = "AI/Behavior Tree/Condition/IsActiveAttacker")]
public class IsActiveAttacker : ConditionNode
{
    public override NodeState Tick()
    {
        return EnemyAttackManager.IsActiveAttacker(ai.GetComponent<Enemy>()) ? NodeState.Success : NodeState.Failure;
    }
}