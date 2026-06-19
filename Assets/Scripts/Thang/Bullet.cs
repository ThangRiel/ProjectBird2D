using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f; // Tốc độ đạn
    public float lifeTime = 2f; // Sau 2 giây tự hủy nếu không trúng gì
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Cho đạn bay thẳng theo hướng "phải" của chính nó
        rb.linearVelocity = transform.right * speed;

        // Tự hủy sau lifeTime giây
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Xử lý va chạm ở đây (ví dụ trúng kẻ địch)
        Debug.Log("Trúng: " + hitInfo.name);

        // Nếu trúng tường hoặc kẻ địch thì hủy viên đạn ngay
        if (!hitInfo.CompareTag("deco") && !hitInfo.CompareTag("Bullet"))
        {
            Destroy(gameObject);
        }
    }
}