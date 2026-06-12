using System.Collections;
using UnityEngine;

public class LoiPlayer : MonoBehaviour
{
    [Header("References")]
    public Transform worldRoot;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float attackDuration = 0.4f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    private Animator animator;
    private Rigidbody2D rb;

    private bool isGrounded;
    public bool isAttacking;

    private float moveInput;
    private float scaleX;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        scaleX = Mathf.Abs(transform.localScale.x);
    }

    void Update()
    {
        if (!isAttacking)
        {
            moveInput = Input.GetAxisRaw("Horizontal");

            // Đổi hướng nhân vật
            if (moveInput > 0)
            {
                transform.localScale = new Vector3(
                    scaleX,
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
            else if (moveInput < 0)
            {
                transform.localScale = new Vector3(
                    -scaleX,
                    transform.localScale.y,
                    transform.localScale.z
                );
            }

            HandleJump();
        }
        else
        {
            moveInput = 0f;
        }

        HandleAttack();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        // Player KHÔNG di chuyển ngang
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // Chỉ map di chuyển
        if (!isAttacking && worldRoot != null)
        {
            worldRoot.position += new Vector3(
                -moveInput * moveSpeed * Time.fixedDeltaTime,
                0f,
                0f
            );
        }
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            isGrounded = false;

            rb.linearVelocity = new Vector2(
                0f,
                0f
            );

            rb.AddForce(
                Vector2.up * jumpForce,
                ForceMode2D.Impulse
            );
        }
    }

    private void HandleAttack()
    {
        if (Input.GetButtonDown("Fire1") && !isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }
    }

    IEnumerator AttackCoroutine()
    {
        isAttacking = true;

        if (bulletPrefab != null && firePoint != null)
        {
            Quaternion bulletRotation = firePoint.rotation;

            if (transform.localScale.x < 0)
            {
                bulletRotation *= Quaternion.Euler(0f, 180f, 0f);
            }

            Instantiate(
                bulletPrefab,
                firePoint.position,
                bulletRotation
            );
        }

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        for (int i = 0; i < col.contactCount; i++)
        {
            if (col.GetContact(i).normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        isGrounded = false;
    }

    private void UpdateAnimator()
    {
        animator.SetBool("isRunning", Mathf.Abs(moveInput) > 0.1f);
        animator.SetBool("isJumping", !isGrounded);
        animator.SetBool("isAttacking", isAttacking);
    }
}