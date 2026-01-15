using UnityEngine;

public enum NodeState
{
    Success,
    Failure,
    Running
}

public abstract class BTNode : ScriptableObject
{
    public abstract NodeState Tick();
}