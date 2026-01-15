using UnityEngine;

public abstract class DecoratorNode : BTNode
{
    public BTNode child;
}

[CreateAssetMenu(fileName = "Inverter", menuName = "AI/Behavior Tree/Decorator/Inverter")]
public class Inverter : DecoratorNode
{
    public override NodeState Tick()
    {
        var state = child.Tick();
        if (state == NodeState.Success)
            return NodeState.Failure;
        if (state == NodeState.Failure)
            return NodeState.Success;
        return state;
    }
}

[CreateAssetMenu(fileName = "Succeeder", menuName = "AI/Behavior Tree/Decorator/Succeeder")]
public class Succeeder : DecoratorNode
{
    public override NodeState Tick()
    {
        child.Tick();
        return NodeState.Success;
    }
}

[CreateAssetMenu(fileName = "Failer", menuName = "AI/Behavior Tree/Decorator/Failer")]
public class Failer : DecoratorNode
{
    public override NodeState Tick()
    {
        child.Tick();
        return NodeState.Failure;
    }
}