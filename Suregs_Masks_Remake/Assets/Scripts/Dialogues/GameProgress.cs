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
    public static GameProgressState CurrentState { get; private set; }
        = GameProgressState.Start;

    public static void SetState(GameProgressState newState)
    {
        CurrentState = newState;
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

        UnityEngine.Debug.Log("GameProgress: " + CurrentState);
    }
}