using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

   
    [SerializeField] private GameObject gameOverPanel;

   
    [SerializeField] private bool pauseOnGameOver = true;

    [SerializeField] private string sceneToReloadOnGameOver = "";

    [SerializeField] private float reloadDelay = 2f;

    public bool IsGameOver { get; private set; } = false;

    private void Awake()
    {
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void GameOver()
    {
        if (IsGameOver) return; 

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
        Time.timeScale = 1f; 
        SceneManager.LoadScene(sceneToReloadOnGameOver);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}