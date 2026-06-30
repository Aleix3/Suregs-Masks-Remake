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

    [Header("Feedback de acción bloqueada")]

    public Color deniedFlashColor = new Color(1f, 0.25f, 0.25f, 1f);
    public float deniedFlashDuration = 0.35f;
    public float shakeStrength = 8f;    
    public int shakeVibrations = 6;    

    private MaskManager maskManager;

    // Posiciones originales de los iconos, para el shake
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
    }

    void OnDestroy()
    {
        UnbindMaskManager();
    }

    void Update()
    {
        if (maskManager == null)
        {
            TryBindMaskManager();
            return;
        }

        UpdateCooldownOverlay(maskManager.Primary, cooldownOverlayPrimary);


        if (Player.Instance != null && HealthBar != null)
            HealthBar.fillAmount = Player.Instance.GetHealth() / Player.Instance.GetMaxHealth();

        if(PlayerEconomy.instance != null)
        { actualMoney.text = PlayerEconomy.instance.gold.ToString(); }
    }


    private void TryBindMaskManager()
    {
        if (Player.Instance == null) return;
        maskManager = Player.Instance.MaskManager;
        if (maskManager == null) return;

        maskManager.OnSwap += UpdateMaskIcons;
        maskManager.OnSwapBlocked += HandleSwapBlocked;
        maskManager.OnActivateBlocked += HandleActivateBlocked;

        UpdateMaskIcons(maskManager.Primary, maskManager.Secondary);
    }

    private void UnbindMaskManager()
    {
        if (maskManager == null) return;
        maskManager.OnSwap -= UpdateMaskIcons;
        maskManager.OnSwapBlocked -= HandleSwapBlocked;
        maskManager.OnActivateBlocked -= HandleActivateBlocked;
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
