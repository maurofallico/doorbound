using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public sealed class DoorboundAudio : MonoBehaviour
{
    public enum Cue
    {
        DoorOpen, DoorClose, Gate, Bars, LightOn, LightOff, ClockHour, ClockMinute,
        Button, SecretDoor, NoteOpen, NoteClose, Exhausted, PuzzleSolved
    }

    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup ambientGroup;
    [SerializeField] private AudioClip exploration;
    [SerializeField] private AudioClip indoorAmbience;
    [SerializeField] private AudioClip outdoorAmbience;
    [SerializeField] private AudioClip outdoorWind;
    [SerializeField] private AudioClip[] indoorSteps;
    [SerializeField] private AudioClip[] outdoorSteps;
    [SerializeField] private AudioClip[] keyVariants;
    [SerializeField] private AudioClip[] cues;

    private static DoorboundAudio current;
    private readonly HashSet<GameObject> keysWithAudio = new();
    private PlayerMovement player;
    private EnemyRoom enemyRoom;
    private Transform entrance;
    private Vector3 outsideDirection;
    private AudioSource music;
    private AudioSource stinger;
    private AudioSource interior;
    private AudioSource exterior;
    private AudioSource wind;
    private AudioSource feedback;
    private float musicDuckUntil;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Register()
    {
        current = null;
        SceneManager.sceneLoaded -= SetUpScene;
        SceneManager.sceneLoaded += SetUpScene;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void SetUpFirstScene() => SetUpScene(SceneManager.GetActiveScene(), LoadSceneMode.Single);

    private static void SetUpScene(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single ||
            (scene.name != "GameScene" && scene.name != "GameScene_UI" && scene.name != "MainMenu") ||
            (current != null && current.gameObject.scene == scene))
            return;

        var prefab = Resources.Load<GameObject>("DoorboundAudio");
        if (prefab == null || !prefab.TryGetComponent<DoorboundAudio>(out var setup))
        {
            Debug.LogError("Doorbound Audio: falta el prefab Resources/DoorboundAudio o su componente.");
            return;
        }

        current = Instantiate(setup);
        current.name = "Doorbound Audio";
        SceneManager.MoveGameObjectToScene(current.gameObject, scene);
        current.Configure(scene);
    }

    private void Configure(Scene scene)
    {
        player = FindFirstObjectByType<PlayerMovement>();
        enemyRoom = FindFirstObjectByType<EnemyRoom>();
        var entranceDoor = FindFirstObjectByType<MansionEntranceDoor>();
        if (player != null && entranceDoor != null)
        {
            entrance = entranceDoor.transform;
            outsideDirection = Vector3.ProjectOnPlane(player.transform.position - entrance.position, Vector3.up).normalized;
        }

        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var pickup in root.GetComponentsInChildren<AudioKeyPickup>(true))
            {
                if (pickup.isActiveAndEnabled && pickup.HasAssignedAudio)
                    keysWithAudio.Add(pickup.Key);
            }

            foreach (var source in root.GetComponentsInChildren<AudioSource>(true))
            {
                if (source.outputAudioMixerGroup == null && !Protected(source.transform))
                    source.outputAudioMixerGroup = sfxGroup;
            }
        }

        music = Loop("Música de exploración", exploration, musicGroup);
        stinger = AddSource(gameObject, musicGroup);
        feedback = AddSource(gameObject, sfxGroup);
        if (player == null)
            return;

        interior = Loop("Ambiente interior", indoorAmbience, ambientGroup);
        exterior = Loop("Grillos del exterior", outdoorAmbience, ambientGroup);
        wind = Loop("Viento del exterior", outdoorWind, ambientGroup);

        if (!player.TryGetComponent<AudioPasos>(out var steps))
            steps = player.gameObject.AddComponent<AudioPasos>();
        steps.ConfigureFallback(indoorSteps, outdoorSteps, sfxGroup);
    }

    private AudioSource Loop(string objectName, AudioClip clip, AudioMixerGroup group)
    {
        var track = new GameObject(objectName);
        track.transform.SetParent(transform, false);
        var source = AddSource(track, group);
        source.clip = clip;
        source.loop = true;
        source.volume = 0f;
        if (clip != null)
            source.Play();
        return source;
    }

    private void Update()
    {
        bool protectedRoom = player != null && Protected(player.transform);
        bool outside = player != null && IsOutside(player.transform.position);
        bool ducked = Time.unscaledTime < musicDuckUntil;
        Fade(music, protectedRoom ? 0f : ducked ? 0.16f : 0.65f);
        Fade(stinger, protectedRoom ? 0f : 1f);
        Fade(interior, protectedRoom || outside ? 0f : 0.55f);
        Fade(exterior, outside && !protectedRoom ? 0.55f : 0f);
        Fade(wind, outside && !protectedRoom ? 0.22f : 0f);
    }

    private static void Fade(AudioSource source, float target)
    {
        if (source != null)
            source.volume = Mathf.MoveTowards(source.volume, target, Time.unscaledDeltaTime * 1.6f);
    }

    public static bool UsesExteriorSteps(Transform at) =>
        current != null && current.IsOutside(at.position);

    public static bool IsProtected(Transform at) => current != null && current.Protected(at);

    public static void PlayIfUnassigned(Cue cue, Component origin)
    {
        if (current != null && origin.GetComponentInChildren<AudioSource>(true) == null)
            Play(cue, origin);
    }

    private bool IsOutside(Vector3 position) =>
        entrance != null && Vector3.Dot(position - entrance.position, outsideDirection) > 0f;

    private bool Protected(Transform at)
    {
        if (enemyRoom != null && enemyRoom.Contains(at.position))
            return true;

        for (var ancestor = at; ancestor != null; ancestor = ancestor.parent)
        {
            if (ancestor.name.Contains("CircularRoom") || ancestor.name.Contains("Circular_Room"))
                return true;
        }

        return false;
    }

    public static void Play(Cue cue, Component origin)
    {
        if (current == null || current.Protected(origin.transform) ||
            (current.player != null && current.Protected(current.player.transform)))
            return;

        var clip = current.cues[(int)cue];
        if (clip == null)
            return;

        if (cue == Cue.PuzzleSolved)
        {
            current.musicDuckUntil = Time.unscaledTime + clip.length;
            current.stinger.PlayOneShot(clip, 0.65f);
            return;
        }

        current.feedback.PlayOneShot(clip, 0.65f);
    }

    public static void PlayKey(KeyPickup key)
    {
        if (current == null || current.Protected(key.transform) ||
            (current.player != null && current.Protected(current.player.transform)) ||
            current.keysWithAudio.Contains(key.gameObject) || key.GetComponentInChildren<AudioSource>(true) != null)
            return;

        var variants = current.keyVariants;
        if (variants.Length > 0)
            current.feedback.PlayOneShot(variants[Random.Range(0, variants.Length)], Random.Range(0.65f, 0.85f));
    }

    private static AudioSource AddSource(GameObject host, AudioMixerGroup group)
    {
        var source = host.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.outputAudioMixerGroup = group;
        source.spatialBlend = 0f;
        return source;
    }
}
