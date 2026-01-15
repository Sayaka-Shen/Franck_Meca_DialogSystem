using System.Collections.Generic;
using DialogueGraph.Shared;
using NUnit.Framework;
using UnityEngine;
using static SpeakerData;

[CreateAssetMenu(fileName = "SpeakerDatatable", menuName = "Scriptable Objects/SpeakerDatatable")]
public class SpeakerDatatable : ScriptableObject
{
    public List<SpeakerData> datas = new List<SpeakerData>();

    public SpeakerData GetSpeaker(int ID) => datas[ID];

    // TO EDIT check null return (T model)
    public SpeakerData GetSpeakerByKey(string key)
    {
        foreach (var speaker in datas)
        {
            if (speaker.Key == key)
            {
                return speaker;
            }
        }
        return null;
    }

    private void OnValidate()
    {
        // TO EDIT => specifique à un objet changé
        foreach (var data in datas)
        {
            data.UpdateDebugInfo();
        }

        //Debug.Log("On validate");
    }
}

