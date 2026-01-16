using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Runtime Graph")]
public class RuntimeDialogueGraph : ScriptableObject
{
    [SerializeReference] 
    public List<RuntimeNode> AllNodes = new();

    [SerializeField]
    public string EntryNodeId;
}