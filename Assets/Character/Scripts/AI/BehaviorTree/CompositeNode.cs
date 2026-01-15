using System.Collections.Generic;
using UnityEngine;

public abstract class CompositeNode : BTNode
{
    public List<BTNode> children = new List<BTNode>();
}

[CreateAssetMenu(fileName = "Selector", menuName = "AI/Behavior Tree/Composite/Selector")]
public class Selector : CompositeNode
{
    public override NodeState Tick()
    {
        foreach (var child in children)
        {
            var state = child.Tick();
            if (state != NodeState.Failure)
            {
                return state;
            }
        }
        return NodeState.Failure;
    }
}

[CreateAssetMenu(fileName = "Sequence", menuName = "AI/Behavior Tree/Composite/Sequence")]
public class Sequence : CompositeNode
{
    public override NodeState Tick()
    {
        foreach (var child in children)
        {
            var state = child.Tick();
            if (state != NodeState.Success)
            {
                return state;
            }
        }
        return NodeState.Success;
    }
}