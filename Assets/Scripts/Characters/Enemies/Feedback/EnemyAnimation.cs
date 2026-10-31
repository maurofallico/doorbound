using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAnimation : MonoBehaviour
{
    private const int WalkState = 0;
    private const int SearchState = 1;
    private const int ChaseState = 2;

    private static readonly int StateParameter = Animator.StringToHash("State");

    [SerializeField] private Animator animator;
    [SerializeField] private float movingSpeedThreshold = 0.2f;
    [SerializeField] private float chaseSpeedThreshold = 3.5f;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.applyRootMotion = false;
    }

    private void Update()
    {
        if (animator == null) return;

        animator.SetInteger(StateParameter, GetState());
    }

    private int GetState()
    {
        if (!agent.isOnNavMesh || agent.velocity.magnitude < movingSpeedThreshold) return SearchState;

        return agent.speed >= chaseSpeedThreshold ? ChaseState : WalkState;
    }
}
