// Auto-generated from CSV
// Do not modify manually

using System.Collections.Generic;

public static class DialogueKeyExtensions
{
    private static readonly Dictionary<DialogueKey, string> _enumToString = new Dictionary<DialogueKey, string>
    {
        { DialogueKey.GT_1, "GT_1" },
        { DialogueKey.GT_2, "GT_2" },
        { DialogueKey.GT_3, "GT_3" },
        { DialogueKey.GT_Q_1, "GT_Q_1" },
        { DialogueKey.AM_C_1, "AM_C_1" },
        { DialogueKey.AM_C_2, "AM_C_2" },
        { DialogueKey.AM_C_3, "AM_C_3" },
        { DialogueKey.AM_CL_1, "AM_CL_1" },
        { DialogueKey.AM_CL_2, "AM_CL_2" },
        { DialogueKey.AM_CL_3, "AM_CL_3" },
    };

    public static string ToKey(this DialogueKey enumValue)
    {
        return _enumToString.TryGetValue(enumValue, out string key) ? key : enumValue.ToString();
    }

    public static DialogueKey FromKey(string key)
    {
        foreach (var kvp in _enumToString)
        {
            if (kvp.Value == key) return kvp.Key;
        }
        return default(DialogueKey);
    }
}
