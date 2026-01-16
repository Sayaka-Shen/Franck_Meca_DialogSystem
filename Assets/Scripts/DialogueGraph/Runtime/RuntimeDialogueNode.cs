using System;
using System.Collections.Generic;
using NUnit.Framework;
using DialogueGraph.Shared;

[Serializable]
public class RuntimeDialogueNode : RuntimeNode
{
    public string NextNodeId;

    // Dialogue
    public string DialogueKey;

    // Choices
    public List<ChoiceData> Choices = new List<ChoiceData>();

    // Speaker
    public string SpeakerKey; // TO EDIT key = SPK_FirstLetter of each name
    public HUMEUR SpeakerHumeur;
}
