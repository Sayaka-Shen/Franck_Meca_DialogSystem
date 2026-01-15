using System;
using System.Collections.Generic;
using Codice.Client.Common;
using Unity.GraphToolkit.Editor;
using UnityEngine;

[Serializable]
public class ChoiceNode : Node
{
    const string optionID = "portCount";
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
        
        context.AddInputPort<string>("Speaker").Build();
        context.AddInputPort<string>("DialogueKey").Build();

        // Speaker
        context.AddInputPort<string>("SpeakerKey").Build();
        context.AddInputPort<int>("Humeur").Build();

        var option = GetNodeOptionByName(optionID);
        option.TryGetValue(out int portCount);
        for (int i = 0; i < portCount; i++)
        {
            context.AddInputPort<string>($"ChoiceKey {i}").Build();
            context.AddOutputPort($"Choice {i}").Build();
        }
    }

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        context.AddOption<int>(optionID).Delayed().WithDefaultValue(2);
        //, defaultValue: 2, attributes: new Attribute[] { new DelayedAttribute() }
    }
}
