using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    private Vector2 moveDirection;
    private Rigidbody2D rb;
    private bool isInitialized = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 direction)
    {
        moveDirection = direction.normalized;
        isInitialized = true;
        
        // Đẩy lực trực tiếp theo vector hướng tuyệt đối, bất chấp góc xoay của Sprite
        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }

        Destroy(gameObject, 3f); 
    }

    void FixedUpdate()
    {
        // Đảm bảo vận tốc không bị thay đổi bởi va chạm nhỏ
        if (isInitialized && rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") || collision.CompareTag("Boss")) return;

        if (collision.CompareTag("Player"))
        {
            collision.gameObject.SendMessage("TakeDamage", 1, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}