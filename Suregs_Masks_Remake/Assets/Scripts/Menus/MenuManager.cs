using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MenuManager : MonoBehaviour
{
    public CanvasGroup[] canvases;

    private int currentIndex = 0;
    private bool activeMenu = false;

    void Start()
    {
        OcultarTodos();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            activeMenu = !activeMenu;

            if (activeMenu)
            {
                currentIndex = 0;
                MostrarCanvas(currentIndex);
            }
            else
            {
                OcultarTodos();
            }
        }

        if (!activeMenu) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            currentIndex++;

            if (currentIndex >= canvases.Length)
                currentIndex = 0;

            MostrarCanvas(currentIndex);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            currentIndex--;

            if (currentIndex < 0)
                currentIndex = canvases.Length - 1;

            MostrarCanvas(currentIndex);
        }
    }

    void MostrarCanvas(int index)
    {
        OcultarTodos();

        canvases[index].alpha = 1;
        canvases[index].interactable = true;
        canvases[index].blocksRaycasts = true;
    }

    void OcultarTodos()
    {
        foreach (CanvasGroup canvas in canvases)
        {
            canvas.alpha = 0;
            canvas.interactable = false;
            canvas.blocksRaycasts = false;
        }
    }
}
