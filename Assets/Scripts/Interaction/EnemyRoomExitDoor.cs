using System;
using System.Collections;
using UnityEngine;

// El collider de este objeto es un trigger que cubre el vano de la puerta.
// Su eje Z (forward) tiene que apuntar hacia afuera de la habitacion.
// La posicion de doorLeaf en el editor es la de puerta cerrada.
[RequireComponent(typeof(BoxCollider))]
public class EnemyRoomExitDoor : MonoBehaviour
{
    [SerializeField] private Transform doorLeaf;
    [SerializeField] private Vector3 openOffset = new Vector3(0f, 3.6f, 0f);
    [SerializeField] private float openDuration = 1f;
    [SerializeField] private float slamDuration = 0.15f;
    [SerializeField] private float timeOpen = 8f;
    [SerializeField] private string playerTag = "Player";

    [Header("Trampa")]
    [SerializeField] private GameObject trapKey;

    [Header("Luces de alarma")]
    [SerializeField] private Light[] warningLights = new Light[0];
    [SerializeField] private float slowFlickerInterval = 0.6f;
    [SerializeField] private float fastFlickerInterval = 0.08f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip tickClip;
    [SerializeField] private AudioClip slamClip;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool trapSprung;
    private bool isRunning;
    private bool playerInDoorway;
    private bool lightsOn;
    private float flickerTimer;

    public event Action Closed;

    public bool CanOpen { get; private set; }
    public bool IsSolved { get; private set; }

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void Awake()
    {
        closedPosition = doorLeaf.localPosition;
        openPosition = closedPosition + openOffset;
        doorLeaf.localPosition = openPosition;
        SetLights(false);
    }

    private void Update()
    {
        if (trapSprung || trapKey == null || trapKey.activeInHierarchy) return;

        trapSprung = true;
        StartCoroutine(TrapRoutine());
    }

    public void OpenTimed()
    {
        if (!CanOpen) return;

        CanOpen = false;
        StartCoroutine(OpenRoutine());
    }

    private IEnumerator TrapRoutine()
    {
        GameHud.Instance?.ShowMessage("La entrada se cerró de golpe... tiene que haber otra forma de abrirla");
        yield return MoveLeaf(openPosition, closedPosition, slamDuration);
        PlayClip(slamClip);
        CanOpen = true;
    }

    private IEnumerator OpenRoutine()
    {
        isRunning = true;
        SetLights(true);
        PlayClip(openClip);
        GameHud.Instance?.ShowMessage("La entrada se está abriendo... no va a durar mucho");

        yield return MoveLeaf(closedPosition, openPosition, openDuration);

        float elapsed = 0f;
        flickerTimer = slowFlickerInterval;

        while (elapsed < timeOpen || playerInDoorway)
        {
            elapsed += Time.deltaTime;
            UpdateFlicker(Mathf.Clamp01(elapsed / timeOpen));
            yield return null;
        }

        SetLights(false);
        yield return MoveLeaf(openPosition, closedPosition, slamDuration);
        PlayClip(slamClip);

        isRunning = false;
        CanOpen = true;
        Closed?.Invoke();
    }

    private IEnumerator MoveLeaf(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            doorLeaf.localPosition = Vector3.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        doorLeaf.localPosition = to;
    }

    private void UpdateFlicker(float progress)
    {
        flickerTimer -= Time.deltaTime;
        if (flickerTimer > 0f) return;

        flickerTimer = Mathf.Lerp(slowFlickerInterval, fastFlickerInterval, progress);
        SetLights(!lightsOn);

        if (lightsOn) PlayClip(tickClip);
    }

    private void SetLights(bool on)
    {
        lightsOn = on;
        foreach (Light warningLight in warningLights)
            if (warningLight != null) warningLight.enabled = on;
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag)) playerInDoorway = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInDoorway = false;

        bool leftRoom = Vector3.Dot(other.transform.position - transform.position, transform.forward) > 0f;
        if (isRunning && leftRoom) Solve();
    }

    // La puerta queda abierta: si se cerrara, volver a entrar dejaria al jugador encerrado.
    private void Solve()
    {
        StopAllCoroutines();
        isRunning = false;
        IsSolved = true;
        doorLeaf.localPosition = openPosition;
        SetLights(false);
    }
}
