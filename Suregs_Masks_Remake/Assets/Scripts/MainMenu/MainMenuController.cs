using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuController : MonoBehaviour
{
    [Header("Nombres de escena")]
    [SerializeField] private string nuevaPartidaScene = "Tutorial";
    [SerializeField] private string continuarScene = "Town";

    [Header("Paneles")]
    [SerializeField] private UIRevealAnimator ajustesPanel;
    [SerializeField] private UIRevealAnimator controlesImagen;
    [SerializeField] private UIRevealAnimator creditosImagen;


    [SerializeField] private UIRevealAnimator confirmarNuevaPartidaPopup;

    [Header("Botón Continuar")]

    [SerializeField] private Button continuarButton;
    [SerializeField] private string continueSaveKey = "HasSaveGame";

    // Claves de PlayerPrefs que pertenecen a Ajustes (deben sobrevivir a "Nueva Partida").
    private const string MUSIC_KEY = "musicVolume";
    private const string SFX_KEY = "sfxVolume";
    private const string FULLSCREEN_KEY = "fullscreen";
    private const string VSYNC_KEY = "vsync";

    private void Awake()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            Destroy(player);
        }
    }

    private void Start()
    {
        ActualizarBotonContinuar();
    }

    private void ActualizarBotonContinuar()
    {
        if (continuarButton == null) return;
        continuarButton.interactable = PlayerPrefs.HasKey(continueSaveKey);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAnyOpenPanel();
        }
    }

    private void CloseAnyOpenPanel()
    {
        if (ajustesPanel != null && ajustesPanel.IsOpen) { ajustesPanel.Hide(); return; }
        if (controlesImagen != null && controlesImagen.IsOpen) { controlesImagen.Hide(); return; }
        if (creditosImagen != null && creditosImagen.IsOpen) { creditosImagen.Hide(); return; }
        if (confirmarNuevaPartidaPopup != null && confirmarNuevaPartidaPopup.IsOpen) { confirmarNuevaPartidaPopup.Hide(); return; }
    }

    // ---------- NUEVA PARTIDA ----------

    public void OnNuevaPartida()
    {
        if (confirmarNuevaPartidaPopup != null)
            confirmarNuevaPartidaPopup.Show();
        else
            OnConfirmarNuevaPartida(); // por si no has montado el popup todavía
    }

    // Enganchado al botón "Confirmar" DENTRO del popup.
    public void OnConfirmarNuevaPartida()
    {
        if (confirmarNuevaPartidaPopup != null) confirmarNuevaPartidaPopup.Hide();
        StartCoroutine(FadeAndLoadScene(nuevaPartidaScene, borrarProgreso: true));
    }

    // Enganchado al botón "Cancelar" DENTRO del popup.
    public void OnCancelarNuevaPartida()
    {
        if (confirmarNuevaPartidaPopup != null) confirmarNuevaPartidaPopup.Hide();
    }

    // ---------- CONTINUAR ----------
    public void OnContinuar()
    {
        StartCoroutine(FadeAndLoadScene(continuarScene, borrarProgreso: false));
    }

    private IEnumerator FadeAndLoadScene(string sceneName, bool borrarProgreso)
    {
        yield return StartCoroutine(CameraManager.Instance.Fade(1));

        if (borrarProgreso)
        {
            BorrarProgresoConservandoAjustes();
        }

        SceneManager.LoadScene(sceneName);
    }

    // Borra todo el PlayerPrefs de progreso (incluida la clave de "hay partida guardada"),
    // pero conserva los ajustes (música, SFX, fullscreen, vsync).
    private void BorrarProgresoConservandoAjustes()
    {
        float music = PlayerPrefs.GetFloat(MUSIC_KEY, 0.75f);
        float sfx = PlayerPrefs.GetFloat(SFX_KEY, 0.75f);
        int fullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, Screen.fullScreen ? 1 : 0);
        int vsync = PlayerPrefs.GetInt(VSYNC_KEY, 1);

        PlayerPrefs.DeleteAll();

        PlayerPrefs.SetFloat(MUSIC_KEY, music);
        PlayerPrefs.SetFloat(SFX_KEY, sfx);
        PlayerPrefs.SetInt(FULLSCREEN_KEY, fullscreen);
        PlayerPrefs.SetInt(VSYNC_KEY, vsync);
        PlayerPrefs.Save();
    }

    // ---------- AJUSTES ----------
    public void OnAjustes()
    {
        if (ajustesPanel != null) ajustesPanel.Show();
    }

    public void OnCerrarAjustes()
    {
        if (ajustesPanel != null) ajustesPanel.Hide();
    }

    // ---------- CONTROLES ----------
    public void OnControles()
    {
        if (controlesImagen != null) controlesImagen.Show();
    }

    public void OnCerrarControles()
    {
        if (controlesImagen != null) controlesImagen.Hide();
    }

    // ---------- CREDITOS ----------
    public void OnCreditos()
    {
        if (creditosImagen != null) creditosImagen.Show();
    }

    public void OnCerrarCreditos()
    {
        if (creditosImagen != null) creditosImagen.Hide();
    }

    // ---------- SALIR ----------
    public void OnSalir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}