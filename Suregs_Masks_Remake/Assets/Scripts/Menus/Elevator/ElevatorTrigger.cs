using UnityEngine;

public class ElevatorTrigger : MonoBehaviour
{
    [SerializeField] private GameObject elevatorCanvas;

    private bool playerInside;

    public bool isFinal = false;

    bool alreadyUnlocked = false;

    private void Update()
    {
        if (!playerInside)
            return;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Fire1"))
        {
            elevatorCanvas.SetActive(true);

            if(isFinal && !alreadyUnlocked)
            {
                ElevatorLevelProgress.UnlockNextLevel();
                alreadyUnlocked = true;
            }

            Player.Instance.canMove = false;
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Player.Instance.canMove = true;
            elevatorCanvas.SetActive(false);
        }
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
