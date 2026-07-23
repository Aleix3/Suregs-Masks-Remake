using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    // Otros scripts de menu pueden consultar MenuManager.Instance.IsMenuOpen
    // para saber si el menu principal esta abierto y, si lo esta, no dejar
    // que se abra ni procese input ningun otro menu.
    public static MenuManager Instance { get; private set; }

    public bool IsMenuOpen => activeMenu;

    public CanvasGroup[] canvases;

    public GameObject[] firstSelectedPerCanvas;

    public int currentIndex = 0;
    private bool activeMenu = false;

    private EnabledSettings enabledSettings;

    void Awake()
    {
        Instance = this;
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
                AudioManager.Instance.PlaySFX(AudioManager.Instance.openInventoryClip);
                Player.Instance.LockMovement(this);
                currentIndex = 0;
                MostrarCanvas(currentIndex);
            }
            else
            {
                Player.Instance.UnlockMovement(this);
                OcultarTodos();
            }
        }

        if (!activeMenu)
        {

            return;
        }



        if (Input.GetKeyDown(KeyCode.E))
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.changeInventoryPageClip);
            currentIndex++;

            if (currentIndex >= canvases.Length)
                currentIndex = 0;

            MostrarCanvas(currentIndex);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.changeInventoryPageClip);
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

        if (index == 3)
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

        // Limpiamos la selecci�n para que no se quede "enganchada" a un
        // elemento de un canvas que ya no es visible.
        EventSystem.current.SetSelectedGameObject(null);
    }
}