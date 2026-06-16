using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class LoiPlayer : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Attack")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.6f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackDuration = 0.4f;

    private Rigidbody2D rb;
    private Animator animator;

    private float moveInput;
    private bool isGrounded;
    private bool isAttacking;

    private float originalScaleX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        originalScaleX = Mathf.Abs(transform.localScale.x);
    }

    private void Update()
    {
        if (!isAttacking)
            moveInput = Input.GetAxisRaw("Horizontal");
        else
            moveInput = 0f;

        // Lật nhân vật
        if (moveInput > 0)
            transform.localScale = new Vector3(originalScaleX, transform.localScale.y, transform.localScale.z);
        else if (moveInput < 0)
            transform.localScale = new Vector3(-originalScaleX, transform.localScale.y, transform.localScale.z);

        // Ground Check
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded && !isAttacking)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // Attack
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }

        animator.SetBool("isRunning", Mathf.Abs(moveInput) > 0.1f);
        animator.SetBool("isJumping", !isGrounded);
        animator.SetBool("isAttacking", isAttacking);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;

        yield return new WaitForSeconds(0.1f);

        Attack();

        yield return new WaitForSeconds(
            attackDuration - 0.1f
        );

        isAttacking = false;
    }

    private void Attack()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            enemyLayer
        );

        foreach (Collider2D enemy in enemies)
        {
            enemy.SendMessage(
                "TakeDamage",
                attackDamage,
                SendMessageOptions.DontRequireReceiver
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(
                attackPoint.position,
                attackRadius
            );
        }
    }
}