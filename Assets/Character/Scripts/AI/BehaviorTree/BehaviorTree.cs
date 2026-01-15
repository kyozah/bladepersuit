using UnityEngine;

[CreateAssetMenu(fileName = "New Behavior Tree", menuName = "AI/Behavior Tree")]
public class BehaviorTree : ScriptableObject
{
    public BTNode root;

    public void Initialize(EnemyBehaviorTree ai, Transform player)
    {
        InitializeNode(root, ai, player);
    }

    private void InitializeNode(BTNode node, EnemyBehaviorTree ai, Transform player)
    {
        if (node is ConditionNode condition)
        {
            condition.SetContext(ai, player);
        }
        else if (node is ActionNode action)
        {
            action.SetContext(ai, player);
        }
        else if (node is CompositeNode composite)
        {
            foreach (var child in composite.children)
            {
                InitializeNode(child, ai, player);
            }
        }
        else if (node is DecoratorNode decorator && decorator.child != null)
        {
            InitializeNode(decorator.child, ai, player);
        }
    }

    public NodeState Tick()
    {
        return root?.Tick() ?? NodeState.Failure;
    }
}