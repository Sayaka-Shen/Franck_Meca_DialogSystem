using UnityEditor.AssetImporters;
using UnityEngine;
using Unity.GraphToolkit.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using DialogueGraph.Shared;
using Unity.GraphToolkit;
using UnityEditor;
using NUnit.Framework.Internal;

[ScriptedImporter(1, DialogueGraphClass.AssetExtension)]
public class DialogueGraphImporter : ScriptedImporter
{

#if UNITY_EDITOR
    private ConditionRegistry conditionRegistry;
#endif

    public override void OnImportAsset(AssetImportContext ctx)
    {
        DialogueGraphClass editorGraph = GraphDatabase.LoadGraphForImporter<DialogueGraphClass>(ctx.assetPath);
        RuntimeDialogueGraph runtimeGraph = ScriptableObject.CreateInstance<RuntimeDialogueGraph>();

        Dictionary<INode, string> nodeIdMap = new();

        // --- DEBUG ---
        bool hasErrors = false;
        int nodeIndex = 1;
        
        foreach (INode node in editorGraph.GetNodes())
        {
            // TO EDIT à voir si on peut pas le combiner en ValidateRuntimeNode()
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
            // validate if node
        }
        
        if (hasErrors)
        {
            Debug.LogWarning($"<color=orange><b>Le graph contient des erreurs de validation!</b></color>");
        }
        else
        {
            Debug.Log($"<color=green><b>Tous les nodes sont valides!</b></color>");
        }
        

        //  Génère TOUS les IDs
        foreach (var node in editorGraph.GetNodes())
            nodeIdMap[node] = Guid.NewGuid().ToString();

        // Start Node
        var startNode = editorGraph.GetNodes().OfType<DialogueStartNode>().FirstOrDefault();
        if (startNode == null)
        {
            Debug.LogError("No DialogueStartNode");
            return;
        }

        var startOut = startNode.GetOutputPorts().FirstOrDefault()?.firstConnectedPort;
        if (startOut == null)
        {
            Debug.LogError("StartNode has no connection");
            return;
        }

        runtimeGraph.EntryNodeId = nodeIdMap[startOut.GetNode()];

        //// --- PROCESS ---
        foreach (var node in editorGraph.GetNodes())
        {
            RuntimeNode runtimeNode = null;

            if (node is DialogueStartNode)
            {
                runtimeNode = new RuntimeNode();
            }
            else if (node is DialogueEndNode)
            {
                runtimeNode = new RuntimeEndNode();
            }
            else if (node is IF ifNode)
            {
                var rIf = new RuntimeIFNode();
                ProcessIfNode(ifNode, rIf, nodeIdMap);
                runtimeNode = rIf;
            }
            else if (node is DialogueNode dNode)
            {
                var r = new RuntimeDialogueNode();
                ProcessDialogueNode(dNode, r, nodeIdMap);
                runtimeNode = r;
            }
            else if (node is ChoiceNode cNode)
            {
                var r = new RuntimeDialogueNode();
                ProcessChoiceNode(cNode, r, nodeIdMap);
                runtimeNode = r;
            }

            if (runtimeNode != null)
            {
                runtimeNode.NodeId = nodeIdMap[node];
                runtimeGraph.AllNodes.Add(runtimeNode);
            }
        }


        // --- DEBUG ---

        //foreach (var node in runtimeGraph.AllNodes)
        //{
        //    if (node is RuntimeDialogueNode d &&
        //        !string.IsNullOrEmpty(d.NextNodeId) &&
        //        !runtimeGraph.AllNodes.Any(n => n.NodeId == d.NextNodeId))
        //    {
        //        Debug.LogError($"Broken link {node.NodeId} → {d.NextNodeId}");
        //    }

        //    if (node is RuntimeIFNode i)
        //    {
        //        if (!runtimeGraph.AllNodes.Any(n => n.NodeId == i.TrueNodeId))
        //            Debug.LogError($"IF TRUE broken: {i.TrueNodeId}");

        //        if (!runtimeGraph.AllNodes.Any(n => n.NodeId == i.FalseNodeId))
        //            Debug.LogError($"IF FALSE broken: {i.FalseNodeId}");
        //    }
        //}

        ctx.AddObjectToAsset("RuntimeData", runtimeGraph);
        ctx.SetMainObject(runtimeGraph);
    }

    // --- DIALOGUE Node ---
    void ProcessDialogueNode(DialogueNode node, RuntimeDialogueNode r,
        Dictionary<INode, string> map)
    {
        r.DialogueKey = GetPortValue<DialogueKey>(node.GetInputPortByName("DialogueKey"));
        r.SpeakerKey = GetPortValue<string>(node.GetInputPortByName("SpeakerKey"));
        r.SpeakerHumeur = GetPortValue<HUMEUR>(node.GetInputPortByName("Humeur"));

        //var port = node.GetInputPortByName("Humeur");
        //Debug.Log($"Humeur connected: {port.firstConnectedPort != null}");

        //var sourcePort = port.firstConnectedPort;
        //object test = sourcePort.;
        //if(sourcePort.TryGetValue(out var sourceVal)) Debug.Log(sourceVal);

        var outPort = node.GetOutputPortByName("out")?.firstConnectedPort;
        if (outPort != null)
            r.NextNodeId = map[outPort.GetNode()];
    }

    // --- CHOICE Node ---
    void ProcessChoiceNode(ChoiceNode node, RuntimeDialogueNode r,
        Dictionary<INode, string> map)
    {
        r.DialogueKey = GetPortValue<DialogueKey>(node.GetInputPortByName("DialogueKey"));
        r.SpeakerKey = GetPortValue<string>(node.GetInputPortByName("SpeakerKey"));
        r.SpeakerHumeur = GetPortValue<HUMEUR>(node.GetInputPortByName("Humeur"));

        foreach (var outputPort in node.GetOutputPorts().Where(p => p.name.StartsWith("Choice ")))
        {
            string index = outputPort.name.Substring("Choice ".Length);
            IPort textPort = node.GetInputPortByName($"ChoiceKey { index }");

            ChoiceData choiceData = new ChoiceData
            {
                ChoiceKey = GetPortValue<DialogueKey>(textPort),
                DesinationNodeID = outputPort.firstConnectedPort != null ? map[outputPort.firstConnectedPort.GetNode()] : null
            };

            r.Choices.Add(choiceData);

        }
    }

    // --- IF Node ---
    void ProcessIfNode(IF node, RuntimeIFNode r, Dictionary<INode, string> map)
    {
        r.ConditionKey = GetPortValue<string>(node.GetInputPortByName("Condition"));

#if UNITY_EDITOR
        // recup registry
        if (conditionRegistry == null)
            conditionRegistry = AssetDatabase.LoadAssetAtPath<ConditionRegistry>(
                "Assets/Scripts/DialogueGraph/Runtime/Conditions/ConditionRegistry.asset");

        // save key
        if (conditionRegistry != null && !string.IsNullOrEmpty(r.ConditionKey))
            conditionRegistry.Register(r.ConditionKey);
#endif

        var t = node.GetOutputPortByName("True")?.firstConnectedPort;
        var f = node.GetOutputPortByName("False")?.firstConnectedPort;

        if (t != null) r.TrueNodeId = map[t.GetNode()];
        if (f != null) r.FalseNodeId = map[f.GetNode()];
    }

    T GetPortValue<T>(IPort port)
    {
        if (port == null) return default;
        port.TryGetValue(out T val);
        return val;
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
                Debug.Log($"<color=green>Speaker: { speakerData.Name } ({ speakerKey })</color>");
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
