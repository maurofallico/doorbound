using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrol : MonoBehaviour
{
    private enum State { Patrolling, Chasing }
    private State currentState = State.Patrolling;

    private enum PatrolMode
    {
        Loop,
        PingPong
    }

    [SerializeField] private Transform[] waypoints;

    [Header("Circular Room")]
    [SerializeField] private bool restrictToRoom;
    [SerializeField] private Vector3 roomCenter;
    [SerializeField] private Vector2 roomRadii = new Vector2(10.65f, 13.25f);
    [SerializeField] private float wallMargin = 0.75f;
    [SerializeField] private float circularPatrolRadius = 5f;
    [SerializeField] private float roomHeightTolerance = 2f;

    [SerializeField] private PatrolMode patrolMode = PatrolMode.PingPong;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waypointReachedThreshold = 0.1f;
    [SerializeField] private float waitTimeAtWaypoint = 0f;
    [SerializeField] private float chaseSpeed = 3.5f;

    [SerializeField] private float loseSightDelay = 2f;
    [SerializeField] private float viewRadius = 6f;

    [Range(0, 360)]
    [SerializeField] private float viewAngle = 90f;

    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool rotateTowardsMovement = true;

    [Header("Visual Feedback")]
    [SerializeField] private Renderer enemyRenderer;
    [SerializeField] private Color chasingColor = Color.red;

    private float waitTimer = 0f;
    private bool isWaiting = false;
    private Transform playerTransform;
    private NavMeshAgent agent;
    private Vector3 spawnPosition;
    private float loseSightTimer = 0f;
    private bool defeatPending;

    private IPatrolStrategy patrolStrategy;
    private RoomBounds roomBounds;
    private EnemyMove mover;
    private EnemyVision vision;
    private EnemyVisualFeedback visualFeedback;
    private PlayerContactHandler contactHandler;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        spawnPosition = transform.position;
        if (enemyRenderer == null)
        {
            enemyRenderer = GetComponent<Renderer>();
        }

        roomBounds = new RoomBounds(restrictToRoom, roomCenter, roomRadii, wallMargin, roomHeightTolerance);
        patrolStrategy = CreatePatrolStrategy();
        mover = new EnemyMove(agent, rotateTowardsMovement);
        vision = new EnemyVision(transform, roomBounds, viewRadius, viewAngle, obstacleMask);
        visualFeedback = new EnemyVisualFeedback(enemyRenderer, chasingColor);
        contactHandler = new PlayerContactHandler(playerTag);
        ApplyStateColor();

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        if (defeatPending) return;

        DetectPlayer();

        if (currentState == State.Chasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
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

    private IPatrolStrategy CreatePatrolStrategy()
    {
        if (restrictToRoom)
        {
            return new CircularPatrolStrategy(roomCenter, circularPatrolRadius);
        }

        if (HasWaypoints())
        {
            return new WaypointPatrolStrategy(waypoints, patrolMode == PatrolMode.PingPong);
        }

        return new NavMeshPatrolStrategy(spawnPosition, circularPatrolRadius);
    }

    public void SetWaypoints(Transform[] newWaypoints)
    {
        waypoints = newWaypoints;
        restrictToRoom = false;
        patrolStrategy = CreatePatrolStrategy();
    }

    private bool HasWaypoints()
    {
        if (waypoints == null) return false;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                return true;
            }
        }

        return false;
    }

    private void DetectPlayer()
    {
        EnemyDetectionResult result = vision.Detect(playerTransform, currentState == State.Chasing);
        if (result == EnemyDetectionResult.NoPlayer)
        {
            return;
        }

        if (result == EnemyDetectionResult.OutsideRoom)
        {
            loseSightTimer = 0f;
            SetState(State.Patrolling);
            return;
        }

        if (result == EnemyDetectionResult.Visible)
        {
            SetState(State.Chasing);
            loseSightTimer = loseSightDelay;
        }
        else if (currentState == State.Chasing)
        {
            loseSightTimer -= Time.deltaTime;
            if (loseSightTimer <= 0f)
            {
                SetState(State.Patrolling);
            }
        }
    }

    private Vector3 GetFacingDirection()
    {
        return transform.forward;
    }

    private void ChasePlayer()
    {
        if (playerTransform == null)
        {
            SetState(State.Patrolling);
            return;
        }

        MoveTowardsTarget(playerTransform.position, chaseSpeed);
    }

    private void Patrol()
    {
        if (patrolStrategy == null || !patrolStrategy.HasTarget)
        {
            return;
        }

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
            }
            return;
        }

        Vector3 target = patrolStrategy.GetTarget(transform.position);
        bool reachedTarget = !restrictToRoom && patrolStrategy.HasReachedTarget(transform.position, waypointReachedThreshold);

        MoveTowardsTarget(target, patrolSpeed);

        if (restrictToRoom)
        {
            reachedTarget = patrolStrategy.HasReachedTarget(transform.position, waypointReachedThreshold);
        }

        if (!reachedTarget)
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
        mover.MoveTowards(roomBounds.ClampTarget(targetPosition), speed);
    }

    private void SetState(State nextState)
    {
        if (currentState == nextState)
        {
            return;
        }

        currentState = nextState;
        if (restrictToRoom && nextState == State.Patrolling && patrolStrategy is CircularPatrolStrategy circularPatrol)
        {
            circularPatrol.SetClosestNextWaypoint(transform.position);
        }
        ApplyStateColor();
    }

    private bool IsInsideRoom(Vector3 position)
    {
        return roomBounds.Contains(position);
    }

    private void ApplyStateColor()
    {
        visualFeedback.Apply(currentState == State.Chasing);
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
        if (defeatPending) return;

        if (contactHandler.IsPlayerContact(other))
        {
            defeatPending = true;
        }
    }

    private void OnDrawGizmos()
    {
        if (restrictToRoom)
        {
            Vector3 previous = roomCenter + Vector3.right * roomRadii.x;
            Gizmos.color = Color.green;
            for (int i = 1; i <= 64; i++)
            {
                float angle = i * Mathf.PI * 2f / 64f;
                Vector3 next = roomCenter + new Vector3(Mathf.Cos(angle) * roomRadii.x, 0f, Mathf.Sin(angle) * roomRadii.y);
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }

        if (!restrictToRoom && waypoints != null && waypoints.Length > 0)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                Gizmos.DrawSphere(waypoints[i].position, 0.15f);

                Transform next = waypoints[(i + 1) % waypoints.Length];
                if (next != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, next.position);
                }
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 facing = Application.isPlaying
            ? GetFacingDirection()
            : transform.forward;

        Vector3 leftBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * facing;
        Vector3 rightBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * facing;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);
    }
}
