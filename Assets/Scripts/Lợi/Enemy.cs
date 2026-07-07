using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 4f;

    [Header("Detection")]
    [SerializeField] private float detectRange = 6f;
    [SerializeField] private float attackRange = 1.2f;

    [Header("Attack")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackDuration = 0.6f;

    [Header("Sprite")]
    [SerializeField] private bool spriteFacesRight = false;

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Transform player;

    private int currentHealth;

    private float leftLimit;
    private float rightLimit;
    private bool movingRight = true;

    private bool isDead;
    private bool isAttacking;
    private float nextAttackTime;

    private enum State
    {
        Patrol,
        Chase,
        Attack
    }

    private State state;

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

        GameObject obj = GameObject.FindGameObjectWithTag("Player");

        if (obj != null)
            player = obj.transform;
    }

    private void Update()
    {
        if (isDead)
            return;

        if (isAttacking)
            return;

        if (player == null)
        {
            Patrol();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
            state = State.Attack;
        else if (distance <= detectRange)
            state = State.Chase;
        else
            state = State.Patrol;

        switch (state)
        {
            case State.Patrol:
                Patrol();
                break;

            case State.Chase:
                ChasePlayer();
                break;

            case State.Attack:
                AttackPlayer();
                break;
        }
    }

    //========================
    // Patrol
    //========================
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

    //========================
    // Chase
    //========================
    private void ChasePlayer()
    {
        anim.SetBool("isRunning", true);

        bool faceRight = player.position.x > transform.position.x;

        UpdateFacing(faceRight);

        Vector3 target = player.position;
        target.y = transform.position.y;

        transform.position = Vector2.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime);
    }

    //========================
    // Attack
    //========================
    private void AttackPlayer()
    {
        if (Time.time < nextAttackTime)
            return;

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        anim.SetBool("isRunning", false);

        bool faceRight = player.position.x > transform.position.x;
        UpdateFacing(faceRight);

        rb.linearVelocity = Vector2.zero;

        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(attackDuration * 0.5f);

        if (player != null)
        {
            float distance = Vector2.Distance(
                transform.position,
                player.position);

            if (distance <= attackRange + 0.2f)
            {
                player.SendMessage(
                    "TakeDamage",
                    damage,
                    SendMessageOptions.DontRequireReceiver);
            }
        }

        yield return new WaitForSeconds(attackDuration * 0.5f);

        nextAttackTime = Time.time + attackCooldown;

        isAttacking = false;
    }

    //========================
    // Take Damage
    //========================
    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (currentHealth > 0)
        {
            anim.SetTrigger("Hit");
        }
        else
        {
            Die();
        }
    }

    //========================
    // Die
    //========================
    private void Die()
    {
        isDead = true;

        StopAllCoroutines();

        anim.SetBool("isRunning", false);
        anim.SetTrigger("Die");

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = false;

        Destroy(gameObject, 1f);
    }

    //========================
    // Flip
    //========================
    private void UpdateFacing(bool facingRight)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX =
                spriteFacesRight
                ? !facingRight
                : facingRight;
        }
    }

    //========================
    // Gizmos
    //========================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(
            new Vector3(transform.position.x - patrolDistance, transform.position.y),
            new Vector3(transform.position.x + patrolDistance, transform.position.y));
    }
}