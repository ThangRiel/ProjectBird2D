using UnityEngine;

public class PlayerControllerH : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer = default;
    [SerializeField] private Transform groundCheck = null;
    [SerializeField] private Transform groundCheck1 = null;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private Vector3 facingRightScale = new(1.80351f, 1.80351f, 1.80351f);

    private Animator animator;
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool jumpRequested;
    private float moveInput;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        moveInput = Input.GetAxis("Horizontal");
        jumpRequested |= Input.GetButtonDown("Jump");

        UpdateFacingDirection();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        isGrounded = CheckGrounded();

        Vector2 velocity = rb.linearVelocity;
        velocity.x = moveInput * moveSpeed;

        if (jumpRequested && isGrounded)
        {
            velocity.y = jumpForce;
            isGrounded = false;
        }

        rb.linearVelocity = velocity;
        jumpRequested = false;
    }

    private bool CheckGrounded()
    {
        bool groundedAtFirstPoint = groundCheck != null && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        bool groundedAtSecondPoint = groundCheck1 != null && Physics2D.OverlapCircle(groundCheck1.position, groundCheckRadius, groundLayer);

        return groundedAtFirstPoint || groundedAtSecondPoint;
    }

    private void UpdateFacingDirection()
    {
        if (Mathf.Approximately(moveInput, 0f))
        {
            return;
        }

        float direction = Mathf.Sign(moveInput);
        transform.localScale = new Vector3(facingRightScale.x * direction, facingRightScale.y, facingRightScale.z);
    }

    private void UpdateAnimator()
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool("isRunning", Mathf.Abs(moveInput) > 0.1f);
        animator.SetBool("isJumping", !isGrounded);
    }
}
