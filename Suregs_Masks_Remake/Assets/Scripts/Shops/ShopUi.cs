using System.Collections.Generic;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public List<ShopButton> buttons = new List<ShopButton>();

    public int currentIndex = 0;

    public bool isOpen = false;

    void Start()
    {
        UpdateHover();
    }

    void Update()
    {
        if (!isOpen) return;

        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            currentIndex++;
            if (currentIndex >= buttons.Count)
                currentIndex = 0;

            UpdateHover();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = buttons.Count - 1;

            UpdateHover();
        }

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
        {
            buttons[currentIndex].Select();
        }
    }

    void UpdateHover()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].SetHover(i == currentIndex);
        }
    }

    public void Open()
    {
        isOpen = true;
        currentIndex = 0;
        UpdateHover();
        gameObject.SetActive(true);
    }

    public void Close()
    {
        isOpen = false;
        gameObject.SetActive(false);
    }
}
