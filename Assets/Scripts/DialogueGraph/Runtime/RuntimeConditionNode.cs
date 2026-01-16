using UnityEngine;

[System.Serializable]
public class RuntimeIFNode : RuntimeNode
{
    public string TrueNodeId;
    public string FalseNodeId;

    public string ConditionKey;
}