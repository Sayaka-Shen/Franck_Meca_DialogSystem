using UnityEditor.AssetImporters;
using UnityEngine;
using Unity.GraphToolkit.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using DialogueGraph.Shared;
using UnityEditor;

[ScriptedImporter(1, DialogueGraphClass.AssetExtension)]
public class DialogueGraphImporter : ScriptedImporter
{

#if UNITY_EDITOR
    private ConditionRegistry conditionRegistry;
#endif

    public override void OnImportAsset(AssetImportContext ctx)
    {
        DialogueGraphClass editorGraph =GraphDatabase.LoadGraphForImporter<DialogueGraphClass>(ctx.assetPath);
        RuntimeDialogueGraph runtimeGraph = ScriptableObject.CreateInstance<RuntimeDialogueGraph>();

        Dictionary<INode, string> nodeIdMap = new();

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

        // --- PROCESS ---
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
        foreach (var node in runtimeGraph.AllNodes)
        {
            if (node is RuntimeDialogueNode d &&
                !string.IsNullOrEmpty(d.NextNodeId) &&
                !runtimeGraph.AllNodes.Any(n => n.NodeId == d.NextNodeId))
            {
                Debug.LogError($"❌ Broken link {node.NodeId} → {d.NextNodeId}");
            }

            if (node is RuntimeIFNode i)
            {
                if (!runtimeGraph.AllNodes.Any(n => n.NodeId == i.TrueNodeId))
                    Debug.LogError($"❌ IF TRUE broken: {i.TrueNodeId}");

                if (!runtimeGraph.AllNodes.Any(n => n.NodeId == i.FalseNodeId))
                    Debug.LogError($"❌ IF FALSE broken: {i.FalseNodeId}");
            }
        }

        ctx.AddObjectToAsset("RuntimeData", runtimeGraph);
        ctx.SetMainObject(runtimeGraph);
    }

    // --- DIALOGUE Node ---
    void ProcessDialogueNode(DialogueNode node, RuntimeDialogueNode r,
        Dictionary<INode, string> map)
    {
        r.DialogueKey = GetPortValue<string>(node.GetInputPortByName("DialogueKey"));
        r.SpeakerKey = GetPortValue<string>(node.GetInputPortByName("SpeakerKey"));
        r.SpeakerHumeur = (HUMEUR)GetPortValue<int>(node.GetInputPortByName("Humeur"));

        var outPort = node.GetOutputPortByName("out")?.firstConnectedPort;
        if (outPort != null)
            r.NextNodeId = map[outPort.GetNode()];
    }

    // --- CHOICE Node ---
    void ProcessChoiceNode(ChoiceNode node, RuntimeDialogueNode r,
        Dictionary<INode, string> map)
    {
        r.DialogueKey = GetPortValue<string>(node.GetInputPortByName("DialogueKey"));
        r.SpeakerKey = GetPortValue<string>(node.GetInputPortByName("SpeakerKey"));
        r.SpeakerHumeur = (HUMEUR)GetPortValue<int>(node.GetInputPortByName("Humeur"));

        foreach (var port in node.GetOutputPorts().Where(p => p.name.StartsWith("Choice ")))
        {
            string idx = port.name.Replace("Choice ", "");
            var keyPort = node.GetInputPortByName($"ChoiceKey {idx}");

            r.Choices.Add(new ChoiceData
            {
                ChoiceKey = GetPortValue<string>(keyPort),
                DesinationNodeID = port.firstConnectedPort != null
                    ? map[port.firstConnectedPort.GetNode()]
                    : null
            });
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
}
