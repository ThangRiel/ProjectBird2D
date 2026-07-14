using System.Collections;
using UnityEngine;

public class SlowZoneHandler : MonoBehaviour
{
    [SerializeField] private TrapConfigSO config; // kéo asset Trap_SlowIce (TrapConfigSO) vào đây
    [SerializeField] private Color frozenTint = new Color(0.5f, 0.8f, 1f, 1f); // màu xanh dương khi bị đóng băng

    private RunAndFly runAndFly;
    private SpriteRenderer sr;
    private Color originalColor;
    private Coroutine activeSlowRoutine;

    private void Awake()
    {
        runAndFly = GetComponent<RunAndFly>();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[SlowZoneHandler] OnTriggerEnter2D với tag: {other.tag}");
        if (!other.CompareTag("SlowZone")) return;

        Debug.Log("[SlowZoneHandler] Chạm SlowZone, bắt đầu làm chậm");

        if (activeSlowRoutine != null)
            StopCoroutine(activeSlowRoutine);

        activeSlowRoutine = StartCoroutine(ApplySlow());
    }

    private IEnumerator ApplySlow()
    {
        runAndFly.speedMultiplier = config.slowMultiplier;
        if (sr != null) sr.color = frozenTint;
        Debug.Log($"[SlowZoneHandler] Áp speedMultiplier = {config.slowMultiplier}, trong {config.slowDuration}s");

        yield return new WaitForSeconds(config.slowDuration);

        runAndFly.speedMultiplier = 1f;
        if (sr != null) sr.color = originalColor;
        Debug.Log("[SlowZoneHandler] Hết hiệu ứng, trả lại tốc độ bình thường");
        activeSlowRoutine = null;
    }
}