using UnityEngine;

public class EnemyFly : MonoBehaviour
{
    public float moveSpeed = 3f;

    Rigidbody2D rb;

    void Awake()
    {
        rb =
            GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        rb.linearVelocity =
            new Vector2(
                -moveSpeed,
                rb.linearVelocity.y
            );
    }
}