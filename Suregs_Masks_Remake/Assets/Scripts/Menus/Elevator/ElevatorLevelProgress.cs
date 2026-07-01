using UnityEngine;

public static class ElevatorLevelProgress
{
    private const int START_UNLOCKED = 3;

    static void Init()
    {
        if (!PlayerPrefs.HasKey("UnlockedLevels"))
        {
            PlayerPrefs.SetInt("UnlockedLevels", START_UNLOCKED);
            PlayerPrefs.Save();
        }
    }

    public static void UnlockNextLevel()
    {
        Init();

        int unlocked = PlayerPrefs.GetInt("UnlockedLevels");

        unlocked++;

        PlayerPrefs.SetInt("UnlockedLevels", unlocked);
        PlayerPrefs.Save();
    }

    public static bool IsUnlocked(int levelIndex)
    {
        Init();

        int unlocked = PlayerPrefs.GetInt("UnlockedLevels");

        return levelIndex < unlocked;
    }
}