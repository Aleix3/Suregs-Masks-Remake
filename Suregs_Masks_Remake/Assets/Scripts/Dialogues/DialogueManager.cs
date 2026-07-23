using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    public static DialogueManager Instance { get; private set; }

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

    private bool blockAdvanceInput;

    [Header("Commerce")]
    public GameObject[] shops;
    private bool commerceOpen;

    [Header("Mensajes simples (avisos, bloqueos, etc)")]
    [HideInInspector] public bool simpleMessageActive;
    private bool simpleMessageBlockInput;

    public BoxCollider2D triggerTuorialCombat;
    private SentenceType? lastSentenceType = null;


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

        if (mainFlow == null || mainFlow.Count == 0)
        {
            EndDialogue();
            return;
        }

        flowStack.Push(mainFlow);
        currentSentences = flowStack.Peek();
        blockAdvanceInput = true;
        DisplayNextSentence();
        Player.Instance.LockMovement(this);
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
        else if (fallbackSentences.Count > 0)
        {
            foreach (var s in fallbackSentences)
                flow.Enqueue(s);
        }
        else if (dialogue.isCommerceDialogue && dialogue.sentences.Count > 0)
        {
            // Los NPCs de comercio siempre deben poder hablarse, así que repetimos la primera frase.
            flow.Enqueue(new RuntimeSentence(dialogue.sentences[0], 0));
        }

        return flow;
    }


    public void DisplayNextSentence()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClip);
        if (lastSentenceType == SentenceType.CombatTutorial)
        {
            triggerTuorialCombat.enabled = true;
            lastSentenceType = null;
            GameProgress.Advance();
        }

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

        if (sentence.overrideSpeaker)
        {
            npcNameText.text = sentence.speakerName;
            portraitImage.sprite = sentence.speakerPortrait;
        }
        else
        {
            npcNameText.text = currentDialogue.npcName;
            portraitImage.sprite = currentDialogue.portrait;
        }

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

            case SentenceType.CombatTutorial:
                break;
            case SentenceType.Destroy:
                Destroy(currentNPC.gameObject);
                break;
            case SentenceType.Ending:
            {
                if (sentence.id == "1")
                {
                        AudioManager.Instance.PlayMusic(AudioManager.Instance.badEndingMusic);
                        StartCoroutine(ChangeScene("BadEnding"));
                }
                else
                {
                        AudioManager.Instance.PlayMusic(AudioManager.Instance.goodEndingMusic);
                        StartCoroutine(ChangeScene("GoodEnding"));

                }
                break;
            }
                
        }
        lastSentenceType = sentence.type;
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
        commerceOpen = true;
        shops[commerceID].SetActive(true);
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
        Player.Instance.UnlockMovement(this);
        yield return new WaitForSeconds(0.3f);

        UIState.IsUIOpen = false;
    }


    private void Update()
    {
        if (MenuManager.Instance != null && MenuManager.Instance.IsMenuOpen)
            return;
        if (Input.GetKeyDown(KeyCode.H))
        {
            GameProgress.Advance();
        }

        if (simpleMessageActive)
        {
            if (simpleMessageBlockInput)
            {
                if (Input.GetKeyUp(KeyCode.E))
                    simpleMessageBlockInput = false;

                return;
            }

            if (Input.GetKeyDown(KeyCode.E))
                CloseSimpleMessage();

            return;
        }

        if (!dialogueActive)
            return;

        if (commerceOpen)
            return;

        if (blockAdvanceInput)
        {
            if (Input.GetKeyUp(KeyCode.E))
            {
                blockAdvanceInput = false;
            }

            return;
        }

        if (optionsContainer.childCount > 0)
        {
            HandleOptionInput();
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
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
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClip);
            optionButtons[currentOptionIndex].Select();
        }
    }

    private void UpdateOptionHover()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.selectClip);
        for (int i = 0; i < optionButtons.Count; i++)
        {
            optionButtons[i].SetHover(i == currentOptionIndex);
        }
    }

    private bool IsSentenceAvailable(DialogueSentence sentence)
    {
        return sentence.minState <= GameProgress.CurrentState;
    }

    public void CloseCommerce()
    {
        commerceOpen = false;

        DisplayNextSentence();
    }

    public void ShowSimpleMessage(string speaker, string text, Sprite portrait = null, bool comesFromInput = false)
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClip);
        if (dialogueActive || simpleMessageActive) return;

        simpleMessageActive = true;
        if(comesFromInput)
        {
            simpleMessageBlockInput = true;
        }
        
        UIState.IsUIOpen = true;

        ClearOptions();
        dialoguePanel.SetActive(true);
        npcNameText.text = speaker;
        portraitImage.sprite = portrait;
        dialogueText.text = text;

        if (Player.Instance != null)
            Player.Instance.LockMovement(this);
    }

    private void CloseSimpleMessage()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClip);
        simpleMessageActive = false;
        simpleMessageBlockInput = false;
        dialoguePanel.SetActive(false);
        UIState.IsUIOpen = false;

        if (Player.Instance != null)
            Player.Instance.UnlockMovement(this);
    }

    public void FindShops()
    {
        shops = new GameObject[3];

        shops[0] = FindSceneObject("BlackSmithCanvas");
        shops[1] = FindSceneObject("MerchantCanvas");
        shops[2] = FindSceneObject("WitchCanvas");
    }

    private GameObject FindSceneObject(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            GameObject result = FindInChildren(root.transform, objectName);
            if (result != null)
                return result;
        }

        return null;
    }

    private GameObject FindInChildren(Transform parent, string objectName)
    {
        if (parent.name == objectName)
            return parent.gameObject;

        foreach (Transform child in parent)
        {
            GameObject result = FindInChildren(child, objectName);
            if (result != null)
                return result;
        }

        return null;
    }

    IEnumerator ChangeScene(string scene)
    {
        dialoguePanel.SetActive(false);
        yield return StartCoroutine(CameraManager.Instance.Fade(1, 2));

        SceneManager.LoadScene(scene);
    }
}