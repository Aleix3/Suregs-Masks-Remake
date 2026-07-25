using UnityEngine;

public enum GameProgressState
{
    Start,
    PostTraining,
    DunZero,
    DunOne,
    DunTwo,
    DunThree,
    DunFour,
    DunFive,
    DunSix
}

public static class GameProgress
{
    private const string PROGRESS_KEY = "GameProgress_CurrentState";

    public static GameProgressState CurrentState { get; private set; }
        = GameProgressState.Start;

    // Constructor estático: se ejecuta una única vez, la primera vez que
    // algo accede a la clase, y carga el progreso guardado (si existe).
    static GameProgress()
    {
        Load();
    }

    public static void SetState(GameProgressState newState)
    {
        CurrentState = newState;
        Save();
    }

    public static bool Is(GameProgressState state)
    {
        return CurrentState == state;
    }

    public static bool AtLeast(GameProgressState state)
    {
        return CurrentState >= state;
    }

    public static void Advance()
    {
        int next = (int)CurrentState + 1;
        if (next > (int)GameProgressState.DunSix)
            next = 0;
        CurrentState = (GameProgressState)next;
        Save();
        Debug.Log("GameProgress: " + CurrentState);
    }

    private static void Save()
    {
        PlayerPrefs.SetInt(PROGRESS_KEY, (int)CurrentState);
        PlayerPrefs.Save();
    }

    private static void Load()
    {
        int savedValue = PlayerPrefs.GetInt(PROGRESS_KEY, (int)GameProgressState.Start);

        // Por seguridad, si el enum cambia de tamaño entre versiones y el valor
        // guardado queda fuera de rango, volvemos a Start en vez de romper.
        if (savedValue < 0 || savedValue > (int)GameProgressState.DunSix)
            savedValue = (int)GameProgressState.Start;

        CurrentState = (GameProgressState)savedValue;
    }

    // Útil para un botón de "Nueva partida" / reset de progreso
    public static void ResetProgress()
    {
        CurrentState = GameProgressState.Start;
        PlayerPrefs.DeleteKey(PROGRESS_KEY);

        DialogueMemory.ResetMemory();

        PlayerPrefs.Save();
    }
}