using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FireBall : MonoBehaviour
{
    [SerializeField] private int damage = 2; // Sát thương quả cầu lửa
    [SerializeField] private float lifetime = 4f; // Tự hủy sau 4 giây nếu rơi ra ngoài bản đồ

    private void Start()
    {
        // Tự động hủy để tránh rác bộ nhớ
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu chạm trúng Player
        if (collision.CompareTag("Player"))
        {
            collision.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject); // Chạm trúng thì nổ, biến mất
        }
        // Nếu chạm trúng đất (Ground)
        else if (((1 << collision.gameObject.layer) & LayerMask.GetMask("Ground")) != 0)
        {
            // Tại đây bạn có thể tạo thêm hiệu ứng nổ nếu muốn
            Destroy(gameObject); 
        }
    }
}