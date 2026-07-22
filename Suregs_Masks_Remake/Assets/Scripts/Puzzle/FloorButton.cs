using UnityEngine;

public class FloorButton : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite pressedSprite;

    [SerializeField] private string playerTag = "Player";

    [SerializeField] private ButtonSequenceManager manager;

    private SpriteRenderer spriteRenderer;
    private bool isPressed = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (manager == null)
        {
            manager = FindFirstObjectByType<ButtonSequenceManager>();
        }

        SetNormal();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger) return;
        if (isPressed) return; // evita que se dispare varias veces mientras el player está encima
        if (!other.CompareTag(playerTag)) return;
        if (manager == null)
        {
            Debug.LogWarning($"[FloorButton] {name} no tiene asignado un ButtonSequenceManager.");
            return;
        }

        manager.OnButtonPressed(this);
    }

    public void SetPressed()
    {
        isPressed = true;
        if (pressedSprite != null) spriteRenderer.sprite = pressedSprite;
    }

    public void SetNormal()
    {
        isPressed = false;
        if (normalSprite != null) spriteRenderer.sprite = normalSprite;
    }
}
