using UnityEngine;
using UnityEngine.SceneManagement;

public class RetryButton : MonoBehaviour
{
 public void RestartLevel()
    {
        SceneManager.LoadScene("SampleScene");
        Time.timeScale = 1f;
    }
}
