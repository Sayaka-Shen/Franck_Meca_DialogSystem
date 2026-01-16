using System;
using Codice.Client.Common;
using Unity.GraphToolkit.Editor;
using UnityEngine;

[Serializable]
public class OR : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
        context.AddOutputPort("out").Build();
    }
}


[Serializable]
public class IF : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();

        //ntext.AddInputPort<bool>("Condition").Build();
        context.AddInputPort<string> ("Condition").Build();
        context.AddOutputPort("True").Build();
        context.AddOutputPort("False").Build();
    }

}

