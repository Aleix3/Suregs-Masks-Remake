using System.Collections.Generic;

public static class DialogueMemory
{
    private static HashSet<string> seen = new HashSet<string>();

    public static string MakeKey(DialogueData data, int index)
    {
        return data.name + "_" + index;
    }

    public static bool HasSeen(string key)
    {
        return seen.Contains(key);
    }

    public static void MarkSeen(string key)
    {
        seen.Add(key);
    }
}