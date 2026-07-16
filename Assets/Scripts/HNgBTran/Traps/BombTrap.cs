using UnityEngine;

public class BombTrap : MonoBehaviour
{
    [Header("Explosion")]
    public GameObject explosionEffect;   // kéo GameObject con "ExplosionEffect" vào đây
    public float destroyDelay = 1f;      // thời gian chờ trước khi bom biến mất, khớp độ dài animation nổ

    Animator animator;
    Collider2D col;
    bool hasExploded = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded) return;
        if (!other.CompareTag("Player")) return;

        Explode();
    }

    void Explode()
    {
        hasExploded = true;

        animator.SetTrigger("Explode");
        col.enabled = false; // tránh bị trigger nổ lần 2 hoặc trừ máu lặp lại

        if (explosionEffect != null)
            explosionEffect.SetActive(true); // bật effect sprite nổ có sẵn

        ScreenEffects.Instance?.PlayExplosionEffect(); // rung màn + chớp sáng, xem Bước 4

        Destroy(gameObject, destroyDelay);
    }
}