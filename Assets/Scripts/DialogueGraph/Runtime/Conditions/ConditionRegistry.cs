using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Condition Registry")]
public class ConditionRegistry : ScriptableObject
{
    [ReadOnly]
    public List<string> Conditions = new();

    // Register key in scriptable object after graph save
    // call in importer process IFNode
    public void Register(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        // no doublons
        if (!Conditions.Contains(key))
        {
            Conditions.Add(key);

        }
    }
}