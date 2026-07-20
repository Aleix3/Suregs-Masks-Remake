using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;


public class SettingsManager : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicMixerParam = "MusicVolume";
    [SerializeField] private string sfxMixerParam = "SFXVolume";

    [Header("Referencias UI")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vsyncToggle;

    [Header("Botón 'Volver al Menú")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    private const string MUSIC_KEY = "musicVolume";
    private const string SFX_KEY = "sfxVolume";
    private const string FULLSCREEN_KEY = "fullscreen";
    private const string VSYNC_KEY = "vsync";

    private void Start()
    {
        LoadSettings();

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSfxVolume);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        vsyncToggle.onValueChanged.AddListener(SetVSync);
    }

    private void LoadSettings()
    {
        float music = PlayerPrefs.GetFloat(MUSIC_KEY, 0.75f);
        float sfx = PlayerPrefs.GetFloat(SFX_KEY, 0.75f);
        bool fullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, Screen.fullScreen ? 1 : 0) == 1;
        bool vsync = PlayerPrefs.GetInt(VSYNC_KEY, 1) == 1;

        // SetValueWithoutNotify evita disparar el evento onValueChanged al cargar
        musicSlider.SetValueWithoutNotify(music);
        sfxSlider.SetValueWithoutNotify(sfx);
        fullscreenToggle.SetIsOnWithoutNotify(fullscreen);
        vsyncToggle.SetIsOnWithoutNotify(vsync);

        ApplyMusicVolume(music);
        ApplySfxVolume(sfx);
        ApplyFullscreen(fullscreen);
        ApplyVSync(vsync);
    }

    public void SetMusicVolume(float value)
    {
        ApplyMusicVolume(value);
        PlayerPrefs.SetFloat(MUSIC_KEY, value);
    }

    public void SetSfxVolume(float value)
    {
        ApplySfxVolume(value);
        PlayerPrefs.SetFloat(SFX_KEY, value);
    }

    public void SetFullscreen(bool value)
    {
        ApplyFullscreen(value);
        PlayerPrefs.SetInt(FULLSCREEN_KEY, value ? 1 : 0);
    }

    public void SetVSync(bool value)
    {
        ApplyVSync(value);
        PlayerPrefs.SetInt(VSYNC_KEY, value ? 1 : 0);
    }

    // ---------- VOLVER AL MENÚ (solo botón del panel InGame) ----------
    public void OnVolverAlMenu()
    {
        StartCoroutine(FadeAndLoadMenu());
    }

    private IEnumerator FadeAndLoadMenu()
    {
        Time.timeScale = 1f; // por si el juego estaba pausado al abrir el panel

        yield return StartCoroutine(CameraManager.Instance.Fade(1));

        SceneManager.LoadScene(mainMenuScene);
    }

    private void ApplyMusicVolume(float value)
    {
        if (audioMixer == null) return;
        // Los sliders van de 0 a 1, el Audio Mixer trabaja en decibelios (logarítmico)
        float db = value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f;
        audioMixer.SetFloat(musicMixerParam, db);
    }

    private void ApplySfxVolume(float value)
    {
        if (audioMixer == null) return;
        float db = value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f;
        audioMixer.SetFloat(sfxMixerParam, db);
    }

    private void ApplyFullscreen(bool value)
    {
        Screen.fullScreen = value;
        Debug.Log($"[SettingsManager] Fullscreen aplicado: {value} (Screen.fullScreenMode = {Screen.fullScreenMode}). " +
                   "Nota: esto NO se ve reflejado dentro del Editor, solo en una build.");
    }

    private void ApplyVSync(bool value)
    {
        QualitySettings.vSyncCount = value ? 1 : 0;
        Debug.Log($"[SettingsManager] VSync aplicado: {value} (QualitySettings.vSyncCount = {QualitySettings.vSyncCount}). " +
                   "Nota: dentro del Editor puede verse afectado por el VSync propio del Game View.");
    }
}