using System.Collections.Generic;
using UnityEngine;

public static class DialogueMemory
{
    private const string Prefix = "DialogueSeen_";

    private static HashSet<string> seen = new HashSet<string>();

    public static string MakeKey(DialogueData data, int index)
    {
        return data.name + "_" + index;
    }

    public static bool HasSeen(string key)
    {
        if (seen.Contains(key))
            return true;

        if (PlayerPrefs.GetInt(Prefix + key, 0) == 1)
        {
            seen.Add(key);
            return true;
        }

        return false;
    }

    public static void MarkSeen(string key)
    {
        if (seen.Contains(key))
            return;

        seen.Add(key);

        PlayerPrefs.SetInt(Prefix + key, 1);
        PlayerPrefs.Save();
    }

    public static void ResetMemory()
    {
        foreach (string key in seen)
            PlayerPrefs.DeleteKey(Prefix + key);

        seen.Clear();
        PlayerPrefs.Save();
    }
}