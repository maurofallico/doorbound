using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manager simple para centralizar el estado del juego y el Game Over.
/// Colocá este script en un GameObject vacío llamado, por ejemplo, "GameManager".
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Over")]
    [Tooltip("Panel de UI que se mostrará al perder (arrástralo desde la jerarquía). Puede quedar vacío.")]
    [SerializeField] private GameObject gameOverPanel;

    [Tooltip("Si es true, pausa el tiempo del juego (Time.timeScale = 0) al perder.")]
    [SerializeField] private bool pauseOnGameOver = true;

    [Tooltip("Si se asigna un nombre de escena, se recargará automáticamente tras 'reloadDelay' segundos.")]
    [SerializeField] private string sceneToReloadOnGameOver = "";

    [SerializeField] private float reloadDelay = 2f;

    public bool IsGameOver { get; private set; } = false;

    private void Awake()
    {
        // Singleton sencillo
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void GameOver()
    {
        if (IsGameOver) return; // Evita ejecutar la lógica más de una vez

        IsGameOver = true;
        Debug.Log("GAME OVER");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (pauseOnGameOver)
        {
            Time.timeScale = 0f;
        }

        if (!string.IsNullOrEmpty(sceneToReloadOnGameOver))
        {
            Invoke(nameof(ReloadScene), reloadDelay);
        }
    }

    private void ReloadScene()
    {
        Time.timeScale = 1f; // Restaurar el tiempo antes de recargar
        SceneManager.LoadScene(sceneToReloadOnGameOver);
    }

    // Método público útil si querés reiniciar manualmente (por ejemplo desde un botón de UI)
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}