using UnityEngine;

public class ObstacleHandler : MonoBehaviour
{
    [Header("Effects")]
    public float knockbackForce = 5f;      // lực bật ngược khi chạm gai
    public float invincibleDuration = 1f;  // thời gian bất tử sau khi chạm (giây)

    [Header("Debug")]
    public bool logHits = true;

    // ── Internal ──────────────────────────────────────────────
    private bool isInvincible = false;
    private float invincibleTimer = 0f;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    // ── Lifecycle ──────────────────────────────────────────────
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Đếm thời gian bất tử
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;

            // Nhấp nháy khi bất tử
            sr.enabled = Mathf.Sin(invincibleTimer * 20f) > 0;

            if (invincibleTimer <= 0f)
            {
                isInvincible = false;
                sr.enabled = true;
            }
        }
    }

    // ── Trigger ────────────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Obstacle")) return;
        if (isInvincible) return;

        if (logHits)
            Debug.Log("[ObstacleHandler] Chạm gai!");

        HitByObstacle();
    }

    void HitByObstacle()
    {
        // Knockback
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(-knockbackForce, knockbackForce * 0.8f), ForceMode2D.Impulse);
        }

        // Trừ máu
        HealthManager health = GetComponent<HealthManager>();
        if (health != null)
            health.TakeDamage(1);
        
        // Bất tử tạm thời
        isInvincible = true;
        invincibleTimer = invincibleDuration;
    }

    void GameOver()
    {
        Debug.Log("Hết máu! Thua cuộc.");
        // Dừng game tạm thời
        Time.timeScale = 0f;
    }

}