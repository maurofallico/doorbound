using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class EscapeSequence : MonoBehaviour
{
    private class HouseLight
    {
        public Light light;
        public float baseIntensity;
        public float seed;
    }

    [SerializeField] private GameObject exitTrigger;
    [SerializeField] private string playerTag = "Player";

    [Header("Luces que no titilan")]
    [SerializeField] private string[] steadyLightRoots = { "Courtyard_Decoration", "Exterior_EntranceLighting", "CinematicSystem", "EnemyRoomPuzzle" };

    [Header("Apagon")]
    [SerializeField] private float blackoutDuration = 1.4f;
    [SerializeField] private float minTimeBetweenBlackouts = 7f;
    [SerializeField] private float maxTimeBetweenBlackouts = 13f;
    [SerializeField] private float shortBlackoutDuration = 0.35f;

    [Header("Titileo")]
    [SerializeField] private float nearRadius = 18f;
    [SerializeField] private float nearFlickerSpeed = 9f;
    [SerializeField] private float farFlickerSpeed = 2.5f;
    [SerializeField] private float nearOffThreshold = 0.5f;
    [SerializeField] private float farOffThreshold = 0.28f;
    [SerializeField] private float offIntensity = 0.04f;

    [Header("Mensajes")]
    [SerializeField] private string escapeMessage = "La reja de la entrada se abrió. ¡Sal de la mansión!";
    [SerializeField] private string reminderMessage = "Escapa por la reja de la entrada";
    [SerializeField] private float reminderInterval = 14f;

    private readonly List<HouseLight> houseLights = new List<HouseLight>();
    private Transform player;
    private bool hasStarted;
    private bool isFlickering;
    private bool isBlackout;

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void Awake()
    {
        if (exitTrigger != null) exitTrigger.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasStarted || !other.CompareTag(playerTag)) return;

        hasStarted = true;
        player = other.transform;
        CollectHouseLights();
        StartCoroutine(EscapeRoutine());
    }

    private void CollectHouseLights()
    {
        foreach (Light houseLight in FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (houseLight.type == LightType.Directional || IsSteady(houseLight.transform)) continue;

            houseLights.Add(new HouseLight
            {
                light = houseLight,
                baseIntensity = houseLight.intensity,
                seed = Random.Range(0f, 100f)
            });
        }
    }

    private bool IsSteady(Transform lightTransform)
    {
        for (Transform current = lightTransform; current != null; current = current.parent)
        {
            foreach (string rootName in steadyLightRoots)
                if (current.name == rootName) return true;
        }

        return false;
    }

    private IEnumerator EscapeRoutine()
    {
        DoorboundAudio.Play(DoorboundAudio.Cue.LightOff, this);
        isBlackout = true;
        ApplyBlackout();

        yield return new WaitForSeconds(blackoutDuration);

        FenceGate gate = FindFirstObjectByType<FenceGate>();
        if (gate != null) gate.OpenForEscape();
        if (exitTrigger != null) exitTrigger.SetActive(true);

        isBlackout = false;
        isFlickering = true;
        GameHud.Instance?.ShowMessage(escapeMessage);

        StartCoroutine(RemindersRoutine());
        StartCoroutine(BlackoutsRoutine());
    }

    private IEnumerator RemindersRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(reminderInterval);
            GameHud.Instance?.ShowMessage(reminderMessage);
        }
    }

    private IEnumerator BlackoutsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minTimeBetweenBlackouts, maxTimeBetweenBlackouts));

            isBlackout = true;
            DoorboundAudio.Play(DoorboundAudio.Cue.LightOff, this);
            yield return new WaitForSeconds(shortBlackoutDuration);

            isBlackout = false;
            DoorboundAudio.Play(DoorboundAudio.Cue.LightOn, this);
        }
    }

    private void Update()
    {
        if (!isFlickering) return;

        if (isBlackout)
        {
            ApplyBlackout();
            return;
        }

        foreach (HouseLight houseLight in houseLights)
        {
            if (houseLight.light == null) continue;

            float distance = Vector3.Distance(houseLight.light.transform.position, player.position);
            float closeness = 1f - Mathf.Clamp01(distance / nearRadius);
            float speed = Mathf.Lerp(farFlickerSpeed, nearFlickerSpeed, closeness);
            float threshold = Mathf.Lerp(farOffThreshold, nearOffThreshold, closeness);

            float noise = Mathf.PerlinNoise(houseLight.seed, Time.time * speed);
            float factor = noise < threshold ? offIntensity : Mathf.Lerp(0.55f, 1f, noise);
            houseLight.light.intensity = houseLight.baseIntensity * factor;
        }
    }

    private void ApplyBlackout()
    {
        foreach (HouseLight houseLight in houseLights)
            if (houseLight.light != null) houseLight.light.intensity = houseLight.baseIntensity * offIntensity;
    }
}
