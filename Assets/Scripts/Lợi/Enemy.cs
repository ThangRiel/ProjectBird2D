using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Tuần tra")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private bool spriteFacesRight = false;

    [Header("Tấn công")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float damageToPlayer = 10f;

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;

    private float leftLimit;
    private float rightLimit;
    private bool movingRight = true;

    private float nextAttackTime;
    private bool isAttacking = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Giới hạn tuần tra
        leftLimit = transform.position.x - patrolDistance;
        rightLimit = transform.position.x + patrolDistance;

        // Tìm Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null)
        {
            Patrol();
            return;
        }

        float distanceToPlayer = Vector2.Distance(
            transform.position,
            playerTransform.position
        );

        // Player trong tầm đánh
        if (distanceToPlayer <= attackRange)
        {
            Attack();
        }
        else
        {
            // Player ra ngoài tầm -> quay lại tuần tra
            isAttacking = false;
            Patrol();
        }
    }

    void Patrol()
    {
        if (isAttacking) return;

        if (anim != null)
            anim.SetBool("isRunning", true);

        Vector3 pos = transform.position;

        if (movingRight)
        {
            pos.x += moveSpeed * Time.deltaTime;
            if (pos.x >= rightLimit)
                movingRight = false;
        }
        else
        {
            pos.x -= moveSpeed * Time.deltaTime;
            if (pos.x <= leftLimit)
                movingRight = true;
        }

        transform.position = pos;
        UpdateFacing(movingRight);
    }

    void Attack()
    {
        isAttacking = true;

        if (anim != null)
            anim.SetBool("isRunning", false);

        // Quay mặt về phía Player
        bool playerIsRight = playerTransform.position.x > transform.position.x;
        UpdateFacing(playerIsRight);

        // Đánh theo cooldown
        if (Time.time >= nextAttackTime)
        {
            if (anim != null)
                anim.SetTrigger("Attack");

            Debug.Log("Enemy đánh Player, gây " + damageToPlayer + " sát thương.");

            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void UpdateFacing(bool isFacingRight)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = spriteFacesRight
                ? !isFacingRight
                : isFacingRight;
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.x = isFacingRight
                ? Mathf.Abs(scale.x)
                : -Mathf.Abs(scale.x);

            if (!spriteFacesRight)
                scale.x *= -1;

            transform.localScale = scale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}