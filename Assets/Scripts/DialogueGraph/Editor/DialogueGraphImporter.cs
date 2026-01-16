using UnityEngine;
using UnityEditor.AssetImporters;
using Unity.GraphToolkit.Editor;
using System;
using System.Collections.Generic;
using DialogueGraph.Shared;
using System.Linq;
using Unity.GraphToolkit;
using UnityEditor;

[ScriptedImporter(1, DialogueGraphClass.AssetExtension)]
public class DialogueGraphImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        DialogueGraphClass editorGraph = GraphDatabase.LoadGraphForImporter<DialogueGraphClass>(ctx.assetPath);
        
        bool hasErrors = false;
        int nodeIndex = 1;
        
        foreach (INode node in editorGraph.GetNodes())
        {
            if (node is DialogueNode dialogueNode)
            {
                hasErrors |= ValidateDialogueNode(dialogueNode, nodeIndex);
                nodeIndex++;
            }
            else if (node is ChoiceNode choiceNode)
            {
                hasErrors |= ValidateChoiceNode(choiceNode, nodeIndex);
                nodeIndex++;
            }
        }
        
        if (hasErrors)
        {
            Debug.LogWarning($"<color=orange><b>Le graph contient des erreurs de validation!</b></color>");
        }
        else
        {
            Debug.Log($"<color=green><b>Tous les nodes sont valides!</b></color>");
        }
        
        RuntimeDialogueGraph runtimeGraph = ScriptableObject.CreateInstance<RuntimeDialogueGraph>();
        Dictionary<INode, string> nodeIdMap = new Dictionary<INode, string>();

        foreach (INode node in editorGraph.GetNodes())
        {
            nodeIdMap[node] = Guid.NewGuid().ToString();
        }

        INode startNode = editorGraph.GetNodes().OfType<INode>().FirstOrDefault();
        if (startNode != null)
        {
            IPort entryPort = startNode.GetOutputPorts().FirstOrDefault()?.firstConnectedPort;
            if (entryPort != null)
            {
                runtimeGraph.EntryNodeId = nodeIdMap[entryPort.GetNode()];
            }
        }
        
        foreach (INode node in editorGraph.GetNodes())
        {
            if (node is DialogueStartNode || node is DialogueEndNode) continue;
            
            RuntimeDialogueNode runtimeNode = new RuntimeDialogueNode{ NodeId = nodeIdMap[node] };
            if (node is DialogueNode dialogueNode)
            {
                ProcessDialogueNode(dialogueNode, runtimeNode, nodeIdMap);
            }
            else if (node is ChoiceNode choiceNode)
            {
                ProcessChoiceNode(choiceNode, runtimeNode, nodeIdMap);
            }

            runtimeGraph.AllNodes.Add(runtimeNode);
        }
        
        ctx.AddObjectToAsset("RuntimeData", runtimeGraph);
        ctx.SetMainObject(runtimeGraph);
        
    }

    private void ProcessDialogueNode(DialogueNode node, RuntimeDialogueNode runtimeNode,
        Dictionary<INode, string> nodeIdMap)
    {
        runtimeNode.SpeakerName = GetPortValue<string>(node.GetInputPortByName("Speaker"));
        runtimeNode.DialogueKey = GetPortValue<DialogueKey>(node.GetInputPortByName("DialogueKey"));

        // Speaker
        runtimeNode.SpeakerKey = GetPortValue<string>(node.GetInputPortByName("SpeakerKey"));
        // when save if key doesn't exit print error

        runtimeNode.SpeakerHumeur = (HUMEUR)GetPortValue<int>(node.GetInputPortByName("Humeur"));

        IPort nextNodePort = node.GetOutputPortByName("out")?.firstConnectedPort;
        if (nextNodePort != null)
        {
            runtimeNode.NextNodeId = nodeIdMap[nextNodePort.GetNode()];
        }
    }

    private void ProcessChoiceNode(ChoiceNode node, RuntimeDialogueNode runtimeNode,
        Dictionary<INode, string> nodeIdMap)
    {
        // dialogue 
        runtimeNode.SpeakerName = GetPortValue<string>(node.GetInputPortByName("Speaker"));
        runtimeNode.DialogueKey = GetPortValue<DialogueKey>(node.GetInputPortByName("DialogueKey"));

        //speaker
        runtimeNode.SpeakerKey = GetPortValue<string>(node.GetInputPortByName("SpeakerKey"));
        runtimeNode.SpeakerHumeur = (HUMEUR)GetPortValue<int>(node.GetInputPortByName("Humeur"));

        // choice
        var choiceOutputPorts = node.GetOutputPorts().Where(p => p.name.StartsWith("Choice "));

        foreach(var outputPort in choiceOutputPorts)
        {
            string index = outputPort.name.Substring("Choice ".Length);
            IPort textPort = node.GetInputPortByName($"ChoiceKey { index }");

            ChoiceData choiceData = new ChoiceData
            {
                ChoiceKey = GetPortValue<DialogueKey>(textPort),
                DesinationNodeID = outputPort.firstConnectedPort != null ? nodeIdMap[outputPort.firstConnectedPort.GetNode()] : null
            };

            runtimeNode.Choices.Add(choiceData);

        }

    }

    private T GetPortValue<T>(IPort port)
    {
        if (port == null) return default;

        if (port.isConnected)
        {
            if (port.firstConnectedPort.GetNode() is IVariableNode variableNode)
            {
                variableNode.variable.TryGetDefaultValue(out T value);
                return value;
            }
        }

        port.TryGetValue(out T fallbackValue);
        return fallbackValue;
    }


    private bool ValidateDialogueNode(DialogueNode node, int index)
    {
        Debug.Log($"<color=cyan>Node {index} - DialogueNode</color>");
        bool hasError = false;
        
        var speakerKeyPort = node.GetInputPortByName("SpeakerKey");
        if (speakerKeyPort != null && speakerKeyPort.TryGetValue(out string speakerKey) && !string.IsNullOrEmpty(speakerKey))
        {
            SpeakerData speakerData = GetSpeakerFromDatabase(speakerKey);
            if (speakerData != null)
            {
                Debug.Log($"<color=green> Speaker: { speakerData.Name } ({ speakerKey })</color>");
            }
        }
        else
        {
            Debug.LogWarning($"<color=yellow> Speaker non défini</color>");
        }

        IPort dialogueKeyPort = node.GetInputPortByName("DialogueKey");
        if (dialogueKeyPort != null && dialogueKeyPort.TryGetValue(out DialogueKey dialogueKey))
        {
            string key = dialogueKey.ToKey();
            if (!string.IsNullOrEmpty(key))
            {
                string dialogueText = GetDialogueFromDatabase(key);
                if (!string.IsNullOrEmpty(dialogueText))
                {
                    Debug.Log($"<color=green> Dialogue: \"{ dialogueText }\"</color>");
                }
            }
            else
            {
                Debug.LogWarning($"<color=yellow> DialogueKey non défini</color>");
            }
        }
        
        Debug.Log("");
        return hasError;
    }

    private bool ValidateChoiceNode(ChoiceNode node, int index)
    {
        Debug.Log($"<color=magenta> Node { index } - ChoiceNode</color>");
        bool hasError = false;
        
        IPort speakerKeyPort = node.GetInputPortByName("SpeakerKey");
        if (speakerKeyPort != null && speakerKeyPort.TryGetValue(out string speakerKey) && !string.IsNullOrEmpty(speakerKey))
        {
            var speakerData = GetSpeakerFromDatabase(speakerKey);
            if (speakerData != null)
            {
                Debug.Log($"<color=green>Speaker: {speakerData.Name} ({ speakerKey })</color>");
            }
        }

        IPort dialogueKeyPort = node.GetInputPortByName("DialogueKey");
        if (dialogueKeyPort != null && dialogueKeyPort.TryGetValue(out DialogueKey dialogueKey))
        {
            string key = dialogueKey.ToKey();
            if (!string.IsNullOrEmpty(key))
            {
                var dialogueText = GetDialogueFromDatabase(key);
                if (!string.IsNullOrEmpty(dialogueText))
                {
                    Debug.Log($"<color=green>Question: \"{ dialogueText }\"</color>");
                }
            }
        }

        INodeOption option = node.GetNodeOptionByName("portCount");
        option.TryGetValue(out int portCount);
        
        Debug.Log($"<color=cyan>Choix ({ portCount }):</color>");
        
        for (int i = 0; i < portCount; i++)
        {
            IPort choicePort = node.GetInputPortByName($"ChoiceKey {i}");
            
            if (choicePort != null && choicePort.TryGetValue(out DialogueKey choiceKey))
            {
                string key = choiceKey.ToKey();
                if (!string.IsNullOrEmpty(key))
                {
                    var text = GetDialogueFromDatabase(key);
                    if (!string.IsNullOrEmpty(text))
                    {
                        Debug.Log($"<color=green>{ i + 1 }.\"{ text }\"</color>");
                    }
                }
            }
        }
        
        Debug.Log("");
        return hasError;
    }

    private SpeakerData GetSpeakerFromDatabase(string key)
    {
        string[] guids = AssetDatabase.FindAssets("t:SpeakerDatatable");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var datatable = AssetDatabase.LoadAssetAtPath<SpeakerDatatable>(path);
            if (datatable != null)
            {
                var speaker = datatable.GetSpeakerByKey(key);
                if (speaker != null)
                    return speaker;
            }
        }
        return null;
    }

    private string GetDialogueFromDatabase(string key)
    {
        string[] guids = AssetDatabase.FindAssets("DialogueData t:TextAsset");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var dialogueAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (dialogueAsset != null)
            {
                DialogueTable table = new DialogueTable();
                table.Load(dialogueAsset);
                DialogueTable.Row row = table.Find_Key(key);
                if (row != null)
                    return row.FR;
            }
        }
        return null;
    }

}
