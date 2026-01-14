using System.Collections.Generic;
using UnityEngine;

public class RuntimeDialogueGraph : ScriptableObject
{
    public string EntryNodeId;
    public List<RuntimeDialogueNode> AllNodes = new List<RuntimeDialogueNode>();
}
