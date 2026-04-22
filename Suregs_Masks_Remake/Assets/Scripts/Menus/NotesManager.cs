using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Item;
using UnityEngine.UI;
using static UnityEditor.Progress;
using TMPro;
using static Enemy;
public class NotesManager : MonoBehaviour
{

    public static NotesManager instance { get; private set; }

    public List<NoteItem> notes = new List<NoteItem>();

    public GameObject notePrefab;

    public Canvas notesCanvas;

    public Transform notesParent;
    public GameObject hover;
    public int currentIndex = 0;
    private int rows = 4;
    private int cols = 3;

    public event System.Action OnNotesChanged;

    public Image closeUpNote;
    public TextMeshProUGUI noteDesc;
    public ScrollRect scrollRect;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // evita duplicados
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // opcional: persiste entre escenas
    }

    private void Start()
    {
        if (notes.Count > 0)
            MoveHoverTo(currentIndex);
        hover.transform.localScale = new Vector3(0.662f, 0.662f, 0.662f);

        
        
    }

    private void Update()
    {
        if (!notesCanvas.gameObject.activeSelf) return;
        if (notes.Count == 0) return;

        int previousIndex = currentIndex;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            currentIndex = (currentIndex + 1) % notes.Count;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            currentIndex = (currentIndex - 1 + notes.Count) % notes.Count;

        if (previousIndex != currentIndex)
            MoveHoverTo(currentIndex);
    }

    private void MoveHoverTo(int index)
    {
        Transform slot = notes[index].transform;
        hover.transform.SetParent(slot, false);
        hover.transform.localPosition = Vector3.zero;

        ScrollToItem(index);
    }

    public NoteItem CreateNoteItem(int id, string name, string description)
    {

        // crear nuevo GameObject en el primer slot vacío
        GameObject newItem = Instantiate(notePrefab);
        NoteItem itemComp = newItem.GetComponent<NoteItem>();
        itemComp.id = id;
        itemComp.name = name;
        itemComp.description = description;

        // buscar slot vacío
        for (int s = 0; s < notes.Count; s++)
        {
            Transform slot = notes[s].transform;
            if (slot.childCount == 0)
            {
                newItem.transform.SetParent(notesParent, false);
                newItem.transform.localPosition = Vector3.zero;
                break;
            }
        }

        notes.Add(itemComp);
        OnNotesChanged?.Invoke();
        return itemComp;
    }

    public void OpenNote(int id)
    {
        closeUpNote.gameObject.SetActive(true);
        noteDesc.text = notes[id].description;

    }

    private void ScrollToItem(int index)
    {
        RectTransform content = scrollRect.content;
        RectTransform viewport = scrollRect.viewport;
        RectTransform item = notes[index].GetComponent<RectTransform>();

        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        // Posición Y del item dentro del content (pivot arriba)
        float itemTop = Mathf.Abs(item.anchoredPosition.y);
        float itemBottom = itemTop + item.rect.height;

        float viewTop = content.anchoredPosition.y;
        float viewBottom = viewTop + viewportHeight;

        // Si el item está por debajo de la vista → bajamos
        if (itemBottom > viewBottom)
        {
            content.anchoredPosition = new Vector2(
                content.anchoredPosition.x,
                itemBottom - viewportHeight
            );
        }
        // Si está por encima → subimos
        else if (itemTop < viewTop)
        {
            content.anchoredPosition = new Vector2(
                content.anchoredPosition.x,
                itemTop
            );
        }
    }

}
