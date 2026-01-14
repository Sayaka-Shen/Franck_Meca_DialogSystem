using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class Class_Portals : MonoBehaviour
{
    [Header("Portal Parameters")]
    [SerializeField] protected int PortalID;
    [SerializeField] protected Vector3 PortalPosition;
    [SerializeField] protected GameObject[] ExitPortals;

    [Header("Portal Visuals")]
    [SerializeField] private RenderTexture PortalBackground;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //[EditorButton("TestFunction")]
    //public string o;
    public void TestFunction()
    {
        Debug.Log("aezrt");
    }
}

[CustomEditor(typeof(MB_Portal))]
public class MB_PortalEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var source = target as MB_Portal;

        base.OnInspectorGUI();

        if (GUILayout.Button("Start Function"))
        {
            source.TestFunction();
        }
    }
}