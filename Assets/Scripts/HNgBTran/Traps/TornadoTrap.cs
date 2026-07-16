using UnityEngine;

public class TornadoTrap : MonoBehaviour
{
    [Header("Swirl Force")]
    public float spinForce = 8f;    // lực xoáy vòng quanh tâm
    public float pullForce = 2f;    // lực hút nhẹ vào tâm
    public float turbulence = 4f;   // lực ngẫu nhiên, tạo cảm giác hỗn loạn "lung tung"

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.GetComponent<PlayerCollision>() == null) return;

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        Vector2 toCenter = (Vector2)transform.position - rb.position;
        Vector2 tangent = new Vector2(-toCenter.y, toCenter.x).normalized;

        Vector2 force = tangent * spinForce + toCenter.normalized * pullForce + Random.insideUnitCircle * turbulence;

        // Giống ObstacleHandler: zero rồi Impulse để không bị movement script đè mất,
        // chỉ khác là lặp lại mỗi frame còn overlap -> tạo hiệu ứng liên tục
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);
    }
}