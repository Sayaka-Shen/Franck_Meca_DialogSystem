
#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConditionController))]
public class ConditionControllerEditor : Editor
{
    ConditionController controller;
    ConditionRegistry registry;

    private void OnEnable()
    {
        controller = (ConditionController)target;

        // load condition registry from path
        registry = AssetDatabase.LoadAssetAtPath<ConditionRegistry>("Assets/Scripts/DialogueGraph/Runtime/Conditions/ConditionRegistry.asset");
        if (registry == null)
            Debug.LogError("ConditionRegistry wasn't found");
    }

    public override void OnInspectorGUI()
    {
        if (controller == null)
            return;

        if (controller.Conditions == null)
            controller.Conditions = new List<DialogueCondition>();

        // list of conditions
        for (int i = 0; i < controller.Conditions.Count; i++)
        {
            var condition = controller.Conditions[i];
            EditorGUILayout.BeginHorizontal();

            // Dropdown from condition registry
            int selectedIndex = 0;
            if (registry != null && registry.Conditions != null)
            {
                selectedIndex = Mathf.Max(0, registry.Conditions.IndexOf(condition.Key));
                selectedIndex = EditorGUILayout.Popup(selectedIndex, registry.Conditions.ToArray());
                condition.Key = registry.Conditions[selectedIndex];
            }
            else
            {
                condition.Key = EditorGUILayout.TextField(condition.Key);
            }

            // Toggle
            condition.Value = EditorGUILayout.Toggle(condition.Value);

            // supp
            if (GUILayout.Button("X"))
            {
                controller.Conditions.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndHorizontal();
        }

        // Add condition
        if (GUILayout.Button("Add Condition"))
        {
            // find unused key
            string newKey = null;

            if (registry != null)
            {
                foreach (var key in registry.Conditions)
                {
                    if (!controller.Conditions.Exists(c => c.Key == key))
                    {
                        newKey = key;
                        break;
                    }
                }
            }

            // no key available => don't add
            if (!string.IsNullOrEmpty(newKey))
                controller.Conditions.Add(new DialogueCondition { Key = newKey, Value = false });
        }

        // Save
        if (GUI.changed)
            EditorUtility.SetDirty(controller);
    }
}
#endif