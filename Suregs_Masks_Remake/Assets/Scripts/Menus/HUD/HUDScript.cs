using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDScript : MonoBehaviour
{
    public GameObject maskSlotPrimary;
    public GameObject maskSlotSecondary;

    public Image iconPrimary;
    public Image iconSecondary;

    public Image cooldownOverlayPrimary;

    public Image HealthBar;

    public TextMeshProUGUI actualMoney;

    [Header("Pociones")]
    public Image potionIcon;
    public TextMeshProUGUI potionQuantity;

    [Header("Feedback de acción bloqueada")]

    public Color deniedFlashColor = new Color(1f, 0.25f, 0.25f, 1f);
    public float deniedFlashDuration = 0.35f;
    public float shakeStrength = 8f;
    public int shakeVibrations = 6;

    [Header("Notificaciones de Mask Points")]
    public GameObject maskPointNotificationPrefab;

    public RectTransform notificationContainer;

    [Header("Misiones")]
    [Tooltip("Texto donde se muestra la descripción de la misión principal actual.")]
    public TextMeshProUGUI mainQuestText;

    [Tooltip("Prefab de notificación para cuando arranca una misión secundaria. Puede ser el mismo que maskPointNotificationPrefab si comparten el mismo componente MaskPointNotification.")]
    public GameObject sideQuestNotificationPrefab;

    private MaskManager maskManager;
    private PotionManager potionManager;
    private QuestManager questManager;

    // Posiciones originales de los iconos, para el shake al invertir mascaras
    private Vector3 _primaryOriginalPos;
    private Vector3 _secondaryOriginalPos;
    private Color _primaryOriginalColor;
    private Color _secondaryOriginalColor;

    private Coroutine _primaryDeniedRoutine;
    private Coroutine _secondaryDeniedRoutine;

    void Start()
    {
        if (iconPrimary != null) { _primaryOriginalPos = iconPrimary.rectTransform.localPosition; _primaryOriginalColor = iconPrimary.color; }
        if (iconSecondary != null) { _secondaryOriginalPos = iconSecondary.rectTransform.localPosition; _secondaryOriginalColor = iconSecondary.color; }

        TryBindMaskManager();
        TryBindPotionManager();
        TryBindQuestManager();
    }

    void OnDestroy()
    {
        UnbindMaskManager();
        UnbindPotionManager();
        UnbindQuestManager();
    }

    void Update()
    {
        if (potionManager == null)
        {
            TryBindPotionManager();
        }

        if (questManager == null)
        {
            TryBindQuestManager();
        }

        if (maskManager == null)
        {
            TryBindMaskManager();
            return;
        }

        UpdateCooldownOverlay(maskManager.Primary, cooldownOverlayPrimary);


        if (Player.Instance != null && HealthBar != null)
            HealthBar.fillAmount = Player.Instance.GetHealth() / Player.Instance.GetMaxHealth();

        if (PlayerEconomy.instance != null)
        {
            actualMoney.text = PlayerEconomy.instance.gold.ToString();
        }
    }


    private void TryBindMaskManager()
    {
        if (Player.Instance == null) return;
        maskManager = Player.Instance.MaskManager;
        if (maskManager == null) return;

        maskManager.OnSwap += UpdateMaskIcons;
        maskManager.OnSwapBlocked += HandleSwapBlocked;
        maskManager.OnActivateBlocked += HandleActivateBlocked;

        // Suscribir notificaciones de puntos
        if (MaskTreeManager.Instance != null)
            MaskTreeManager.Instance.OnPointsAdded += HandlePointsAdded;

        UpdateMaskIcons(maskManager.Primary, maskManager.Secondary);
    }

    private void UnbindMaskManager()
    {
        if (maskManager == null) return;
        maskManager.OnSwap -= UpdateMaskIcons;
        maskManager.OnSwapBlocked -= HandleSwapBlocked;
        maskManager.OnActivateBlocked -= HandleActivateBlocked;

        if (MaskTreeManager.Instance != null)
            MaskTreeManager.Instance.OnPointsAdded -= HandlePointsAdded;
    }

    private void TryBindPotionManager()
    {
        if (PotionManager.instance == null) return;

        potionManager = PotionManager.instance;
        potionManager.OnPotionChanged += UpdatePotionIcon;
        potionManager.OnNoPotions += ClearPotionIcon;

        // Pintar el estado actual ya mismo (el evento solo dispara con cambios futuros)
        potionManager.PublishCurrentState();
    }

    private void UnbindPotionManager()
    {
        if (potionManager == null) return;

        potionManager.OnPotionChanged -= UpdatePotionIcon;
        potionManager.OnNoPotions -= ClearPotionIcon;
    }

    private void TryBindQuestManager()
    {
        if (QuestManager.Instance == null) return;

        questManager = QuestManager.Instance;
        questManager.OnMainQuestChanged += UpdateMainQuestText;
        questManager.OnSideQuestStarted += HandleSideQuestStarted;
        questManager.OnSideQuestCompleted += HandleSideQuestCompleted;

        // Pintar el estado actual ya mismo (el evento solo dispara con cambios futuros)
        UpdateMainQuestText(questManager.CurrentMainQuest);
    }

    private void UnbindQuestManager()
    {
        if (questManager == null) return;

        questManager.OnMainQuestChanged -= UpdateMainQuestText;
        questManager.OnSideQuestStarted -= HandleSideQuestStarted;
        questManager.OnSideQuestCompleted -= HandleSideQuestCompleted;
    }

    private void UpdateMainQuestText(QuestStep step)
    {
        if (mainQuestText == null) return;
        mainQuestText.text = step != null ? step.description : "";
    }

    private void HandleSideQuestStarted(QuestStep step)
    {
        if (sideQuestNotificationPrefab == null || notificationContainer == null) return;

        var go = Instantiate(sideQuestNotificationPrefab, notificationContainer);
        go.transform.SetAsLastSibling();

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            notificationContainer.GetComponent<RectTransform>());

        var notif = go.GetComponent<MaskPointNotification>();
        notif?.Show(null, $"Nueva misión: {step.description}");
    }

    private void HandleSideQuestCompleted(QuestStep step)
    {
        // Opcional: aquí puedes disparar una notificación de "misión completada"
        // reutilizando el mismo sistema, si lo necesitas más adelante.
    }

    private void UpdatePotionIcon(Item.ItemType type, Sprite sprite, uint quantity)
    {
        if (potionIcon != null)
        {
            potionIcon.sprite = sprite;
            potionIcon.enabled = sprite != null;
            potionIcon.preserveAspect = true;
        }

        if (potionQuantity != null)
            potionQuantity.text = "X" + quantity;
    }

    private void ClearPotionIcon()
    {
        if (potionIcon != null)
            potionIcon.enabled = false;

        if (potionQuantity != null)
            potionQuantity.text = "";
    }

    private void HandlePointsAdded(int maskIndex, int amount)
    {
        if (maskPointNotificationPrefab == null || notificationContainer == null) return;
        if (MaskTreeManager.Instance == null) return;

        BaseMask mask = MaskTreeManager.Instance.masks[maskIndex];
        Sprite icon = mask?.data?.maskIcon;
        string text = $"+{amount} Mask Point{(amount > 1 ? "s" : "")}";

        var go = Instantiate(maskPointNotificationPrefab, notificationContainer);
        go.transform.SetAsLastSibling();

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            notificationContainer.GetComponent<RectTransform>());
        var notif = go.GetComponent<MaskPointNotification>();
        notif?.Show(icon, text);
    }


    private void UpdateMaskIcons(BaseMask primary, BaseMask secondary)
    {
        SetIcon(iconPrimary, primary);
        SetIcon(iconSecondary, secondary);
    }

    private void SetIcon(Image iconImage, BaseMask mask)
    {
        if (iconImage == null) return;

        if (mask != null && mask.data != null && mask.data.maskIcon != null)
        {
            iconImage.sprite = mask.data.maskIcon;
            iconImage.preserveAspect = true;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }
    }


    private void UpdateCooldownOverlay(BaseMask mask, Image overlay)
    {
        if (overlay == null) return;

        if (mask == null || mask.LastCooldownDuration <= 0f || mask.CurrentCooldown <= 0f)
        {
            overlay.fillAmount = 0f;
            overlay.enabled = false;
            return;
        }

        overlay.enabled = true;
        overlay.fillAmount = Mathf.Clamp01(mask.CurrentCooldown / mask.LastCooldownDuration);
    }




    private void HandleSwapBlocked(BaseMask primary, BaseMask secondary)
    {
        PlayDenied(iconPrimary, _primaryOriginalPos, _primaryOriginalColor, ref _primaryDeniedRoutine);
        PlayDenied(iconSecondary, _secondaryOriginalPos, _secondaryOriginalColor, ref _secondaryDeniedRoutine);
    }


    private void HandleActivateBlocked(BaseMask mask)
    {
        PlayDenied(iconPrimary, _primaryOriginalPos, _primaryOriginalColor, ref _primaryDeniedRoutine);
    }

    private void PlayDenied(Image icon, Vector3 originalPos, Color originalColor, ref Coroutine routineRef)
    {
        if (icon == null || !icon.enabled) return;
        if (routineRef != null) StopCoroutine(routineRef);
        routineRef = StartCoroutine(DeniedRoutine(icon, originalPos, originalColor));
    }

    private IEnumerator DeniedRoutine(Image icon, Vector3 originalPos, Color originalColor)
    {
        RectTransform rt = icon.rectTransform;
        float elapsed = 0f;

        while (elapsed < deniedFlashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / deniedFlashDuration;

            // Shake: desplazamiento que decae con el tiempo, oscilando varias veces
            float decay = 1f - t;
            float offset = Mathf.Sin(t * shakeVibrations * Mathf.PI * 2f) * shakeStrength * decay;
            rt.localPosition = originalPos + new Vector3(offset, 0f, 0f);

            // Flash: rojo al inicio, vuelve al color original al terminar
            icon.color = Color.Lerp(deniedFlashColor, originalColor, t);

            yield return null;
        }

        rt.localPosition = originalPos;
        icon.color = originalColor;
    }
}