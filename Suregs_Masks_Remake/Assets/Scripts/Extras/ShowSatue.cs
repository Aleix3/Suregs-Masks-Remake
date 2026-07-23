using UnityEngine;

public class ShowStatue : MonoBehaviour, IInteractable
{
    public GameObject statueCanvas;

    public bool LockPlayerMovement => true;

    private bool isOpen = false;

    private Player currentPlayer;

    void Start()
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.townMusic);
        statueCanvas.SetActive(false);
    }

    public void Interact(Player player)
    {
        currentPlayer = player;

        if(isOpen)
        {
            StopInteract(player);
            return;
        }

        isOpen = true;

        statueCanvas.SetActive(true);

        Player.Instance.LockMovement(this);
    }

    public void StopInteract(Player player)
    {
        isOpen = false;

        statueCanvas.SetActive(false);

        Player.Instance.UnlockMovement(this);
    }

    private void Update()
    {
        if (!isOpen) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopInteract(currentPlayer);
        }
    }
}