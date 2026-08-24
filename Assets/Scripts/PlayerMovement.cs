using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float rotationSpeed = 160f;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float floorDetection = 0.32f;
    [SerializeField] private float jumpForce = 4f;


    private Rigidbody rb;
    private Vector2 moveInput;

    private Animator animator;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
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
    
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            speed = 3.2f;
            animator.SetBool("running", true);
        }
        else
        {
            speed = 1.5f;
            animator.SetBool("running", false);

        }
    }

    private void Update()
    {
        Debug.DrawRay(
            transform.position,
            Vector3.down * floorDetection,
            Color.red
        );

        float rotation = moveInput.x;

        transform.Rotate(
            0f,
            rotation * rotationSpeed * Time.deltaTime,
            0f
        );

        Vector3 movement = transform.forward * moveInput.y;

        transform.position += movement * speed * Time.deltaTime;

        if (rotation > 0 && moveInput.y == 0)
        {
            animator.SetBool("turnRight", true);
        }
        else if (rotation < 0 && moveInput.y == 0)
        {
            animator.SetBool("turnLeft", true);
        }
        else
        {
            animator.SetBool("turnLeft", false);
            animator.SetBool("turnRight", false);
        }

        if (moveInput.y > 0)
        {
            animator.SetBool("forward", true);
            animator.SetBool("backward", false);
        }
        else if (moveInput.y < 0)
        {
            animator.SetBool("forward", false);
            animator.SetBool("backward", true);
        }
        else
        {
            animator.SetBool("forward", false);
            animator.SetBool("backward", false);
        }
        if (IsGrounded())
        {
            animator.SetBool("jumping", false);
        }
        else
        {
            animator.SetBool("jumping", true);
        }
    }
}