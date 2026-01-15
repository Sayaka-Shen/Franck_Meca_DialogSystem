using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;

[Serializable]
[Graph(AssetExtension)]
public class DialogueGraphClass : Graph
{
    public const string AssetExtension = "dialoguegraph";
    
    [MenuItem("Assets/Create/Dialogue Simplified Graph", false)]
    private static void CreateAssetFile()
    {
        GraphDatabase.PromptInProjectBrowserToCreateNewAsset<DialogueGraphClass>();
    }
}
