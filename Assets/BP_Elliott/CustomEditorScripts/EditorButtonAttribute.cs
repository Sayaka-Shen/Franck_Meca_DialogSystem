using System;
using TMPro;
using UnityEditor;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class EditorButtonAttribute : PropertyAttribute
{
    public string call;

    public EditorButtonAttribute(string method)
    {
        call = method;
    }
}

[CustomPropertyDrawer(typeof(EditorButtonAttribute))]
public class EditorButton : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
        
        EditorButtonAttribute button = (EditorButtonAttribute)attribute;
        if (GUI.Button(position, button.call))
        {
            var obj = property.serializedObject.targetObject as MonoBehaviour;
            obj?.SendMessage(button.call);
        }
    }

    private void ParseString(string functionName)
    {

    }
}