using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private bool spriteFacesRight = false;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float damageToPlayer = 10f;

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;

    // Giới hạn tuần tra (theo localPosition)
    private float leftLimit;
    private float rightLimit;
    private bool movingRight = true;

    private float nextAttackTime;
    private bool isAttacking = false;

    void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Lưu vị trí tuần tra ban đầu
        leftLimit = transform.localPosition.x - patrolDistance;
        rightLimit = transform.localPosition.x + patrolDistance;

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

        // Tính khoảng cách thật giữa Enemy và Player
        float distanceToPlayer = Vector2.Distance(
            transform.position,
            playerTransform.position
        );

        if (distanceToPlayer <= attackRange)
        {
            Attack();
        }
        else
        {
            isAttacking = false;
            Patrol();
        }
    }

    private void Patrol()
    {
        if (isAttacking)
            return;

        if (anim != null)
            anim.SetBool("isRunning", true);

        Vector3 localPos = transform.localPosition;

        if (movingRight)
        {
            localPos.x += moveSpeed * Time.deltaTime;

            if (localPos.x >= rightLimit)
            {
                localPos.x = rightLimit;
                movingRight = false;
            }
        }
        else
        {
            localPos.x -= moveSpeed * Time.deltaTime;

            if (localPos.x <= leftLimit)
            {
                localPos.x = leftLimit;
                movingRight = true;
            }
        }

        transform.localPosition = localPos;

        UpdateFacing(movingRight);
    }

    private void Attack()
    {
        isAttacking = true;

        if (anim != null)
            anim.SetBool("isRunning", false);

        // Quay mặt về phía Player
        bool playerIsRight =
            playerTransform.position.x > transform.position.x;

        UpdateFacing(playerIsRight);

        // Đánh theo cooldown
        if (Time.time >= nextAttackTime)
        {
            if (anim != null)
            {
                anim.SetTrigger("Attack");
            }

            Debug.Log(
                "Enemy đánh Player, gây " +
                damageToPlayer +
                " sát thương."
            );

            // Sau này có thể gọi hàm trừ máu Player ở đây
            // playerTransform.GetComponent<PlayerHealth>()?.TakeDamage(damageToPlayer);

            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void UpdateFacing(bool facingRight)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = spriteFacesRight
                ? !facingRight
                : facingRight;
        }
        else
        {
            Vector3 scale = transform.localScale;
            float sign = spriteFacesRight ? 1f : -1f;

            scale.x = (facingRight
                ? Mathf.Abs(scale.x)
                : -Mathf.Abs(scale.x)) * sign;

            transform.localScale = scale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );
    }
}