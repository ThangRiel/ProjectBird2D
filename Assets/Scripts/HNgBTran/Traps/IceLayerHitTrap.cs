using UnityEngine;

public class IceLayerHitTrap : MonoBehaviour
{
    [SerializeField] private TrapConfigSO config;      // asset Trap_IceLayerHit
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Sprite[] layerSprites;    // [0] = còn nguyên -> [cuối] = gần vỡ nhất

    private int remainingHits;
    private BoxCollider2D hitboxCollider;

    private void Awake()
    {
        hitboxCollider = GetComponent<BoxCollider2D>();
        remainingHits = config.iceLayerHits;
        UpdateSprite();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.TryGetComponent<RunAndFly>(out var runAndFly)) return;

        remainingHits--;
        Debug.Log($"[IceLayerHitTrap] Player va chạm, còn lại {remainingHits} lớp");

        if (collision.gameObject.TryGetComponent<SlowZoneHandler>(out var slowHandler))
            slowHandler.ApplySlow(config.slowMultiplier, config.slowDuration);

        if (remainingHits <= 0)
            Break();
        else
            UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (layerSprites.Length == 0) return;
        int index = Mathf.Clamp(config.iceLayerHits - remainingHits, 0, layerSprites.Length - 1);
        sr.sprite = layerSprites[index];
    }

    private void Break()
    {
        Debug.Log("[IceLayerHitTrap] Vỡ hoàn toàn, mất mặt băng!");
        hitboxCollider.enabled = false; // mất nền, Player rơi tự do do trọng lực
        if (sr != null) sr.enabled = false; // ẩn sprite luôn, để lớp băng biến mất thật
    }

}