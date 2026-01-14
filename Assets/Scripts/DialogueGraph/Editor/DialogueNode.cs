using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;

[Serializable]
public class DialogueNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
        context.AddOutputPort("out").Build();
        
        context.AddInputPort<string>("Speaker").Build();
        context.AddInputPort<string>("Dialogue").Build();
    }
}
