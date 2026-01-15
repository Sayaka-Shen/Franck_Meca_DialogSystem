#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OrbitCamera))]
public class OrbitCameraEditor : Editor
{
    SerializedProperty rotationAxisIndexProp;
    SerializedProperty turnModeIndexProp;

    SerializedProperty toggleKeyProp;
    SerializedProperty turnDurationProp;
    SerializedProperty angleAProp;
    SerializedProperty angleBProp;
    SerializedProperty bZoomDuringTurnProp;
    SerializedProperty zoomInAmountProp;
    SerializedProperty zoomCurveProp;

    static bool toggleRotationFoldout = true;

    void OnEnable()
    {
        rotationAxisIndexProp = serializedObject.FindProperty("rotationAxisIndex");
        turnModeIndexProp = serializedObject.FindProperty("turnModeIndex");
        toggleKeyProp = serializedObject.FindProperty("toggleKey");
        turnDurationProp = serializedObject.FindProperty("turnDuration");
        angleAProp = serializedObject.FindProperty("angleA");
        angleBProp = serializedObject.FindProperty("angleB");

        bZoomDuringTurnProp = serializedObject.FindProperty("bZoomDuringTurn");
        zoomInAmountProp = serializedObject.FindProperty("zoomInAmount");
        zoomCurveProp = serializedObject.FindProperty("zoomCurve");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(
            serializedObject,
            "rotationAxisIndex",
            "turnModeIndex",
            "toggleKey",
            "turnDuration",
            "angleA",
            "angleB",
            "bZoomDuringTurn",
            "zoomInAmount",
            "zoomCurve"
        );

        EditorGUILayout.Space(6);

        toggleRotationFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(toggleRotationFoldout, "Toggle Camera Rotation");
        if (toggleRotationFoldout)
        {
            EditorGUI.indentLevel++;

            if (toggleKeyProp != null) EditorGUILayout.PropertyField(toggleKeyProp);
            if (turnDurationProp != null) EditorGUILayout.PropertyField(turnDurationProp);
            if (angleAProp != null) EditorGUILayout.PropertyField(angleAProp);
            if (angleBProp != null) EditorGUILayout.PropertyField(angleBProp);

            EditorGUILayout.Space(3);

            EditorGUILayout.LabelField("Rotation Axis", EditorStyles.boldLabel);
            if (rotationAxisIndexProp != null)
            {
                int axis = Mathf.Clamp(rotationAxisIndexProp.intValue, 0, 2);
                axis = GUILayout.Toolbar(axis, new[] { "X", "Y", "Z" });
                rotationAxisIndexProp.intValue = axis;
            }
            else
            {
                EditorGUILayout.HelpBox("Missing field: rotationAxisIndex", MessageType.Error);
            }

            EditorGUILayout.Space(3);

            EditorGUILayout.LabelField("Turn Mode", EditorStyles.boldLabel);
            if (turnModeIndexProp != null)
            {
                int mode = Mathf.Clamp(turnModeIndexProp.intValue, 0, 1);
                mode = GUILayout.Toolbar(mode, new[] { "Snap", "Circle" });
                turnModeIndexProp.intValue = mode;
            }
            else
            {
                EditorGUILayout.HelpBox("Missing field: turnModeIndex", MessageType.Error);
            }

            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("Zoom During Turn", EditorStyles.boldLabel);

            if (bZoomDuringTurnProp != null) EditorGUILayout.PropertyField(bZoomDuringTurnProp);

            if (bZoomDuringTurnProp != null && bZoomDuringTurnProp.boolValue)
            {
                EditorGUI.indentLevel++;
                if (zoomInAmountProp != null) EditorGUILayout.PropertyField(zoomInAmountProp);
                if (zoomCurveProp != null) EditorGUILayout.PropertyField(zoomCurveProp);
                EditorGUI.indentLevel--;
            }


            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
