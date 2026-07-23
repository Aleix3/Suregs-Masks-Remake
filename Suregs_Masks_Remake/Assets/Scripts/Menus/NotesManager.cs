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

    bool firstTime = false;

    [System.Serializable]
    public class NoteSaveData
    {
        public int id;
    }

    [System.Serializable]
    public class NotesSave
    {
        public List<NoteSaveData> notes = new();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // evita duplicados
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // opcional: persiste entre escenas

        LoadNotes();
    }

    private void Start()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;

        if (notes.Count > 0)
            MoveHoverTo(currentIndex);

        hover.transform.localScale = new Vector3(0.662f, 0.662f, 0.662f);
    }


    private void Update()
    {
        if (!notesCanvas.gameObject.activeSelf) return;
        if (notes.Count == 0) return;

        if (MenuManager.Instance.currentIndex != 2)
        {
            return;
        }

        if (!firstTime)
        {
            if (notes.Count > 0)
                MoveHoverTo(currentIndex);
            hover.GetComponent<RectTransform>().anchoredPosition = new Vector2(750, -750);
            firstTime = true;
        }

        int previousIndex = currentIndex;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            currentIndex = (currentIndex + 1) % notes.Count;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            currentIndex = (currentIndex - 1 + notes.Count) % notes.Count;

        if (previousIndex != currentIndex)
            MoveHoverTo(currentIndex);

        if (Input.GetKeyDown(KeyCode.J))
        {
            if(closeUpNote.gameObject.activeSelf)
            {
                CloseNote();
            }
            else
            {
                OpenNote(currentIndex);
            }
        }

        if(Input.GetKeyDown(KeyCode.Escape) && closeUpNote.gameObject.activeSelf)
        {
            CloseNote();
        }
            
    }

    private void MoveHoverTo(int index)
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.selectItemClip);
        Transform slot = notes[index].transform;
        hover.transform.SetParent(slot, false);
        hover.transform.localPosition = Vector3.zero;

        ScrollToItem(index);
    }

    public NoteItem CreateNoteItem(int id, string name, string description)
    {
        NoteItem existing = notes.Find(n => n.id == id);
        if (existing != null)
            return existing;

        GameObject newItem = Instantiate(notePrefab, notesParent);

        NoteItem itemComp = newItem.GetComponent<NoteItem>();
        itemComp.id = id;
        itemComp.name = name;
        itemComp.description = description;

        notes.Add(itemComp);
        SaveNotes();
        notesParent.GetComponent<GridResizer>().UpdateSize();

        OnNotesChanged?.Invoke();
        return itemComp;
    }

    public void OpenNote(int id)
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClip);
        closeUpNote.gameObject.SetActive(true);
        noteDesc.text = notes[id].description;

    }

    public void CloseNote()
    {
        closeUpNote.gameObject.SetActive(false);

    }

    private void ScrollToItem(int index)
    {
        if (notes.Count <= 1) return;

        Canvas.ForceUpdateCanvases();

        float normalized = 1f - (float)index / (notes.Count - 1);

        scrollRect.verticalNormalizedPosition = normalized;
    }

    private const string NOTES_KEY = "Notes";

    public void SaveNotes()
    {
        NotesSave save = new NotesSave();

        foreach (NoteItem note in notes)
        {
            save.notes.Add(new NoteSaveData
            {
                id = note.id
            });
        }

        string json = JsonUtility.ToJson(save);
        PlayerPrefs.SetString(NOTES_KEY, json);
        PlayerPrefs.Save();
    }

    public void LoadNotes()
    {
        if (!PlayerPrefs.HasKey(NOTES_KEY))
            return;

        ClearNotes();

        string json = PlayerPrefs.GetString(NOTES_KEY);
        NotesSave save = JsonUtility.FromJson<NotesSave>(json);

        if (save == null)
            return;

        foreach (var note in save.notes)
        {
            (string name, string description) = Note.GetItemData(note.id);

            CreateNoteItem(note.id, name, description);
        }

        if (notes.Count > 0)
            MoveHoverTo(0);
    }

    public void ClearNotes()
    {
        foreach (NoteItem note in notes)
        {
            if (note != null)
                Destroy(note.gameObject);
        }

        notes.Clear();

        currentIndex = 0;
        closeUpNote.gameObject.SetActive(false);
    }
}
