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

    public GameObject notesSlots;
    public GameObject hover;
    public int currentIndex = 0;
    private int rows = 4;
    private int cols = 3;

    public event System.Action OnNotesChanged;

    public Image closeUpNote;
    public TextMeshProUGUI noteDesc;

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
        if (notesSlots.transform.childCount > 0)
            MoveHoverTo(currentIndex);
        hover.transform.localScale = new Vector3(0.662f, 0.662f, 0.662f);

        
        
    }

    private void Update()
    {
        if (notesCanvas.gameObject.activeSelf == false) return;
        if (notesSlots.transform.childCount == 0) return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentIndex++;
            if (currentIndex % cols == 0) // pasa del borde derecho
                currentIndex -= cols;     // vuelve al principio de la fila
            MoveHoverTo(currentIndex);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentIndex--;
            if (currentIndex < 0 || currentIndex % cols == cols - 1) // pasa del borde izq
                currentIndex += cols;  // salta al final de la fila
            MoveHoverTo(currentIndex);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentIndex += cols;
            if (currentIndex >= rows * cols) // pasa del borde inferior
                currentIndex %= cols;       // vuelve a la fila superior misma columna
            MoveHoverTo(currentIndex);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentIndex -= cols;
            if (currentIndex < 0)           // pasa del borde superior
                currentIndex += rows * cols; // baja a la última fila misma columna
            MoveHoverTo(currentIndex);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentIndex -= cols;
            if (currentIndex < 0)           // pasa del borde superior
                currentIndex += rows * cols; // baja a la última fila misma columna
            MoveHoverTo(currentIndex);
        }

        
    }

    private void MoveHoverTo(int index)
    {
        Transform slot = notesSlots.transform.GetChild(index);
        hover.transform.SetParent(slot, false);
        hover.transform.localPosition = Vector3.zero;

        // Buscar si hay un hermano del hover item en este slot
        NoteItem item = slot.GetComponentInChildren<NoteItem>();

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
        for (int s = 0; s < notesSlots.transform.childCount; s++)
        {
            Transform slot = notesSlots.transform.GetChild(s);
            if (slot.childCount == 0)
            {
                newItem.transform.SetParent(slot, false);
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

}
