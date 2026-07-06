using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Dash Skill")]
    public bool hasDashSkill = true;
    [SerializeField] private float dashSpeed = 35f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private int skillDamage = 5;
    [SerializeField] private float skillCooldown = 5f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    [Header("UI")]
    [SerializeField] private SkillCooldown dashSkillUI;

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D playerCollider;

    private int currentHealth;
    private bool isPlayerDead;
    private bool isGrounded;
    private bool isAttacking;
    private bool isDashing;

    private float moveInput;
    private float originalScaleX;
    private float originalGravity;

    private int facingDir = 1;
    private Vector2 lockedDashDir;
    private Transform bossTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();

        originalScaleX = Mathf.Abs(transform.localScale.x);
        originalGravity = rb.gravityScale;
    }

    private void Start()
    {
        currentHealth = maxHealth;

        if (dashSkillUI != null)
            dashSkillUI.SetupUIOnUnlock();
    }

    private void Update()
    {
        if (Keyboard.current == null || Mouse.current == null)
            return;

        if (isPlayerDead)
            return;

        moveInput = 0f;

        if (Keyboard.current.aKey.isPressed)
            moveInput = -1f;

        if (Keyboard.current.dKey.isPressed)
            moveInput = 1f;

        if (!isDashing)
        {
            if (moveInput > 0f)
            {
                facingDir = 1;
                transform.localScale = new Vector3(
                    originalScaleX,
                    transform.localScale.y,
                    transform.localScale.z);
            }
            else if (moveInput < 0f)
            {
                facingDir = -1;
                transform.localScale = new Vector3(
                    -originalScaleX,
                    transform.localScale.y,
                    transform.localScale.z);
            }
        }

        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer);
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame &&
            isGrounded &&
            !isDashing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if (Mouse.current.leftButton.wasPressedThisFrame &&
            !isAttacking &&
            !isDashing)
        {
            StartCoroutine(AttackCoroutine());
        }

        bool isCooldown =
            dashSkillUI != null &&
            dashSkillUI.IsCooldown;

        if (Keyboard.current.fKey.wasPressedThisFrame &&
            hasDashSkill &&
            !isCooldown &&
            !isDashing)
        {
            StartCoroutine(DashSkillCoroutine());
        }

        animator.SetBool("isRunning", Mathf.Abs(moveInput) > 0.1f);
        animator.SetBool("isJumping", !isGrounded);
        animator.SetBool("isAttacking", isAttacking);
    }

    private void FixedUpdate()
    {
        if (isPlayerDead || isDashing)
            return;

        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y);
    }
    private IEnumerator DashSkillCoroutine()
    {
        isDashing = true;

        if (dashSkillUI != null)
            dashSkillUI.StartCooldown(skillCooldown);

        FindBoss();

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        lockedDashDir = facingDir == 1 ? Vector2.right : Vector2.left;

        float elapsed = 0f;
        bool hit = false;
        GameObject hitObject = null;

        RaycastHit2D[] hits = new RaycastHit2D[5];

        while (elapsed < dashDuration)
        {
            rb.linearVelocity = lockedDashDir * dashSpeed;

            if (playerCollider != null)
            {
                int hitCount = playerCollider.Cast(
                    lockedDashDir,
                    hits,
                    0.4f);

                for (int i = 0; i < hitCount; i++)
                {
                    Collider2D col = hits[i].collider;

                    if (col.CompareTag("Enemy") ||
                        col.CompareTag("Boss") ||
                        ((1 << col.gameObject.layer) & enemyLayer) != 0)
                    {
                        hit = true;
                        hitObject = col.gameObject;
                        break;
                    }
                }
            }

            if (hit)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero;

        if (!hit &&
            bossTransform != null &&
            Vector2.Distance(transform.position, bossTransform.position) < 1.6f)
        {
            hit = true;
            hitObject = bossTransform.gameObject;
        }

        if (hit && hitObject != null)
        {
            hitObject.SendMessage(
                "TakeDamage",
                skillDamage,
                SendMessageOptions.DontRequireReceiver);

            rb.linearVelocity = Vector2.zero;

            Vector2 knockbackDir = -lockedDashDir;

            rb.AddForce(
                new Vector2(
                    knockbackDir.x * 14f,
                    8.5f),
                ForceMode2D.Impulse);

            yield return new WaitForSeconds(0.35f);
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
        }

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = originalGravity;
        isDashing = false;
    }
    public void TakeDamage(int damage)
    {
        if (isPlayerDead)
            return;

        currentHealth = Mathf.Clamp(
            currentHealth - damage,
            0,
            maxHealth);

        if (currentHealth == 0)
            PlayerDie();
    }

    private void PlayerDie()
    {
        isPlayerDead = true;

        StopAllCoroutines();

        if (animator != null)
            animator.SetTrigger("die");

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        enabled = false;
    }

    private void FindBoss()
    {
        if (bossTransform != null)
            return;

        GameObject boss = GameObject.FindGameObjectWithTag("Boss");

        if (boss != null)
            bossTransform = boss.transform;
    }

    public void UnlockSkill()
    {
        hasDashSkill = true;

        if (dashSkillUI != null)
            dashSkillUI.SetupUIOnUnlock();
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;

        yield return new WaitForSeconds(0.1f);

        Attack();

        yield return new WaitForSeconds(attackDuration - 0.1f);

        isAttacking = false;
    }

    // Thay vì dùng biến đơn, bạn dùng hàm này để đảm bảo quét trúng cả Boss dù Layer thế nào
    private void Attack()
    {
        // Tạo mask quét: Lấy Layer Enemy hiện tại CỘNG THÊM Layer Boss
        int combinedLayer = enemyLayer | LayerMask.GetMask("Boss");

        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, combinedLayer);

        foreach (var obj in hitObjects)
        {
            obj.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(
                attackPoint.position,
                attackRadius);
        }
    }
}