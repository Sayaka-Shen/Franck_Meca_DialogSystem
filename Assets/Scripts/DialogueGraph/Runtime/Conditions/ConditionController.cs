using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class DialogueCondition
{
    public string Key;

    [SerializeField]
    private bool _value;

    public bool Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;

                // Save
                PlayerPrefs.SetInt(Key, _value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
    }
}


public class ConditionController : MonoBehaviour
{
    public List<DialogueCondition> Conditions = new List<DialogueCondition>();

}
