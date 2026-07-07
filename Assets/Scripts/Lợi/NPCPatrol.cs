using UnityEngine;

public class NPCPatrol : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private bool spriteFacesRight = false;

    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private float leftLimit;
    private float rightLimit;
    private bool movingRight = true;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        leftLimit = transform.position.x - patrolDistance;
        rightLimit = transform.position.x + patrolDistance;
    }

    private void Update()
    {
        Patrol();
    }

    private void Patrol()
    {
        if (anim != null)
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
            Vector3 scale = transform.localScale;

            float sign =
                spriteFacesRight ? 1f : -1f;

            scale.x =
                (facingRight
                    ? Mathf.Abs(scale.x)
                    : -Mathf.Abs(scale.x))
                * sign;

            transform.localScale = scale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(
            new Vector3(transform.position.x - patrolDistance, transform.position.y),
            new Vector3(transform.position.x + patrolDistance, transform.position.y));
    }
}