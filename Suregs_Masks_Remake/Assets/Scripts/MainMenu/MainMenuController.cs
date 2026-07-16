using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador principal del MainMenu. Engancha aquí los OnClick de los 6 botones.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Nombres de escena")]
    [SerializeField] private string nuevaPartidaScene = "Tutorial";
    [SerializeField] private string continuarScene = "Town";

    [Header("Paneles / Imágenes con animación (UIRevealAnimator)")]
    [SerializeField] private UIRevealAnimator ajustesPanel;
    [SerializeField] private UIRevealAnimator controlesImagen;
    [SerializeField] private UIRevealAnimator creditosImagen;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAnyOpenPanel();
        }
    }

    // Cierra el primer panel que encuentre abierto (solo debería haber uno a la vez)
    private void CloseAnyOpenPanel()
    {
        if (ajustesPanel != null && ajustesPanel.IsOpen) { ajustesPanel.Hide(); return; }
        if (controlesImagen != null && controlesImagen.IsOpen) { controlesImagen.Hide(); return; }
        if (creditosImagen != null && creditosImagen.IsOpen) { creditosImagen.Hide(); return; }
    }

    // ---------- NUEVA PARTIDA ----------
    public void OnNuevaPartida()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        SceneManager.LoadScene(nuevaPartidaScene);
    }

    // ---------- CONTINUAR ----------
    public void OnContinuar()
    {
        SceneManager.LoadScene(continuarScene);
    }

    // ---------- AJUSTES ----------
    public void OnAjustes()
    {
        if (ajustesPanel != null) ajustesPanel.Show();
    }

    // Llama a esto desde un botón "Cerrar" dentro del propio panel de ajustes
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