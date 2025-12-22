using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StoryPhase
{
    Start,
    PostTraining,
    Dungeon0,
    Dungeon1,
    Dungeon2,
    Dungeon3,
    Dungeon4,
    Dungeon5,
    Dungeon6,
    Final
}

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance;
    public StoryPhase currentPhase;

    private void Awake()
    {
        Instance = this;
    }
}

