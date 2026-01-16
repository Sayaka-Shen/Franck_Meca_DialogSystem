using System;
using System.Collections.Generic;
using Codice.Client.Common;
using Unity.GraphToolkit.Editor;
using UnityEngine;
using DialogueGraph.Shared;

[Serializable]
public class ChoiceNode : Node
{
    const string optionID = "portCount";
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
    
        context.AddInputPort<DialogueKey>("DialogueKey").Build();

        // Speaker
        context.AddInputPort<string>("SpeakerKey").Build();
        context.AddInputPort<HUMEUR>("Humeur").Build();

        var option = GetNodeOptionByName(optionID);
        option.TryGetValue(out int portCount);
        for (int i = 0; i < portCount; i++)
        {
            context.AddInputPort<DialogueKey>($"ChoiceKey {i}").Build();
            context.AddOutputPort($"Choice {i}").Build();
        }
    }


    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        context.AddOption<int>(optionID).Delayed().WithDefaultValue(2);
    }
}
