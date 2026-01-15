using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;
using DialogueGraph.Shared;

[Serializable]
public class DialogueNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
        context.AddOutputPort("out").Build();
        
        context.AddInputPort<string>("Speaker").Build();
        context.AddInputPort<string>("DialogueKey").Build();

        // Speaker
        context.AddInputPort<string>("SpeakerKey").Build();
        context.AddInputPort<int>("Humeur").WithDefaultValue(0).Build();

    }
}
