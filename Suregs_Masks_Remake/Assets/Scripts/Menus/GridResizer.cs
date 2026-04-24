using UnityEngine;
using UnityEngine.UI;

public class GridResizer : MonoBehaviour
{
    public GridLayoutGroup grid;
    public RectTransform content;

    public void UpdateSize()
    {
        int itemCount = content.childCount;

        if (itemCount == 0)
        {
            content.sizeDelta = new Vector2(content.sizeDelta.x, 0);
            return;
        }

        int columns = grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount
            ? grid.constraintCount
            : 1;

        int rows = Mathf.CeilToInt((float)itemCount / columns);

        float height =
            rows * grid.cellSize.y +
            (rows - 1) * grid.spacing.y +
            grid.padding.top +
            grid.padding.bottom - 1417.22f;

        Debug.Log($"Rows: {rows}, Height: {height}");

        Canvas.ForceUpdateCanvases();

        // 🔥 IMPORTANTE: primero deja que Unity calcule el layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        // 🔥 luego aplicas el tamaño
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

        // opcional pero recomendable para evitar offsets raros
        scrollReset();
    }

    private void scrollReset()
    {
        ScrollRect sr = content.GetComponentInParent<ScrollRect>();
        if (sr != null)
            sr.verticalNormalizedPosition = 1f;
    }
}