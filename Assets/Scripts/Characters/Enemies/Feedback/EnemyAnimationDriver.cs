using UnityEngine;

[RequireComponent(typeof(EnemyPatrol))]
public class EnemyAnimationDriver : MonoBehaviour
{
    private static readonly int StateParameter = Animator.StringToHash("State");

    [SerializeField] private Animator animator;

    private EnemyPatrol patrol;

    private void Awake()
    {
        patrol = GetComponent<EnemyPatrol>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        if (animator != null)
        {
            animator.SetInteger(StateParameter, (int)patrol.CurrentState);
        }
    }
}
