using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyPatrol))]
public class EnemyAudio : MonoBehaviour
{
    [Header("Fuentes")]
    [SerializeField] private AudioSource enemySource;
    [SerializeField] private AudioSource musicSource;

    [Header("Pasos")]
    [SerializeField] private AudioClip[] stepClips = new AudioClip[0];
    [SerializeField] private float walkStepInterval = 0.7f;
    [SerializeField] private float chaseStepInterval = 0.4f;
    [SerializeField] private float minStepSpeed = 0.2f;

    [Header("Voz")]
    [SerializeField] private AudioClip discoverClip;
    [SerializeField] private AudioClip searchingClip;
    [SerializeField] private float discoverCooldown = 4f;

    [Header("Musica de persecucion")]
    [SerializeField] private float musicVolume = 0.8f;
    [SerializeField] private float musicFadeInSpeed = 2f;
    [SerializeField] private float musicFadeOutSpeed = 0.4f;

    private EnemyPatrol patrol;
    private NavMeshAgent agent;
    private EnemyPatrol.State previousState;
    private float stepTimer;
    private float lastDiscoverTime = float.NegativeInfinity;
    private int lastStepIndex = -1;

    private void Awake()
    {
        patrol = GetComponent<EnemyPatrol>();
        agent = GetComponent<NavMeshAgent>();
        previousState = patrol.CurrentState;
    }

    private void Update()
    {
        if (IsGameFinished())
        {
            StopAll();
            return;
        }

        EnemyPatrol.State state = patrol.CurrentState;
        if (state != previousState)
        {
            OnStateChanged(state);
            previousState = state;
        }

        UpdateSteps(state);
        UpdateMusic(state);
    }

    private bool IsGameFinished()
    {
        if (!patrol.enabled) return true;
        return GameManager.Instance != null && GameManager.Instance.IsGameOver;
    }

    private void OnStateChanged(EnemyPatrol.State state)
    {
        bool discovered = state == EnemyPatrol.State.Observing
            || (state == EnemyPatrol.State.Chasing && previousState != EnemyPatrol.State.Observing);

        if (discovered && Time.time - lastDiscoverTime >= discoverCooldown)
        {
            lastDiscoverTime = Time.time;
            PlayClip(discoverClip);
        }
        else if (state == EnemyPatrol.State.Investigating && previousState == EnemyPatrol.State.Patrolling)
        {
            PlayClip(searchingClip);
        }
    }

    private void UpdateSteps(EnemyPatrol.State state)
    {
        if (agent.velocity.magnitude < minStepSpeed)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;
        if (stepTimer > 0f) return;

        stepTimer = state == EnemyPatrol.State.Chasing ? chaseStepInterval : walkStepInterval;
        PlayStep();
    }

    private void PlayStep()
    {
        if (stepClips.Length == 0) return;

        int index = Random.Range(0, stepClips.Length);
        if (stepClips.Length > 1 && index == lastStepIndex)
            index = (index + 1) % stepClips.Length;

        lastStepIndex = index;
        PlayClip(stepClips[index]);
    }

    private void UpdateMusic(EnemyPatrol.State state)
    {
        if (musicSource == null || musicSource.clip == null) return;

        bool chasing = state == EnemyPatrol.State.Chasing;
        if (chasing && !musicSource.isPlaying)
        {
            musicSource.volume = 0f;
            musicSource.Play();
        }

        float speed = chasing ? musicFadeInSpeed : musicFadeOutSpeed;
        musicSource.volume = Mathf.MoveTowards(musicSource.volume, chasing ? musicVolume : 0f, speed * Time.deltaTime);

        if (!chasing && musicSource.isPlaying && musicSource.volume <= 0f) musicSource.Stop();
    }

    private void PlayClip(AudioClip clip)
    {
        if (enemySource != null && clip != null) enemySource.PlayOneShot(clip);
    }

    // Time.timeScale queda en 0 en Game Over, asi que la musica se corta aca y no con el fade.
    private void StopAll()
    {
        if (enemySource != null) enemySource.Stop();
        if (musicSource != null) musicSource.Stop();
        enabled = false;
    }
}
