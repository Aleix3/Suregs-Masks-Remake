using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    Queue<string> lines;
    public static DialogueManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void StartDialogue(List<string> dialogueLines)
    {
        lines = new Queue<string>(dialogueLines);
        ShowNextLine();
    }

    public void ShowNextLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        string line = lines.Dequeue();
        // Mostrar en UI
    }

    void EndDialogue()
    {
        // Cerrar caja de diálogo
    }
}

