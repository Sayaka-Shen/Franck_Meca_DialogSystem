using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class DialogueEndNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
    }
}
