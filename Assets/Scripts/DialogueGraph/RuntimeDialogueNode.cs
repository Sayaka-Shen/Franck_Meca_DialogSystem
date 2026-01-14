using System;
using System.Collections.Generic;
using NUnit.Framework;

[Serializable]
public class RuntimeDialogueNode
{
    public string NodeId;
    public string SpeakerName;
    public List<ChoiceData> Choices = new List<ChoiceData>();
    public string DialogueText;
    public string NextNodeId;
}
