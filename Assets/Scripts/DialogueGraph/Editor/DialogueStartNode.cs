using System;
using Unity.GraphToolkit.Editor;

[Serializable]
public class DialogueStartNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddOutputPort("Francko").Build();
    }
}
