using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //MOVEMENT
    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float runSpeed = 3.2f;
    [SerializeField] private float rotationSpeed = 160f;


    //JUMP
    [SerializeField] private float jumpForce = 4f;
    [SerializeField] private float floorDetection = 0.32f;
    [SerializeField] private LayerMask groundLayer;


    private Rigidbody rb;
    private Animator animator;

    private Vector2 moveInput;
    private bool isRunning;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        isGrounded = IsGrounded();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        Move();
        Rotate();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.ReadValueAsButton();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void Move()
    {
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        Vector3 movement = transform.forward * moveInput.y;

        rb.MovePosition(
            rb.position + movement * currentSpeed * Time.fixedDeltaTime
        );
    }

    private void Rotate()
    {
        float rotation = moveInput.x;

        Quaternion deltaRotation = Quaternion.Euler(
            0f,
            rotation * rotationSpeed * Time.fixedDeltaTime,
            0f
        );

        rb.MoveRotation(rb.rotation * deltaRotation);
    }

    private void UpdateAnimations()
    {
        bool movingForward = moveInput.y > 0.1f;
        bool movingBackward = moveInput.y < -0.1f;
        bool turningRight = moveInput.x > 0.1f && Mathf.Abs(moveInput.y) < 0.1f;
        bool turningLeft = moveInput.x < -0.1f && Mathf.Abs(moveInput.y) < 0.1f;

        animator.SetBool("running", isRunning);
        animator.SetBool("forward", movingForward);
        animator.SetBool("backward", movingBackward);
        animator.SetBool("turnRight", turningRight);
        animator.SetBool("turnLeft", turningLeft);
        animator.SetBool("jumping", !isGrounded);
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(
            transform.position,
            Vector3.down,
            floorDetection,
            groundLayer
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(
            transform.position,
            transform.position + Vector3.down * floorDetection
        );
    }
}