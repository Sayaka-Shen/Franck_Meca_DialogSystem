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
{ DialogueKey.T_Q_1, "T_Q_1" },
{ DialogueKey.P_1_1, "P_1_1" },
{ DialogueKey.T_Q_2_1, "T_Q_2_1" },
{ DialogueKey.P_2_1, "P_2_1" },
{ DialogueKey.P_2_2, "P_2_2" },
{ DialogueKey.P_2_3, "P_2_3" },
{ DialogueKey.T_Q_3_1, "T_Q_3_1" },
{ DialogueKey.T_Q_3_2, "T_Q_3_2" },
{ DialogueKey.P_3_1, "P_3_1" },
{ DialogueKey.P_3_2, "P_3_2" },
{ DialogueKey.P_3_3, "P_3_3" },
{ DialogueKey.P_5_1, "P_5_1" },
{ DialogueKey.P_5_2, "P_5_2" },
{ DialogueKey.P_5_3, "P_5_3" },
{ DialogueKey.T_4_1, "T_4_1" },
{ DialogueKey.T_Q_4_2, "T_Q_4_2" },
{ DialogueKey.T_4_2, "T_4_2" },
{ DialogueKey.T_4_3, "T_4_3" },
{ DialogueKey.E_1_1, "E_1_1" },
{ DialogueKey.E_1_2, "E_1_2" },
{ DialogueKey.E_1_3, "E_1_3" },
{ DialogueKey.E_1_4, "E_1_4" },
{ DialogueKey.P_6_1, "P_6_1" },
{ DialogueKey.P_Q_1_3, "P_Q_1_3" },
{ DialogueKey.P_7_1, "P_7_1" },
{ DialogueKey.P_7_2, "P_7_2" },
{ DialogueKey.P_7_3, "P_7_3" },
{ DialogueKey.T_Q_8_1, "T_Q_8_1" },
{ DialogueKey.T_8_2, "T_8_2" },
{ DialogueKey.E_1_5, "E_1_5" },
{ DialogueKey.P_8_1, "P_8_1" },
{ DialogueKey.P_8_2, "P_8_2" },
{ DialogueKey.E_1_6, "E_1_6" },
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
