using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrol : MonoBehaviour
{
    private enum State { Patrolling, Investigating, Observing, Chasing }
    private State currentState = State.Patrolling;

    [Header("Patrol")]
    [Range(0.1f, 1f)]
    [SerializeField] private float patrolSize = 0.5f;
    [SerializeField] private float patrolSpeed = 2.5f;
    [SerializeField] private float waypointReachedThreshold = 0.5f;
    [SerializeField] private float waitTimeAtWaypoint = 0.5f;
    [SerializeField] private float chaseSpeed = 4.5f;

    [Header("Vision")]
    [SerializeField] private float loseSightDelay = 3f;
    [SerializeField] private float viewRadius = 12f;

    [Range(0, 360)]
    [SerializeField] private float viewAngle = 120f;

    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float playerTargetHeight = 1f;

    [Header("Observation")]
    [SerializeField] private float observeDuration = 0.75f;
    [SerializeField] private float observeTurnSpeed = 8f;
    [SerializeField] private float instantChaseDistance = 4f;

    [Header("Hearing")]
    [SerializeField] private float hearingRadius = 8f;
    [SerializeField] private float investigateSpeed = 2.5f;
    [SerializeField] private float investigateReachedThreshold = 0.5f;
    [SerializeField] private float investigateWaitTime = 2f;
    [SerializeField] private float noiseSampleDistance = 2f;

    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool rotateTowardsMovement = true;

    [Header("Visual Feedback")]
    [SerializeField] private Renderer enemyRenderer;
    [SerializeField] private Color chasingColor = Color.red;

    private float waitTimer = 0f;
    private bool isWaiting = false;
    private Transform playerTransform;
    private NavMeshAgent agent;
    private float loseSightTimer = 0f;
    private float observeTimer = 0f;
    private Vector3 investigatePosition;
    private Vector3 lastKnownPlayerPosition;
    private bool defeatPending;

    private CircularPatrolStrategy patrolStrategy;
    private EnemyRoom room;
    private EnemyMove mover;
    private EnemyVision vision;
    private EnemyHearing hearing;
    private EnemyVisualFeedback visualFeedback;
    private PlayerContactHandler contactHandler;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (enemyRenderer == null)
        {
            enemyRenderer = GetComponent<Renderer>();
        }

        room = FindFirstObjectByType<EnemyRoom>();
        if (room == null)
        {
            Debug.LogError("No hay ningun objeto con EnemyRoom en la escena, por lo que el enemigo no puede patrullar.", this);
            enabled = false;
            return;
        }

        patrolStrategy = new CircularPatrolStrategy(room, patrolSize);
        mover = new EnemyMove(agent, rotateTowardsMovement);
        vision = new EnemyVision(transform, room, viewRadius, viewAngle, obstacleMask, playerTargetHeight);
        hearing = new EnemyHearing(transform, room, hearingRadius);
        visualFeedback = new EnemyVisualFeedback(enemyRenderer, chasingColor);
        contactHandler = new PlayerContactHandler(playerTag);
        ApplyStateColor();

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private void Start()
    {
        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{name} no esta sobre un NavMesh en {transform.position} y no se va a mover", this);
        }
    }

    private void OnEnable()
    {
        NoiseEvents.NoiseEmitted += HandleNoise;
    }

    private void OnDisable()
    {
        NoiseEvents.NoiseEmitted -= HandleNoise;
    }

    private void Update()
    {
        if (defeatPending) return;

        DetectPlayer();

        switch (currentState)
        {
            case State.Chasing:
                ChasePlayer();
                break;
            case State.Observing:
                ObservePlayer();
                break;
            case State.Investigating:
                Investigate();
                break;
            default:
                Patrol();
                break;
        }
    }

    private void LateUpdate()
    {
        if (!defeatPending) return;

        agent.ResetPath();
        enabled = false;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void DetectPlayer()
    {
        EnemyDetectionResult result = vision.Detect(playerTransform, currentState == State.Chasing);
        if (result == EnemyDetectionResult.NoPlayer)
        {
            return;
        }

        if (result == EnemyDetectionResult.Visible)
        {
            lastKnownPlayerPosition = playerTransform.position;
        }

        if (currentState == State.Chasing)
        {
            if (result == EnemyDetectionResult.Visible)
            {
                loseSightTimer = loseSightDelay;
                return;
            }

            loseSightTimer -= Time.deltaTime;
            if (loseSightTimer <= 0f)
            {
                SearchLastKnownPosition();
            }
            return;
        }

        if (result == EnemyDetectionResult.OutsideRoom)
        {
            if (currentState == State.Observing)
            {
                SearchLastKnownPosition();
            }
            return;
        }

        if (currentState == State.Observing)
        {
            return;
        }

        if (result == EnemyDetectionResult.Visible)
        {
            if (Vector3.Distance(transform.position, playerTransform.position) <= instantChaseDistance)
            {
                loseSightTimer = loseSightDelay;
                SetState(State.Chasing);
            }
            else
            {
                observeTimer = 0f;
                SetState(State.Observing);
            }
        }
    }

    private void SearchLastKnownPosition()
    {
        if (!NavMesh.SamplePosition(lastKnownPlayerPosition, out NavMeshHit hit, noiseSampleDistance, NavMesh.AllAreas))
        {
            SetState(State.Patrolling);
            return;
        }

        investigatePosition = hit.position;
        SetState(State.Investigating);
    }

    private void HandleNoise(Vector3 noisePosition, float loudness)
    {
        if (defeatPending || currentState == State.Observing)
        {
            return;
        }

        if (!hearing.CanHear(noisePosition, loudness))
        {
            return;
        }

        if (currentState == State.Chasing)
        {
            lastKnownPlayerPosition = noisePosition;
            loseSightTimer = loseSightDelay;
            return;
        }

        if (!NavMesh.SamplePosition(noisePosition, out NavMeshHit hit, noiseSampleDistance, NavMesh.AllAreas))
        {
            return;
        }

        investigatePosition = hit.position;
        isWaiting = false;
        SetState(State.Investigating);
    }

    private void ObservePlayer()
    {
        if (playerTransform == null)
        {
            SetState(State.Patrolling);
            return;
        }

        mover.Stop();
        mover.FaceTowards(playerTransform.position, observeTurnSpeed);

        observeTimer += Time.deltaTime;
        visualFeedback.Apply(observeDuration > 0f ? observeTimer / observeDuration : 1f);

        if (observeTimer >= observeDuration)
        {
            loseSightTimer = loseSightDelay;
            SetState(State.Chasing);
        }
    }

    private void Investigate()
    {
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                SetState(State.Patrolling);
            }
            return;
        }

        MoveTowardsTarget(investigatePosition, investigateSpeed);

        if (!mover.HasArrived(investigateReachedThreshold))
        {
            return;
        }

        isWaiting = true;
        waitTimer = investigateWaitTime;
    }

    private void ChasePlayer()
    {
        if (playerTransform == null)
        {
            SetState(State.Patrolling);
            return;
        }

        MoveTowardsTarget(lastKnownPlayerPosition, chaseSpeed);
    }

    private void Patrol()
    {
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
            }
            return;
        }

        MoveTowardsTarget(patrolStrategy.GetTarget(), patrolSpeed);

        if (!patrolStrategy.HasReachedTarget(transform.position, waypointReachedThreshold) &&
            !mover.HasArrived(waypointReachedThreshold))
        {
            return;
        }

        if (waitTimeAtWaypoint > 0f)
        {
            isWaiting = true;
            waitTimer = waitTimeAtWaypoint;
        }

        patrolStrategy.Advance();
    }

    private void MoveTowardsTarget(Vector3 targetPosition, float speed)
    {
        mover.MoveTowards(room.ClampTarget(targetPosition), speed);
    }

    private void SetState(State nextState)
    {
        if (currentState == nextState)
        {
            return;
        }

        currentState = nextState;
        isWaiting = false;
        if (nextState == State.Patrolling)
        {
            patrolStrategy.SetClosestNextWaypoint(transform.position);
        }
        ApplyStateColor();
    }

    private void ApplyStateColor()
    {
        visualFeedback.Apply(currentState == State.Chasing ? 1f : 0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandlePlayerContact(collision.gameObject);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        HandlePlayerContact(hit.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandlePlayerContact(other.gameObject);
    }

    public void HandlePlayerContact(GameObject other)
    {
        if (defeatPending || room == null) return;

        if (contactHandler.IsPlayerContact(other))
        {
            defeatPending = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);

        if (Application.isPlaying && currentState == State.Investigating)
        {
            Gizmos.DrawSphere(investigatePosition, 0.2f);
        }

        Vector3 leftBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);
    }
}
