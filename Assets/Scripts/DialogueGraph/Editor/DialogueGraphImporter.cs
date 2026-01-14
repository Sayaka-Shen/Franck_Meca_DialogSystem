using UnityEngine;
using UnityEditor.AssetImporters;
using Unity.GraphToolkit.Editor;
using System;
using System.Collections.Generic;
using DialogueGraph.Shared;
using System.Linq;
using Unity.GraphToolkit;

[ScriptedImporter(1, DialogueGraphClass.AssetExtension)]
public class DialogueGraphImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        DialogueGraphClass editorGraph = GraphDatabase.LoadGraphForImporter<DialogueGraphClass>(ctx.assetPath);
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
        runtimeNode.DialogueText = GetPortValue<string>(node.GetInputPortByName("Dialogue"));

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
        runtimeNode.DialogueText = GetPortValue<string>(node.GetInputPortByName("Dialogue"));

        //speaker
        runtimeNode.SpeakerKey = GetPortValue<string>(node.GetInputPortByName("SpeakerKey"));
        runtimeNode.SpeakerHumeur = (HUMEUR)GetPortValue<int>(node.GetInputPortByName("Humeur"));

        // choice
        var choiceOutputPorts = node.GetOutputPorts().Where(p => p.name.StartsWith("Choice "));

        foreach(var outputPort in choiceOutputPorts)
        {
            var index = outputPort.name.Substring("Choice ".Length);
            var textPort = node.GetInputPortByName($"Choice Text {index}");

            var choiceData = new ChoiceData
            {
                ChoiceText = GetPortValue<string>(textPort),
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



}
