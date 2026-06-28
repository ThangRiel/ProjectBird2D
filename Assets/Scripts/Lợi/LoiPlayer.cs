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

    [Header("Attack (Normal)")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.6f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackDuration = 0.4f;

    [Header("Skill Dash")]
    public bool hasDashSkill = false;
    [SerializeField] private KeyCode skillKey = KeyCode.F;
    [SerializeField] private float dashSpeed = 35f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private int skillDamage = 5;
    [SerializeField] private float skillCooldown = 5f;

    [Header("UI")]
    [SerializeField] private SkillCooldown dashSkillUI;

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D playerCollider; 

    private float moveInput;
    private bool isGrounded;
    private bool isAttacking;
    private bool isDashing;

    private float originalScaleX;
    private float originalGravity;

    private Transform bossTransform;
    private int facingDir = 1;
    private Vector2 lockedDashDir; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>(); 
        originalScaleX = Mathf.Abs(transform.localScale.x);
    }

    private void Start()
    {
        FindBoss();

        if (hasDashSkill && dashSkillUI != null)
            dashSkillUI.SetupUIOnUnlock();
    }

    private void Update()
    {
        if (isDashing) return;

        if (!isAttacking)
            moveInput = Input.GetAxisRaw("Horizontal");
        else
            moveInput = 0f;

        if (moveInput > 0)
        {
            facingDir = 1;
            transform.localScale = new Vector3(originalScaleX, transform.localScale.y, transform.localScale.z);
        }
        else if (moveInput < 0)
        {
            facingDir = -1;
            transform.localScale = new Vector3(-originalScaleX, transform.localScale.y, transform.localScale.z);
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded && !isAttacking && !isDashing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if (Input.GetMouseButtonDown(0) && !isAttacking)
            StartCoroutine(AttackCoroutine());

        bool isCooldown = (dashSkillUI != null) ? dashSkillUI.IsCooldown : false;

        if (Input.GetKeyDown(skillKey) && hasDashSkill && !isCooldown && !isAttacking && !isDashing)
        {
            isDashing = true;
            moveInput = 0f; 

            // Khóa hướng lao theo hướng mặt nhìn
            lockedDashDir = (facingDir == 1) ? Vector2.right : Vector2.left;

            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            StartCoroutine(DashSkillCoroutine());
        }

        animator.SetBool("isRunning", Mathf.Abs(moveInput) > 0.1f);
        animator.SetBool("isJumping", !isGrounded);
        animator.SetBool("isAttacking", isAttacking);
    }

    private void FixedUpdate()
    {
        if (isDashing) return;
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    private IEnumerator DashSkillCoroutine()
    {
        // GIỮ NGUYÊN COLLIDER RẮN, KHÔNG BẬT TRIGGER (Chống xuyên thấu bậy bạ)
        if (playerCollider != null) playerCollider.isTrigger = false;

        if (dashSkillUI != null)
            dashSkillUI.StartCooldown(skillCooldown);

        originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        float elapsed = 0f;
        bool hit = false;

        // Mảng lưu kết quả quét va chạm vật lý trước mặt
        RaycastHit2D[] hits = new RaycastHit2D[5];

        while (elapsed < dashDuration)
        {
            // Liên tục ép vận tốc lao thẳng băng theo hướng mặt
            rb.linearVelocity = lockedDashDir * dashSpeed;

            // 🔥 CƠ CHẾ QUÉT VẬT LÝ AN TOÀN CAO: Quét trước mũi Player 0.4 đơn vị xem có chạm trúng Layer Enemy không
            if (playerCollider != null)
            {
                int hitCount = playerCollider.Cast(lockedDashDir, hits, 0.4f);
                for (int i = 0; i < hitCount; i++)
                {
                    // Nếu phát hiện vật cản có Tag Enemy hoặc thuộc Layer quái vật
                    if (hits[i].collider.CompareTag("Enemy") || ((1 << hits[i].collider.gameObject.layer) & enemyLayer) != 0)
                    {
                        hit = true;
                        break;
                    }
                }
            }

            if (hit) break; // Khóa phanh khẩn cấp, dừng lướt ngay lập tức!

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Dừng xe, triệt tiêu lực lao, trả lại trọng lực
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = originalGravity;

        // Nếu tia quét báo trúng Boss, hoặc khoảng cách toán học đủ gần
        if (!hit && bossTransform != null && Vector2.Distance(transform.position, bossTransform.position) < 1.6f)
        {
            hit = true;
        }

        if (hit && bossTransform != null)
        {
            bossTransform.SendMessage("TakeDamage", skillDamage, SendMessageOptions.DontRequireReceiver);
            
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // Hướng bật ngược (knockback)
            Vector2 knockbackDir = -lockedDashDir; 
            
            // Hất văng cầu vồng dội ngược chuẩn xác ra phía sau
            rb.AddForce(new Vector2(knockbackDir.x * 14f, 8.5f), ForceMode2D.Impulse);
            Debug.Log("💥 CHẠM RÌA VẬT LÝ BOSS! ĐÃ NẨY VỀ HƯỚNG: " + knockbackDir);

            yield return new WaitForSeconds(0.35f);
        }
        else
        {
            // Nếu lướt hụt
            yield return new WaitForSeconds(0.1f);
        }

        rb.linearVelocity = Vector2.zero;
        rb.WakeUp();
        isDashing = false; // Mở khóa hoàn toàn
    }

    private void FindBoss()
    {
        GameObject boss = GameObject.FindGameObjectWithTag("Enemy");
        if (boss != null) bossTransform = boss.transform;
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

    private void Attack()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);
        foreach (var enemy in enemies)
        {
            enemy.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}