using System;
using System.Collections.Generic;
using TMPro;
using Unity.GraphToolkit.Editor;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public RuntimeDialogueGraph RuntimeGraph;

    [Header("UI Components")] 
    public GameObject DialoguePanel;
    public TextMeshProUGUI SpeakerNameText;
    public TextMeshProUGUI DialogueText;
    
    private Dictionary<string, RuntimeDialogueNode> _nodeLookup = new Dictionary<string, RuntimeDialogueNode>();
    private RuntimeDialogueNode _currentNode;

    private void Start()
    {
        foreach (var node in RuntimeGraph.AllNodes)
        {
            _nodeLookup[node.NodeId] = node;
        }

        if (!string.IsNullOrEmpty(RuntimeGraph.EntryNodeId))
        {
            ShowNode(RuntimeGraph.EntryNodeId);
        }
        else
        {
            EndDialogue();
        }
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && _currentNode != null)
        {
            if (!string.IsNullOrEmpty(_currentNode.NextNodeId))
            {
                ShowNode(_currentNode.NextNodeId);
            }
            else
            {
                EndDialogue();
            }
        }
    }

    private void ShowNode(string nodeId)
    {
        if (!_nodeLookup.ContainsKey(nodeId))
        {
            EndDialogue();
            return;
        }
        
        _currentNode = _nodeLookup[nodeId];
        
        DialoguePanel.SetActive(true);
        SpeakerNameText.SetText(_currentNode.SpeakerName);
        DialogueText.SetText(_currentNode.DialogueText);
    }

    private void EndDialogue()
    {
        DialoguePanel.SetActive(false); 
        _currentNode = null;
    }
}
