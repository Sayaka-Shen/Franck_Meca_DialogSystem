using UnityEngine;
using UnityEditor.AssetImporters;
using Unity.GraphToolkit.Editor;
using System;
using System.Collections.Generic;
using System.Linq;

[ScriptedImporter(1, DialogueGraph.AssetExtension)]
public class DialogueGraphImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        DialogueGraph editorGraph = GraphDatabase.LoadGraphForImporter<DialogueGraph>(ctx.assetPath);
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

        IPort nextNodePort = node.GetOutputPortByName("out")?.firstConnectedPort;
        if (nextNodePort != null)
        {
            runtimeNode.NextNodeId = nodeIdMap[nextNodePort.GetNode()];
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
