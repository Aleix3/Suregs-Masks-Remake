using System.Collections.Generic;
using UnityEngine;

public class ElevatorScript : MonoBehaviour
{
    public ElevatorButton current;

    public GameObject hover;

    public float inputDelay = 0.2f;
    private float timer;

    private Vector2 lastMoveInput;

    public List<ElevatorButton> buttons;

    

    void Start()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].elevatorScript = this;
        }
        MoveHover();
    }

    private void OnEnable()
    {
        lockPlayer();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= inputDelay)
        {
            HandleMovement();
            timer = 0;
        }

        HandleSelect();
        MoveHover();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockPlayer();
            this.gameObject.SetActive(false);
        }
    }

    void HandleMovement()
    {
        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        // evitar diagonales raras
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            input.y = 0;
        else
            input.x = 0;


        if (input == Vector2.zero)
        {
            lastMoveInput = Vector2.zero;
            return;
        }

        if (input == lastMoveInput)
            return;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.selectClip);
        lastMoveInput = input;

        if (input.x > 0)
        {
            if (current.right != null)
                current = current.right;
        }
        else if (input.x < 0)
        {
            if (current.left != null)
                current = current.left;
        }
        else if (input.y > 0)
        {
            if (current.up != null)
                current = current.up;
        }
        else if (input.y < 0)
        {
            if (current.down != null)
                current = current.down;
        }
    }

    void MoveHover()
    {
        hover.transform.position = current.transform.position;
    }
    void HandleSelect()
    {
        if (Input.GetKeyDown(KeyCode.E) /*|| Input.GetButtonDown("Fire1")*/)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClip);
            current.Press();
        }
    }

    public void lockPlayer()
    {
        Player.Instance.LockMovement(this);
    }
    public void UnlockPlayer()
    {
        Player.Instance.UnlockMovement(this);
    }
}