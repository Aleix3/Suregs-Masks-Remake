using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Persistencia")]
    [Tooltip("Si está activo, guarda/carga el progreso con PlayerPrefs automáticamente.")]
    public bool usePlayerPrefsPersistence = true;

    private const string SavePrefKeyMainIndex = "Quests_MainIndex";
    private const string SavePrefKeySideQuests = "Quests_ActiveSideIds";

    private List<QuestStep> _mainQuestLine;
    private int _currentMainIndex;

    private readonly List<QuestStep> _activeSideQuests = new List<QuestStep>();

    public event Action<QuestStep> OnMainQuestChanged;

    public event Action<QuestStep> OnSideQuestStarted;

    public event Action<QuestStep> OnSideQuestCompleted;

    public QuestStep CurrentMainQuest =>
        _currentMainIndex >= 0 && _currentMainIndex < _mainQuestLine.Count
            ? _mainQuestLine[_currentMainIndex]
            : null;

    public IReadOnlyList<QuestStep> ActiveSideQuests => _activeSideQuests;

    public bool IsMainQuestLineComplete => _currentMainIndex >= _mainQuestLine.Count;

    // Texto exacto de los pasos "vuelve a la mazmorra". Centralizado aquí para
    // no repetir el string literal en cada script que necesite comprobarlo
    // (ElevatorButton, etc). Si el texto cambia, solo hay que tocarlo aquí.
    private const string DungeonReturnQuestText = "Vuelve a las mazmorras y sigue descubriendo la historia de tu padre.";

    /// <summary>
    /// True si la misión principal activa es del tipo "vuelve a la mazmorra".
    /// Úsalo para bloquear el acceso al ascensor/mazmorra mientras el jugador
    /// tenga pendiente otra cosa (hablar con Vhea, visitar tiendas, etc).
    /// </summary>
    public bool CurrentQuestAllowsDungeonEntry()
    {
        if(CurrentMainQuest == null)
        {
            return true;
        }
        else
        {
            return CurrentMainQuest != null && CurrentMainQuest.description == DungeonReturnQuestText;
        }
        
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        _mainQuestLine = QuestDatabase.MainQuestLine;

        if (usePlayerPrefsPersistence)
            LoadProgress();
    }

    private void Start()
    {
        OnMainQuestChanged?.Invoke(CurrentMainQuest);
        foreach (var side in _activeSideQuests)
            OnSideQuestStarted?.Invoke(side);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            CompleteCurrentMainStep();
        }
    }

    public void CompleteCurrentMainStep()
    {
        if (IsMainQuestLineComplete) return;

        _currentMainIndex++;
        OnMainQuestChanged?.Invoke(CurrentMainQuest);

        if (usePlayerPrefsPersistence) SaveProgress();
    }

    public void CompleteMainStepById(string id)
    {
        if (CurrentMainQuest == null) return;

        if (CurrentMainQuest.id != id)
        {
            Debug.LogWarning($"[QuestManager] Se intentó completar el paso '{id}' pero el paso activo es '{CurrentMainQuest.id}'. Se ignora.");
            return;
        }

        CompleteCurrentMainStep();
    }

    public void JumpToMainStep(string id)
    {
        int index = _mainQuestLine.FindIndex(step => step.id == id);
        if (index < 0)
        {
            Debug.LogWarning($"[QuestManager] No existe ningún paso principal con id '{id}'.");
            return;
        }

        _currentMainIndex = index;
        OnMainQuestChanged?.Invoke(CurrentMainQuest);

        if (usePlayerPrefsPersistence) SaveProgress();
    }

    public void StartSideQuest(string id)
    {
        if (_activeSideQuests.Any(q => q.id == id)) return;

        QuestStep step = QuestDatabase.FindSideQuest(id);
        if (step == null)
        {
            Debug.LogWarning($"[QuestManager] No existe ninguna misión secundaria con id '{id}'.");
            return;
        }

        _activeSideQuests.Add(step);
        OnSideQuestStarted?.Invoke(step);

        if (usePlayerPrefsPersistence) SaveProgress();
    }


    public void CompleteSideQuest(string id)
    {
        QuestStep step = _activeSideQuests.FirstOrDefault(q => q.id == id);
        if (step == null) return;

        _activeSideQuests.Remove(step);
        OnSideQuestCompleted?.Invoke(step);

        if (usePlayerPrefsPersistence) SaveProgress();
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt(SavePrefKeyMainIndex, _currentMainIndex);
        PlayerPrefs.SetString(SavePrefKeySideQuests, string.Join(",", _activeSideQuests.Select(q => q.id)));
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        _currentMainIndex = PlayerPrefs.GetInt(SavePrefKeyMainIndex, 0);

        string savedSideIds = PlayerPrefs.GetString(SavePrefKeySideQuests, "");
        _activeSideQuests.Clear();
        if (!string.IsNullOrEmpty(savedSideIds))
        {
            foreach (string id in savedSideIds.Split(','))
            {
                QuestStep step = QuestDatabase.FindSideQuest(id);
                if (step != null) _activeSideQuests.Add(step);
            }
        }
    }

    public void ResetProgress()
    {
        _currentMainIndex = 0;
        _activeSideQuests.Clear();
        PlayerPrefs.DeleteKey(SavePrefKeyMainIndex);
        PlayerPrefs.DeleteKey(SavePrefKeySideQuests);

        OnMainQuestChanged?.Invoke(CurrentMainQuest);
    }
}