using System;
using System.Collections.Generic;
using NUnit.Framework;
using DialogueGraph.Shared;

[Serializable]
public class RuntimeDialogueNode
{
    // Dialogue
    public string NodeId;
    public string SpeakerName;

    public string DialogueKey;
    public string NextNodeId;

    // Choices
    public List<ChoiceData> Choices = new List<ChoiceData>();

    // Speaker
    public string SpeakerKey; // TO EDIT key = SPK_FirstLetter of each name
    public HUMEUR SpeakerHumeur;

}
