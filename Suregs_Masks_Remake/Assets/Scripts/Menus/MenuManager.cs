using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    public CanvasGroup[] canvases;

    public GameObject[] firstSelectedPerCanvas;

    private int currentIndex = 0;
    private bool activeMenu = false;

    private EnabledSettings enabledSettings;

    void Awake()
    {
        enabledSettings = FindFirstObjectByType<EnabledSettings>();
    }

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

        if (!activeMenu)
        {
            Player.Instance.canMove = true;
            return;
        }
        else
        {
            Player.Instance.canMove = false;
        }


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

        if(index == 3)
        {
            enabledSettings.Enable();
        }

        // Seleccionamos el primer elemento navegable de este canvas para que
        // las flechas/mando funcionen desde el primer momento.
        EventSystem.current.SetSelectedGameObject(null);

        if (firstSelectedPerCanvas != null && index < firstSelectedPerCanvas.Length && firstSelectedPerCanvas[index] != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedPerCanvas[index]);
        }
    }

    void OcultarTodos()
    {
        foreach (CanvasGroup canvas in canvases)
        {
            canvas.alpha = 0;
            canvas.interactable = false;
            canvas.blocksRaycasts = false;
        }

        // Limpiamos la selección para que no se quede "enganchada" a un
        // elemento de un canvas que ya no es visible.
        EventSystem.current.SetSelectedGameObject(null);
    }
}