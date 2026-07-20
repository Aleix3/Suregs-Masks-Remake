using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class MenuNavigationWrap : MonoBehaviour
{
    public enum Direction { Vertical, Horizontal }

    [SerializeField] private Direction direction = Direction.Vertical;
    [Tooltip("Botones en el orden en el que se debe navegar entre ellos.")]
    [SerializeField] private List<Selectable> items = new List<Selectable>();

    private void Start()
    {
        SetupWrapNavigation();
    }

    private void SetupWrapNavigation()
    {
        int count = items.Count;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            Selectable current = items[i];
            if (current == null) continue;

            Selectable prev = items[(i - 1 + count) % count];
            Selectable next = items[(i + 1) % count];

            Navigation nav = current.navigation;
            nav.mode = Navigation.Mode.Explicit;

            if (direction == Direction.Vertical)
            {
                nav.selectOnUp = prev;
                nav.selectOnDown = next;
            } 
            else
            {
                nav.selectOnLeft = prev;
                nav.selectOnRight = next;
            }

            current.navigation = nav;
        }
    }
}
