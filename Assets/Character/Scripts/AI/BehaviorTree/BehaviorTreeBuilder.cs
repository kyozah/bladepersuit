using UnityEngine;

public class BehaviorTreeBuilder : MonoBehaviour
{
    public BehaviorTree CreateEnemyBehaviorTree()
    {
        BehaviorTree tree = ScriptableObject.CreateInstance<BehaviorTree>();

        // Root: Selector
        Selector root = ScriptableObject.CreateInstance<Selector>();

        // Branch 1: Idle if no player
        Sequence idleSequence = ScriptableObject.CreateInstance<Sequence>();
        idleSequence.children.Add(ScriptableObject.CreateInstance<Idle>());

        // Branch 2: Active attacker behavior
        Sequence activeSequence = ScriptableObject.CreateInstance<Sequence>();
        activeSequence.children.Add(ScriptableObject.CreateInstance<IsActiveAttacker>());
        Selector activeSelector = ScriptableObject.CreateInstance<Selector>();

        // Sub-branch: Attack if in range
        Sequence attackSequence = ScriptableObject.CreateInstance<Sequence>();
        attackSequence.children.Add(ScriptableObject.CreateInstance<PlayerInAttackRange>());
        attackSequence.children.Add(ScriptableObject.CreateInstance<AttackPlayer>());

        // Sub-branch: Chase if detected
        Sequence chaseSequence = ScriptableObject.CreateInstance<Sequence>();
        chaseSequence.children.Add(ScriptableObject.CreateInstance<PlayerInDetectionRange>());
        chaseSequence.children.Add(ScriptableObject.CreateInstance<MoveToPlayer>());

        activeSelector.children.Add(attackSequence);
        activeSelector.children.Add(chaseSequence);
        activeSequence.children.Add(activeSelector);

        // Branch 3: Retreat if not active
        Sequence retreatSequence = ScriptableObject.CreateInstance<Sequence>();
        Inverter notActive = ScriptableObject.CreateInstance<Inverter>();
        notActive.child = ScriptableObject.CreateInstance<IsActiveAttacker>();
        retreatSequence.children.Add(notActive);
        retreatSequence.children.Add(ScriptableObject.CreateInstance<RetreatFromPlayer>());

        root.children.Add(activeSequence);
        root.children.Add(retreatSequence);
        root.children.Add(idleSequence);

        tree.root = root;
        return tree;
    }
}