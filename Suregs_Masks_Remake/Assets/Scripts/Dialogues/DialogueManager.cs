using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{

    private struct RuntimeSentence
    {
        public DialogueSentence sentence;
        public int originalIndex;

        public RuntimeSentence(DialogueSentence sentence, int index)
        {
            this.sentence = sentence;
            this.originalIndex = index;
        }
    }

    public static DialogueManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;

    [Header("Choices")]
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private GameObject optionButtonPrefab;

    private Queue<RuntimeSentence> currentSentences = new Queue<RuntimeSentence>();

    private NPCInteractable currentNPC;
    private DialogueData currentDialogue;

    private bool dialogueActive;
    private int runtimeIndex;

    private List<DialogueOptionButton> optionButtons =
    new List<DialogueOptionButton>();

    private int currentOptionIndex;

    private bool dialogueEnded;
    private Stack<Queue<RuntimeSentence>> flowStack;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    //====================================================
    // START DIALOGUE
    //====================================================

    public void StartDialogue(DialogueData dialogue, NPCInteractable npc)
    {
        if (dialogue == null)
        {
            Debug.LogWarning("DialogueData NULL");
            return;
        }

        dialogueEnded = false;
        UIState.IsUIOpen = true;

        currentNPC = npc;
        currentDialogue = dialogue;
        runtimeIndex = 0;

        dialogueActive = true;

        dialoguePanel.SetActive(true);

        npcNameText.text = dialogue.npcName;
        portraitImage.sprite = dialogue.portrait;

        // STACK SYSTEM
        flowStack = new Stack<Queue<RuntimeSentence>>();

        // construir flujo base correctamente
        Queue<RuntimeSentence> mainFlow = BuildMainFlow(dialogue);

        // si no hay diálogo válido
        if (mainFlow == null || mainFlow.Count == 0)
        {
            EndDialogue();
            return;
        }

        flowStack.Push(mainFlow);
        currentSentences = flowStack.Peek();

        DisplayNextSentence();
        Player.Instance.canMove = false;
    }

    private Queue<RuntimeSentence> BuildMainFlow(DialogueData dialogue)
    {
        Queue<RuntimeSentence> flow = new Queue<RuntimeSentence>();

        List<RuntimeSentence> newSentences = new List<RuntimeSentence>();
        List<RuntimeSentence> fallbackSentences = new List<RuntimeSentence>();

        for (int i = 0; i < dialogue.sentences.Count; i++)
        {
            var sentence = dialogue.sentences[i];

            // Solo frases desbloqueadas por estado
            if (sentence.minState <= GameProgress.CurrentState)
            {
                string key = DialogueMemory.MakeKey(dialogue, i);

                RuntimeSentence runtime =
                    new RuntimeSentence(sentence, i);

                if (!DialogueMemory.HasSeen(key))
                {
                    newSentences.Add(runtime);
                }
                else
                {
                    if (sentence.minState == GameProgress.CurrentState)
                    {
                        fallbackSentences.Add(runtime);
                    }
                }
            }
        }

        // PRIORIDAD: nuevas primero
        if (newSentences.Count > 0)
        {
            foreach (var s in newSentences)
                flow.Enqueue(s);
        }
        else
        {
            foreach (var s in fallbackSentences)
                flow.Enqueue(s);
        }

        return flow;
    }


    public void DisplayNextSentence()
    {


        ClearOptions();

        Debug.Log("currentSentences: " + currentSentences.Count);

        if (currentSentences.Count == 0)
        {
            flowStack.Pop();

            if (flowStack.Count == 0)
            {
                EndDialogue();
                return;
            }

            currentSentences = flowStack.Peek();
            DisplayNextSentence();
            return;
        }

        RuntimeSentence runtimeSentence =
            currentSentences.Dequeue();

        ProcessSentence(runtimeSentence);

        runtimeIndex++;
    }


    private void ProcessSentence(RuntimeSentence runtimeSentence)
    {
        DialogueSentence sentence = runtimeSentence.sentence;

        string key = DialogueMemory.MakeKey(
            currentDialogue,
            runtimeSentence.originalIndex
        );

        DialogueMemory.MarkSeen(key);

        dialogueText.text = sentence.text;

        switch (sentence.type)
        {
            case SentenceType.Normal:
                break;

            case SentenceType.Choice:
                ShowOptions(sentence.options);
                return;

            case SentenceType.Commerce:
                OpenCommerce(sentence.commerceID);
                return;

            case SentenceType.QuestUpdate:
                UpdateQuest(sentence);
                return;
        }
    }


    private void ShowOptions(List<DialogueOption> options)
    {
        optionButtons.Clear();
        currentOptionIndex = 0;

        foreach (DialogueOption option in options)
        {
            GameObject buttonObj =
                Instantiate(optionButtonPrefab, optionsContainer);

            DialogueOptionButton button =
                buttonObj.GetComponent<DialogueOptionButton>();

            button.Setup(option, this);

            optionButtons.Add(button);
        }

        currentOptionIndex = 0;
        UpdateOptionHover();

    }

    public void SelectOption(DialogueOption option)
    {
        ClearOptions();

        Queue<RuntimeSentence> optionFlow = new Queue<RuntimeSentence>();


        foreach (DialogueSentence sentence in option.nextSentences)
        {
            if (IsSentenceAvailable(sentence))
            {
                optionFlow.Enqueue(new RuntimeSentence(sentence, -1));
            }
        }

        flowStack.Push(optionFlow);
        currentSentences = optionFlow;

        DisplayNextSentence();
    }

    private void ClearOptions()
    {
        foreach (Transform child in optionsContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void UpdateQuest(DialogueSentence sentence)
    {
        Debug.Log("Quest Updated");
    }

    private void OpenCommerce(int commerceID)
    {
        Debug.Log("Open Shop: " + commerceID);
    }


    public void EndDialogue()
    {
        StartCoroutine(EndDialogueRoutine());
    }

    private IEnumerator EndDialogueRoutine()
    {
        Debug.Log("END");

        dialogueActive = false;

        dialoguePanel.SetActive(false);

        ClearOptions();

        currentNPC = null;
        currentDialogue = null;
        Player.Instance.canMove = true;
        yield return new WaitForSeconds(0.3f);

        UIState.IsUIOpen = false;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            GameProgress.Advance();
        }

        if (!dialogueActive)
            return;

        if (optionsContainer.childCount > 0)
        {
            HandleOptionInput();
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (optionsContainer.childCount > 0)
                return;

            DisplayNextSentence();
        }
    }

    private void HandleOptionInput()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            currentOptionIndex++;
            if (currentOptionIndex >= optionButtons.Count)
                currentOptionIndex = 0;

            UpdateOptionHover();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            currentOptionIndex--;
            if (currentOptionIndex < 0)
                currentOptionIndex = optionButtons.Count - 1;

            UpdateOptionHover();
        }

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
        {
            optionButtons[currentOptionIndex].Select();
        }
    }

    private void UpdateOptionHover()
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            optionButtons[i].SetHover(i == currentOptionIndex);
        }
    }

    private bool IsSentenceAvailable(DialogueSentence sentence)
    {
        return sentence.minState <= GameProgress.CurrentState;
    }
}