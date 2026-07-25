using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ElevatorButton : MonoBehaviour
{
    public bool locked;
    public string sceneName;
    public int levelIndex;
    public ElevatorTrigger elevatorTriggerScript;
    public ElevatorScript elevatorScript;

    [Header("Navigation")]
    public ElevatorButton up;
    public ElevatorButton down;
    public ElevatorButton left;
    public ElevatorButton right;

    [Header("Bloqueo por misión")]
    public bool isDungeonEntrance = false;

    public string pendingTasksSpeaker = "Jakov";
    public Sprite speakerSprite;

    [TextArea]
    public string pendingTasksMessage = "Todavía tengo cosas pendientes que hacer antes de volver a la mazmorra.";
    void Start()
    {
        locked = !ElevatorLevelProgress.IsUnlocked(levelIndex);

        if (!locked)
        {
            GetComponent<Image>().enabled = false;
        }
    }


    public void Press()
    {
        if (locked)
            return;

        if (SceneManager.GetActiveScene().name == sceneName)
            return;

        if (MenuManager.Instance != null && MenuManager.Instance.IsMenuOpen)
            return;


        if (isDungeonEntrance
            && QuestManager.Instance != null
            && !QuestManager.Instance.CurrentQuestAllowsDungeonEntry())
        {
            if (DialogueManager.Instance != null)
            {
                elevatorTriggerScript.CloseCanvas();
                DialogueManager.Instance.ShowSimpleMessage(pendingTasksSpeaker, pendingTasksMessage, speakerSprite);
            }

            return;
        }

        
        if (sceneName == "Town")
        {
            if (Player.Instance.isFacingLeft)
            {
                Player.Instance.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            }
            else
            {
                Player.Instance.transform.localScale = new Vector3(-0.55f, 0.55f, 0.55f);
            }
        }
        
        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    private IEnumerator FadeAndLoadScene(string sceneNamee)
    {
        yield return StartCoroutine(CameraManager.Instance.Fade(1));

        elevatorScript.UnlockPlayer();
        SceneManager.LoadScene(sceneNamee);
    }
}