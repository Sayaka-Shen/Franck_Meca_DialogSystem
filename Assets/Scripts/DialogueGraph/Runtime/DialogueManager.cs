using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DialogueGraph.Shared;
using UnityEngine.Audio;

public class DialogueManager : MonoBehaviour
{
    public RuntimeDialogueGraph RuntimeGraph;

    [Header("Dialogue")]
    [SerializeField] private LANGUAGE m_currentLanguage;
    [SerializeField] private TextAsset m_dialogueData;
    private DialogueTable m_dialogueTable = new();

    [Header("UI")]
    public GameObject DialoguePanel;
    public TextMeshProUGUI SpeakerNameText;
    public TextMeshProUGUI DialogueText;

    [Header("Choices")]
    public Button choiceButtonPrefab;
    public Transform ChoiceButtonContainer;

    [Header("Speaker")]
    [SerializeField] private SpeakerDatatable SpeakerDatatable;
    [SerializeField] private RawImage HumeurRImage;
    [SerializeField] private RawImage SpeakerRImage;

    // Sound
    AudioSource audioSource;

    private Dictionary<string, RuntimeNode> lookup = new();
    private RuntimeNode _currentNode;

    void Start()
    {
        // Init Dialogue Table
        m_dialogueTable.Load(m_dialogueData);
        
        // TO EDIT
        // audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) Debug.LogWarning("No Audio Source on Dialogue Manager");


        foreach (var n in RuntimeGraph.AllNodes)
            lookup[n.NodeId] = n;

        ShowNode(RuntimeGraph.EntryNodeId);
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && _currentNode != null)
        {
            if(_currentNode is RuntimeDialogueNode rd && rd.Choices.Count == 0)
            {
                if (!string.IsNullOrEmpty(rd.NextNodeId))
                {
                    ShowNode(rd.NextNodeId);
                }
                else
                {
                    EndDialogue();
                }
            }
        }
    }

    // --- NODE ---
    void ShowNode(string id)
    {
        if (string.IsNullOrEmpty(id) || !lookup.TryGetValue(id, out _currentNode))
        {
            Debug.LogError($"NodeId {id} NOT FOUND");
            EndDialogue();
            return;
        }

        // EndNode
        if (_currentNode is RuntimeEndNode)
        {
            EndDialogue();
            return;
        }

        // IF Node
        if (_currentNode is RuntimeIFNode ifNode)
        {
            bool result = EvaluateCondition(ifNode.ConditionKey);
            ShowNode(result ? ifNode.TrueNodeId : ifNode.FalseNodeId);
            return;
        }

        var d = _currentNode as RuntimeDialogueNode;
        if (d == null)
        {
            EndDialogue();
            return;
        }

        var currentSpeaker = SpeakerDatatable.GetSpeakerByKey(d.SpeakerKey);
        DialogueTable.Row row = m_dialogueTable.Find_Key(d.DialogueKey.ToKey());

        DialoguePanel.SetActive(true);
        SpeakerNameText.text = currentSpeaker.Name;
        DialogueText.text = GetText(row);

        // ---- CHOICE ----
        // clean
        foreach (Transform c in ChoiceButtonContainer)
            Destroy(c.gameObject);

        // add
        foreach (var choice in d.Choices)
        {
            var btn = Instantiate(choiceButtonPrefab, ChoiceButtonContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text =
                GetText(m_dialogueTable.Find_Key(choice.ChoiceKey.ToKey()));

            btn.onClick.AddListener(() => ShowNode(choice.DesinationNodeID));
        }

        // ---- AUDIO ----
        audioSource.PlayOneShot(currentSpeaker.AudioClip);
    }

    void EndDialogue()
    {
        DialoguePanel.SetActive(false);
        _currentNode = null;
    }

    // --- CONDITIONS ---
    bool EvaluateCondition(string key)
    {
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    // --- TRADUCTION ---
    string GetText(DialogueTable.Row r)
    {
        return m_currentLanguage switch
        {
            LANGUAGE.Français => r.FR,
            LANGUAGE.Anglais => r.AN,
            LANGUAGE.Espagnol => r.ES,
            _ => r.FR
        };
    }
}
