using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameIntroCinematic : MonoBehaviour
{
    private const string MenuSceneName = "MainMenu";

    [SerializeField] private bool playOnStart = true;
    [SerializeField] private CinematicDirector director;
    [SerializeField] private GameObject skipHint;

    [Header("Jugador")]
    [SerializeField] private Transform playerStart;
    [SerializeField] private Transform playerEnd;
    [SerializeField] private float walkSpeed = 3f;

    [Header("Tomas")]
    [SerializeField] private CinematicShot establishingShot;
    [SerializeField] private CinematicShot approachShot;
    [SerializeField] private CinematicShot gateShot;
    [SerializeField] private float fadeInDuration = 2f;

    private static bool alreadyPlayed;

    private Transform player;
    private CharacterController playerController;
    private Animator playerAnimator;
    private FenceGate gate;
    private bool isPlaying;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        alreadyPlayed = false;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == MenuSceneName) alreadyPlayed = false;
    }

    private void Start()
    {
        if (skipHint != null) skipHint.SetActive(false);
        if (!playOnStart || director == null || director.Player == null) return;

        player = director.Player;
        playerController = player.GetComponent<CharacterController>();
        playerAnimator = player.GetComponent<Animator>();
        gate = FindFirstObjectByType<FenceGate>();

        if (alreadyPlayed)
        {
            PlacePlayer(playerEnd);
            return;
        }

        alreadyPlayed = true;
        StartCoroutine(PlayIntro());
    }

    private void Update()
    {
        if (!isPlaying || Keyboard.current == null || !Keyboard.current.spaceKey.wasPressedThisFrame) return;

        Skip();
    }

    private IEnumerator PlayIntro()
    {
        isPlaying = true;
        if (skipHint != null) skipHint.SetActive(true);

        director.SetFade(1f);
        director.Begin();
        PlacePlayer(playerStart);

        StartCoroutine(director.Fade(1f, 0f, fadeInDuration));
        yield return director.PlayShot(establishingShot);

        if (gate != null) gate.Interact(player.GetComponent<PlayerInventory>());
        StartCoroutine(WalkToEnd());

        yield return director.PlayShot(approachShot);
        yield return director.PlayShot(gateShot);

        Finish();
        yield return director.End();
    }

    private IEnumerator WalkToEnd()
    {
        Vector3 toEnd = playerEnd.position - player.position;
        toEnd.y = 0f;

        while (toEnd.magnitude > 0.1f)
        {
            Vector3 direction = toEnd.normalized;
            player.rotation = Quaternion.LookRotation(direction);
            playerController.Move((direction * walkSpeed + Vector3.down) * Time.deltaTime);
            playerAnimator.SetFloat("moveAmount", 0.5f, 0.1f, Time.deltaTime);

            yield return null;

            toEnd = playerEnd.position - player.position;
            toEnd.y = 0f;
        }

        while (playerAnimator.GetFloat("moveAmount") > 0.01f)
        {
            playerAnimator.SetFloat("moveAmount", 0f, 0.1f, Time.deltaTime);
            yield return null;
        }
    }

    private void Skip()
    {
        StopAllCoroutines();
        Finish();
        PlacePlayer(playerEnd);
        if (gate != null) gate.SlamShut();
        director.EndImmediately();
    }

    private void Finish()
    {
        isPlaying = false;
        if (skipHint != null) skipHint.SetActive(false);
    }

    private void PlacePlayer(Transform point)
    {
        playerController.enabled = false;
        player.SetPositionAndRotation(point.position, point.rotation);
        playerController.enabled = true;
    }
}
