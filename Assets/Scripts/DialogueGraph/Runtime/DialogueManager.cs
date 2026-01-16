using System;
using System.Collections.Generic;
using System.Xml.Linq;
using DialogueGraph.Shared;
using TMPro;
using Unity.GraphToolkit.Editor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public RuntimeDialogueGraph RuntimeGraph;

    [Header("Dialogue")] 
    [SerializeField] private LANGUAGE m_currentLanguage;
    [SerializeField] private TextAsset m_dialogueData;
    private DialogueTable m_dialogueTable = new DialogueTable();

    [Header("UI Components")] 
    public GameObject DialoguePanel;
    public TextMeshProUGUI SpeakerNameText;
    public TextMeshProUGUI DialogueText;

    [Header("Choice Button UI")]
    public Button choiceButtonPrefab;
    public Transform ChoiceButtonContainer;

    [Header("Speaker")]
    [SerializeField] private SpeakerDatatable SpeakerDatatable;
    [SerializeField] private RawImage HumeurRImage;
    [SerializeField] private RawImage SpeakerRImage;

    // Sound
    AudioSource audioSource;

    private Dictionary<string, RuntimeDialogueNode> _nodeLookup = new Dictionary<string, RuntimeDialogueNode>();
    private RuntimeDialogueNode _currentNode;

    private void Start()
    {
        // Init Dialogue Table
        m_dialogueTable.Load(m_dialogueData);
        
        // TO EDIT
        // audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) Debug.LogWarning("No Audio Source on Dialogue Manager");

        // NODES
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
        if (Mouse.current.leftButton.wasPressedThisFrame && _currentNode != null && _currentNode.Choices.Count == 0)
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
        SpeakerData currentSpeaker = SpeakerDatatable.GetSpeakerByKey(_currentNode.SpeakerKey);
        if (currentSpeaker == null)
            Debug.LogError($"Key Speaker {_currentNode.SpeakerKey} doesn't exist.");

        // -- dialogue --
        DialogueTable.Row dialogueRow = m_dialogueTable.Find_Key(_currentNode.DialogueKey.ToKey());
        
        DialoguePanel.SetActive(true);
        SpeakerNameText.SetText(currentSpeaker.Name);
        DialogueText.SetText(GetLocalizedText(dialogueRow));
           
        // -- choices --
        
        foreach (Transform child in ChoiceButtonContainer)
        {
            Destroy(child.gameObject);
        }

        if (_currentNode.Choices.Count > 0)
        {
            foreach (var choice in _currentNode.Choices)
            {
                Button button = Instantiate(choiceButtonPrefab, ChoiceButtonContainer);

                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = GetLocalizedText(m_dialogueTable.Find_Key(choice.ChoiceKey.ToKey()));
                }

                if(button != null)
                {
                    button.onClick.AddListener(() =>
                    {
                        if (!string.IsNullOrEmpty(choice.DesinationNodeID))
                        {
                            ShowNode(choice.DesinationNodeID);
                        }
                        else
                        {
                            EndDialogue();
                        }
                    });
                }
            }
        }

        // -- speaker --
        SpeakerRImage.texture = currentSpeaker.Sprite;

        if (_currentNode.SpeakerHumeur != HUMEUR.Defaut)
        {
            Texture2D text2D = currentSpeaker.GetTextByHumeur(_currentNode.SpeakerHumeur);
            HumeurRImage.texture = text2D;
        }

        audioSource.PlayOneShot(currentSpeaker.AudioClip);
    }

    private void EndDialogue()
    {
        DialoguePanel.SetActive(false); 
        _currentNode = null;
    }

    private string GetLocalizedText(DialogueTable.Row row)
    {
        switch (m_currentLanguage)
        {
            case LANGUAGE.Français:
                return row.FR;
            case LANGUAGE.Anglais:
                return row.AN;
            case LANGUAGE.Espagnol:
                return row.ES;
            default:
                return row.FR;
        }
    }
}
