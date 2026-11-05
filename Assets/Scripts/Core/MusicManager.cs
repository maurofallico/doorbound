
using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambientMusic;
    [SerializeField] private AudioSource chaseMusic;
    [SerializeField] private AudioSource puzzleSolvedMusic;

    [Header("Fade Settings")]
    [SerializeField] private float fadeOutDuration = 2f;
    [SerializeField] private float fadeInDuration = 0.1f;

    private Coroutine fadeCoroutine;
    private bool isChasing;
    private bool puzzleSolved;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ambientMusic.volume = 1f;
        chaseMusic.volume = 0f;
        puzzleSolvedMusic.volume = 0f;

        ambientMusic.Play();
        chaseMusic.Play();

        puzzleSolvedMusic.loop = false;
    }

    public void StartChaseMusic()
    {
        if (isChasing || puzzleSolved) return;

        isChasing = true;

        FadeMusic(0f, 1f);
    }

    public void StopChaseMusic()
    {
        if (!isChasing || puzzleSolved) return;

        isChasing = false;

        FadeMusic(1f, 0f);
    }

    public void StartPuzzleSolvedMusic()
    {
        if (puzzleSolved) return;

        puzzleSolved = true;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(
            PuzzleSolvedCoroutine()
        );
    }

    private IEnumerator PuzzleSolvedCoroutine()
    {
        float elapsed = 0f;

        float ambientStart = ambientMusic.volume;
        float chaseStart = chaseMusic.volume;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = elapsed / fadeOutDuration;

            ambientMusic.volume = Mathf.Lerp(
                ambientStart, 0f, t
            );

            chaseMusic.volume = Mathf.Lerp(
                chaseStart, 0f, t
            );

            yield return null;
        }

        ambientMusic.volume = 0f;
        chaseMusic.volume = 0f;

        puzzleSolvedMusic.volume = 0f;
        puzzleSolvedMusic.Play();

        elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = elapsed / fadeInDuration;

            puzzleSolvedMusic.volume = Mathf.Lerp(
                0f, 1f, t
            );

            yield return null;
        }

        puzzleSolvedMusic.volume = 1f;

        yield return new WaitForSecondsRealtime(
            puzzleSolvedMusic.clip.length
        );

        puzzleSolvedMusic.Stop();

        elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = elapsed / fadeOutDuration;

            ambientMusic.volume = Mathf.Lerp(
                0f, 1f, t
            );

            yield return null;
        }

        ambientMusic.volume = 1f;
        fadeCoroutine = null;
    }

    private void FadeMusic(
        float ambientTarget,
        float chaseTarget)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(
            FadeCoroutine(ambientTarget, chaseTarget)
        );
    }


    private IEnumerator FadeCoroutine(
        float ambientTarget,
        float chaseTarget)
    {
        float ambientStart = ambientMusic.volume;
        float chaseStart = chaseMusic.volume;

        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = elapsed / fadeOutDuration;

            ambientMusic.volume = Mathf.Lerp(
                ambientStart,
                ambientTarget,
                t
            );

            chaseMusic.volume = Mathf.Lerp(
                chaseStart,
                chaseTarget,
                t
            );

            yield return null;
        }

        ambientMusic.volume = ambientTarget;
        chaseMusic.volume = chaseTarget;

        fadeCoroutine = null;
    }

}
