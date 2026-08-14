using UnityEngine;

public class ElevatorTrigger : MonoBehaviour
{
    [SerializeField] private GameObject elevatorCanvas;

    private bool playerInside;

    public bool isFinal = false;

    private bool alreadyUnlocked;

    public int dungeonId;

    private string UnlockKey => "DungeonUnlocked_" + dungeonId;

    private void Start()
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.dungeonMusic);

        // Cargar el estado guardado
        alreadyUnlocked = PlayerPrefs.GetInt(UnlockKey, 0) == 1;
    }

    private void Update()
    {
        if (!playerInside || DialogueManager.Instance.simpleMessageActive)
            return;

        if (MenuManager.Instance != null && MenuManager.Instance.IsMenuOpen)
            return;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Fire1"))
        {
            elevatorCanvas.SetActive(true);

            if (isFinal && !alreadyUnlocked)
            {
                switch (dungeonId)
                {
                    case 0:
                        GameProgress.Advance();
                        break;

                    case 1:
                        GameProgress.Advance();
                        QuestManager.Instance.CompleteMainStepById("10");
                        break;

                    case 2:
                        GameProgress.Advance();
                        QuestManager.Instance.CompleteMainStepById("12");
                        break;

                    case 3:
                        GameProgress.Advance();
                        QuestManager.Instance.CompleteMainStepById("14");
                        break;

                    case 4:
                        GameProgress.Advance();
                        QuestManager.Instance.CompleteMainStepById("17");
                        break;

                    case 5:
                        GameProgress.Advance();
                        QuestManager.Instance.CompleteMainStepById("19");
                        break;

                    case 6:
                        GameProgress.Advance();
                        QuestManager.Instance.CompleteMainStepById("21");
                        break;

                    case 7:
                        QuestManager.Instance.CompleteMainStepById("25");
                        break;

                    default:
                        break;
                }

                ElevatorLevelProgress.UnlockNextLevel();

                // Marcar este dungeon como desbloqueado
                alreadyUnlocked = true;
                PlayerPrefs.SetInt(UnlockKey, 1);
                PlayerPrefs.Save();
            }
        }
    }

    public void CloseCanvas()
    {
        elevatorCanvas.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = false;
        }
    }
}