using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "SampleScene";
    private UIDocument uiDocument;
    private void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        root.Q<Button>("play-button").clicked += OnPlayClicked;
        // root.Q<Button>("options-button").clicked += OnOptionsClicked;
        // root.Q<Button>("profiles-button").clicked += OnProfilesClicked;
        root.Q<Button>("exit-button").clicked += OnExitClicked;
    }

    private void OnPlayClicked()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnOptionsClicked()
    {
        Debug.Log(" panel de opciones");
    }

    private void OnProfilesClicked()
    {
        Debug.Log("panel de perfiles");
    }

    private void OnExitClicked()
    {
        Debug.Log("Saliendo del juego");
        Application.Quit();
    }
}