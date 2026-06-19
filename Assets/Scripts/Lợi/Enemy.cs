using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 3;

    [Header("Stomp")]
    [SerializeField] private Transform stompPoint;
    [SerializeField] private float bounceForce = 8f;

    [Header("Patrol")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private bool spriteFacesRight = false;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int damageToPlayer = 10;

    [Header("Death")]
    [SerializeField] private float destroyDelay = 1f;

    private int currentHealth;

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private Transform playerTransform;

    private float leftLimit;
    private float rightLimit;
    private bool movingRight = true;

    private float nextAttackTime;
    private bool isDead = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
    }

    private void Start()
    {
        leftLimit = transform.position.x - patrolDistance;
        rightLimit = transform.position.x + patrolDistance;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (isDead)
            return;

        if (playerTransform == null)
        {
            Patrol();
            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            playerTransform.position
        );

        if (distance <= attackRange)
        {
            AttackPlayer();
        }
        else
        {
            Patrol();
        }
    }

    //=================================
    // TUẦN TRA
    //=================================
    private void Patrol()
    {
        anim.SetBool("isRunning", true);

        Vector3 pos = transform.position;

        if (movingRight)
        {
            pos.x += moveSpeed * Time.deltaTime;

            if (pos.x >= rightLimit)
            {
                pos.x = rightLimit;
                movingRight = false;
            }
        }
        else
        {
            pos.x -= moveSpeed * Time.deltaTime;

            if (pos.x <= leftLimit)
            {
                pos.x = leftLimit;
                movingRight = true;
            }
        }

        transform.position = pos;

        UpdateFacing(movingRight);
    }

    //=================================
    // TẤN CÔNG
    //=================================
    private void AttackPlayer()
    {
        anim.SetBool("isRunning", false);

        bool playerIsRight =
            playerTransform.position.x >
            transform.position.x;

        UpdateFacing(playerIsRight);

        if (Time.time >= nextAttackTime)
        {
            anim.SetTrigger("Attack");

            Debug.Log(
                gameObject.name +
                " đánh Player, gây " +
                damageToPlayer +
                " sát thương."
            );

            // Sau này thêm:
            // playerTransform.GetComponent<PlayerHealth>()
            //     ?.TakeDamage(damageToPlayer);

            nextAttackTime =
                Time.time + attackCooldown;
        }
    }

    //=================================
    // NHẬN SÁT THƯƠNG
    //=================================
    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        Debug.Log(
            gameObject.name +
            " nhận " +
            damage +
            " sát thương. Máu còn: " +
            currentHealth
        );

        if (currentHealth > 0)
        {
            anim.SetTrigger("Hit");
        }
        else
        {
            Die();
        }
    }

    //=================================
    // CHẾT
    //=================================
    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log(gameObject.name + " chết!");

        anim.ResetTrigger("Hit");
        anim.ResetTrigger("Attack");

        anim.SetBool("isRunning", false);
        anim.SetTrigger("Die");

        // Tắt toàn bộ collider
        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            col.enabled = false;
        }

        // Dừng vật lý
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        Destroy(gameObject, destroyDelay);
    }

    //=================================
    // PLAYER DẪM ĐẦU ENEMY
    //=================================
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead)
            return;

        if (!collision.gameObject.CompareTag("Player"))
            return;

        Rigidbody2D playerRb =
            collision.gameObject.GetComponent<Rigidbody2D>();

        if (playerRb == null)
            return;

        // Player phải đang rơi xuống
        if (playerRb.linearVelocity.y >= 0f)
            return;

        // Kiểm tra Player có ở trên StompPoint không
        if (stompPoint != null &&
            playerRb.position.y >= stompPoint.position.y)
        {
            Debug.Log(gameObject.name + " bị dẫm!");

            // Cho Player nảy lên
            playerRb.linearVelocity =
                new Vector2(
                    playerRb.linearVelocity.x,
                    bounceForce
                );

            // Enemy chết ngay
            Die();
        }
    }

    //=================================
    // ĐỔI HƯỚNG
    //=================================
    private void UpdateFacing(bool facingRight)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX =
                spriteFacesRight
                ? !facingRight
                : facingRight;
        }
        else
        {
            Vector3 scale =
                transform.localScale;

            float sign =
                spriteFacesRight ? 1f : -1f;

            scale.x =
                (facingRight
                    ? Mathf.Abs(scale.x)
                    : -Mathf.Abs(scale.x))
                * sign;

            transform.localScale =
                scale;
        }
    }

    //=================================
    // GIZMOS
    //=================================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );

        if (stompPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(
                stompPoint.position,
                0.15f
            );
        }
    }
}