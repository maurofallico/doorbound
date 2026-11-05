using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class CinematicDirector : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinematicCamera;
    [SerializeField] private int activePriority = 40;
    [SerializeField] private float returnBlendTime = 1.2f;
    [SerializeField] private string playerTag = "Player";

    [Header("Pantalla")]
    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform bottomBar;
    [SerializeField] private float barHeight = 110f;
    [SerializeField] private float barSpeed = 250f;
    [SerializeField] private Image fadeImage;

    private PlayerMovement playerMovement;
    private PlayerInteractor playerInteractor;
    private Animator playerAnimator;
    private float currentBarHeight;
    private float targetBarHeight;

    public Transform Player { get; private set; }

    private void Awake()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
        {
            Player = playerObject.transform;
            playerMovement = playerObject.GetComponent<PlayerMovement>();
            playerInteractor = playerObject.GetComponent<PlayerInteractor>();
            playerAnimator = playerObject.GetComponent<Animator>();
        }

        SetBars(0f);
        SetFade(0f);
    }

    private void Update()
    {
        if (Mathf.Approximately(currentBarHeight, targetBarHeight)) return;

        SetBars(Mathf.MoveTowards(currentBarHeight, targetBarHeight, barSpeed * Time.deltaTime));
    }

    public void Begin()
    {
        SetPlayerLocked(true);
        targetBarHeight = barHeight;
    }

    public IEnumerator PlayShot(CinematicShot shot)
    {
        ApplyShot(shot, 0f);
        cinematicCamera.Priority.Value = activePriority;

        float elapsed = 0f;
        while (elapsed < shot.duration)
        {
            elapsed += Time.deltaTime;
            ApplyShot(shot, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / shot.duration)));
            yield return null;
        }
    }

    public IEnumerator End()
    {
        cinematicCamera.Priority.Value = 0;
        targetBarHeight = 0f;

        yield return new WaitForSeconds(returnBlendTime);

        SetPlayerLocked(false);
    }

    public void EndImmediately()
    {
        cinematicCamera.Priority.Value = 0;
        targetBarHeight = 0f;
        SetBars(0f);
        SetFade(0f);
        SetPlayerLocked(false);
    }

    public IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetFade(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }

        SetFade(to);
    }

    public void SetFade(float alpha)
    {
        if (fadeImage == null) return;

        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }

    private void ApplyShot(CinematicShot shot, float progress)
    {
        Transform camera = cinematicCamera.transform;
        Transform to = shot.to != null ? shot.to : shot.from;

        camera.position = Vector3.Lerp(shot.from.position, to.position, progress);
        camera.rotation = shot.lookAt != null
            ? Quaternion.LookRotation(shot.lookAt.position - camera.position)
            : Quaternion.Slerp(shot.from.rotation, to.rotation, progress);
    }

    private void SetPlayerLocked(bool locked)
    {
        if (playerMovement != null) playerMovement.enabled = !locked;
        if (playerInteractor != null) playerInteractor.SetInteractionLocked(locked);
        if (locked && playerAnimator != null) playerAnimator.SetFloat("moveAmount", 0f);

        if (GameHud.Instance != null)
        {
            Canvas hudCanvas = GameHud.Instance.GetComponent<Canvas>();
            if (hudCanvas != null) hudCanvas.enabled = !locked;
        }
    }

    private void SetBars(float height)
    {
        currentBarHeight = height;
        if (topBar != null) topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, height);
        if (bottomBar != null) bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, height);
    }
}
