using UnityEngine;
using UnityEngine.InputSystem;

public class RunAndFlyH : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Double Jump")]
    public int maxJumpCount = 2;

    [Header("Limit Movement")]
    public Transform anchorTransform;
    public float maxForwardOffset;
    public float maxBackwardOffset;

    [Header("Death")]
    public float outOfBoundsTimeLimit = 2f;

    [Header("Fall Recover")]
    public float fallThresholdAngle = 10f;
    public float uprightSpeed = 5f;

    [Header("Animation")]
    public float SpeedMultiplier = 0.5f;

    [Header("Dust")]
    public SimpleObjectPooler dustPooler;
    public float dustCreationRate = 0.2f;

    private Animator animator;
    private Rigidbody2D rb;

    private PlayerInputAction input;

    private Vector2 moveInput;

    private bool isGrounded = true;
    private bool isDead = false;

    private float nextDustTime;

    private float outOfBoundsTimer;

    private int jumpCount;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        input = new PlayerInputAction();

        dustPooler = FindAnyObjectByType<SimpleObjectPooler>();
    }

    void OnEnable()
    {
        input.Enable();

        input.Player.Move.performed += Move;
        input.Player.Move.canceled += Move;

        input.Player.Jump.performed += Jump;
    }

    void OnDisable()
    {
        input.Player.Move.performed -= Move;
        input.Player.Move.canceled -= Move;

        input.Player.Jump.performed -= Jump;

        input.Disable();
    }

    void Move(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    void Jump(InputAction.CallbackContext ctx)
    {
        if (jumpCount >= maxJumpCount)
            return;

        rb.linearVelocity =
            new Vector2(rb.linearVelocity.x, 0);

        rb.AddForce(
            Vector2.up * jumpForce,
            ForceMode2D.Impulse
        );

        jumpCount++;

        isGrounded = false;
    }

    void Update()
    {
        if (isDead)
            return;

        HandleMovement();

        UpdateAnimation();

        CheckOutOfBounds();

        CheckAndRecoverFromFall();

        SpawnDust();
    }

    void HandleMovement()
    {
        float horizontal = moveInput.x;

        if (anchorTransform != null)
        {
            float min =
                anchorTransform.position.x
                - maxBackwardOffset
                + 0.2f;

            float max =
                anchorTransform.position.x
                + maxForwardOffset
                - 0.2f;

            float x = transform.position.x;

            if (x <= min && horizontal < 0)
                horizontal = 0;

            if (x >= max && horizontal > 0)
                horizontal = 0;
        }

        rb.linearVelocity =
            new Vector2(
                horizontal * moveSpeed,
                rb.linearVelocity.y
            );
    }

    void UpdateAnimation()
    {
        animator.SetBool(
            "isFlying",
            !isGrounded
        );

        float y =
            rb.linearVelocity.y;

        float animSpeed =
            y > -1
                ? (1 + y) * SpeedMultiplier
                : 0.5f;

        animator.SetFloat(
            "verticalSpeed",
            animSpeed
        );
    }

    void SpawnDust()
    {
        if (!isGrounded)
            return;

        if (Mathf.Abs(rb.linearVelocity.x) < 0.5f)
            return;

        if (Time.time < nextDustTime)
            return;

        if (dustPooler == null)
            return;

        GameObject dust =
            dustPooler.GetPooledDustEffect();

        dust.transform.position =
            transform.position
            + Vector3.down * 0.66f;

        DustEffect effect =
            dust.GetComponent<DustEffect>();

        if (effect)
            effect.Activate();

        nextDustTime =
            Time.time + dustCreationRate;
    }

    void OnCollisionStay2D(Collision2D col)
    {
        for (int i = 0; i < col.contactCount; i++)
        {
            if (col.GetContact(i).normal.y > 0.5f)
            {
                isGrounded = true;

                jumpCount = 0;

                return;
            }
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        isGrounded = false;
    }

    void CheckOutOfBounds()
    {
        if (anchorTransform == null)
            return;

        float min =
            anchorTransform.position.x
            - maxBackwardOffset;

        float max =
            anchorTransform.position.x
            + maxForwardOffset;

        bool outside =
            transform.position.x < min
            || transform.position.x > max;

        if (outside)
        {
            outOfBoundsTimer += Time.deltaTime;

            if (outOfBoundsTimer >= outOfBoundsTimeLimit)
                Die();
        }
        else
        {
            outOfBoundsTimer = 0;
        }
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;

        rb.linearVelocity =
            Vector2.zero;

        Debug.Log("Game Over");
    }

    void CheckAndRecoverFromFall()
    {
        float z =
            transform.eulerAngles.z;

        if (z > 180)
            z -= 360;

        if (
            Mathf.Abs(z)
            > fallThresholdAngle
            && isGrounded
        )
        {
            Quaternion target =
                Quaternion.Euler(
                    0,
                    transform.eulerAngles.y,
                    0
                );

            transform.rotation =
                Quaternion.Lerp(
                    transform.rotation,
                    target,
                    Time.deltaTime
                    * uprightSpeed
                );
        }
    }
}