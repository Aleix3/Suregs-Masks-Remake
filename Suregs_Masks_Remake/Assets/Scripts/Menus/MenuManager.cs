using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject[] canvases;
    private int currentIndex = 0;
    private bool activeMenu = false;

    void Start()
    {
        // Apagar todos al inicio
        foreach (GameObject canvas in canvases)
        {
            canvas.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
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
        canvases[index].SetActive(true);
    }

    void OcultarTodos()
    {
        foreach (GameObject canvas in canvases)
        {
            canvas.SetActive(false);
        }
    }
}
