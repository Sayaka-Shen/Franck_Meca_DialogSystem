using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class CSVToEnumGenerator : EditorWindow
{
    private TextAsset m_csvFile;
    private string m_enumName = "DialoguesKeys";
    private string m_outputPath = "Assets/Scripts/DialogueGraph/Shared/Dialogue";
    private bool m_generateExtensions = true;
    private bool m_separateFile = true;
    
    [MenuItem("Tools/CSV to Enum Generator")]
    public static void ShowWindow()
    {
        GetWindow<CSVToEnumGenerator>("CSV to Enum");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("CSV to Enum Generator", EditorStyles.boldLabel);
        
        m_csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", m_csvFile, typeof(TextAsset), false);
        m_enumName = EditorGUILayout.TextField("Enum Name", m_enumName);
        m_outputPath = EditorGUILayout.TextField("Output Path", m_outputPath);
        
        GUILayout.Space(10);
        
        m_generateExtensions = EditorGUILayout.Toggle("Generate Extensions", m_generateExtensions);
        if (m_generateExtensions)
        {
            m_separateFile = EditorGUILayout.Toggle("Separate File", m_separateFile);
        }
        
        if (GUILayout.Button("Generate Enum"))
        {
            GenerateEnum();
        }
    }
    
    private void GenerateEnum()
    {
        if (!m_csvFile)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a CSV file", "OK");
            return;
        }
        
        // Store the keys from the CSV
        List<string> keys = new List<string>();
        
        // Cut text by line and store it
        string[] lines = m_csvFile.text.Split('\n');
        if (lines.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "CSV file is empty", "OK");
            return;
        }
        
        // Header (first line), split words (column) with comma
        string[] headers = lines[0].Split(',');
        // Index of the column key
        int keyIndex = -1;
        
        // Go through all the rows and try to find the key row
        for (int i = 0; i < headers.Length; i++)
        {
            if (headers[i].Trim().ToLower() == "key")
            {
                keyIndex = i;
                break;
            }
        }
        
        if (keyIndex == -1)
        {
            EditorUtility.DisplayDialog("Error", "Column 'key' not found in CSV", "OK");
            return;
        }
        
        // Iterate through all the lines (starting with 1 not the header) 
        for (int i = 1; i < lines.Length; i++)
        {
            // No empty line
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            
            // Split line in columns
            string[] values = lines[i].Split(',');
            
            // Check for a key
            if (values.Length > keyIndex)
            {
                // Get key value without space
                string key = values[keyIndex].Trim();
                
                // Check if key is there or not already there
                if (!string.IsNullOrEmpty(key) && !keys.Contains(key))
                {
                    // Clean and add keys to be usable in enum c#
                    key = SanitizeEnumValue(key);
                    keys.Add(key);
                }
            }
        }
        
        StringBuilder m_enumSb = new StringBuilder();
        m_enumSb.AppendLine("// Auto-generated from CSV");
        m_enumSb.AppendLine("// Do not modify manually");
        m_enumSb.AppendLine();
        m_enumSb.AppendLine($"public enum { m_enumName }");
        m_enumSb.AppendLine("{");
        
        foreach (string key in keys)
        {
            m_enumSb.AppendLine($"{ key },");
        }
        
        m_enumSb.AppendLine("}");
        
        if (!Directory.Exists(m_outputPath))
        {
            Directory.CreateDirectory(m_outputPath);
        }
        
        string enumFilePath = Path.Combine(m_outputPath, $"{ m_enumName }.cs");
        File.WriteAllText(enumFilePath, m_enumSb.ToString());
        
        if (m_generateExtensions)
        {
            StringBuilder extSb = new StringBuilder();
            extSb.AppendLine("// Auto-generated from CSV");
            extSb.AppendLine("// Do not modify manually");
            extSb.AppendLine();
            extSb.AppendLine("using System.Collections.Generic;");
            extSb.AppendLine();
            extSb.AppendLine($"public static class { m_enumName }Extensions");
            extSb.AppendLine("{");
            extSb.AppendLine($"    private static readonly Dictionary<{ m_enumName }, string> _enumToString = new Dictionary<{ m_enumName }, string>");
            extSb.AppendLine("    {");
            
            foreach (string key in keys)
            {
                extSb.AppendLine($"{{ { m_enumName }.{ key }, \"{ key }\" }},");
            }
            
            extSb.AppendLine("};");
            extSb.AppendLine();
            extSb.AppendLine($"public static string ToKey(this { m_enumName } enumValue)");
            extSb.AppendLine("{");
            extSb.AppendLine("   return _enumToString.TryGetValue(enumValue, out string key) ? key : enumValue.ToString();");
            extSb.AppendLine("}");
            extSb.AppendLine();
            extSb.AppendLine($"public static { m_enumName } FromKey(string key)");
            extSb.AppendLine("{");
            extSb.AppendLine($"foreach (var kvp in _enumToString)");
            extSb.AppendLine("{");
            extSb.AppendLine("    if (kvp.Value == key) return kvp.Key;");
            extSb.AppendLine("    }");
            extSb.AppendLine($"      return default({m_enumName});");
            extSb.AppendLine("    }");
            extSb.AppendLine("}");
            
            if (m_separateFile)
            {
                string extFilePath = Path.Combine(m_outputPath, $"{ m_enumName }Extensions.cs");
                File.WriteAllText(extFilePath, extSb.ToString());
            }
            else
            {
                File.AppendAllText(enumFilePath, "\n" + extSb.ToString());
            }
        }
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", $"Enum generated at { enumFilePath }", "OK");
    }
    
    private string SanitizeEnumValue(string value)
    {
        // Change invalid char
        value = value.Replace(" ", "_")
                     .Replace("-", "_")
                     .Replace(".", "_");
        
        // Only start with a letter 
        if (char.IsDigit(value[0]))
        {
            value = "_" + value;
        }
        
        // Keep only numbers, underscores 
        StringBuilder sb = new StringBuilder();
        foreach (char c in value)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                sb.Append(c);
            }
        }
        
        return sb.ToString();
    }
}