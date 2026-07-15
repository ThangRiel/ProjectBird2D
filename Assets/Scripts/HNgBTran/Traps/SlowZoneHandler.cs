using System.Collections;
using UnityEngine;

public class SlowZoneHandler : MonoBehaviour
{
    [SerializeField] private TrapConfigSO config; // asset Trap_SlowIce
    [SerializeField] private Color frozenTint = new Color(0.5f, 0.8f, 1f, 1f);

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
        if (!other.CompareTag("SlowZone")) return;
        ApplySlow(config.slowMultiplier, config.slowDuration);
    }

    public void ApplySlow(float multiplier, float duration)
    {
        if (activeSlowRoutine != null)
            StopCoroutine(activeSlowRoutine);
        activeSlowRoutine = StartCoroutine(SlowRoutine(multiplier, duration));
    }

    private IEnumerator SlowRoutine(float multiplier, float duration)
    {
        runAndFly.speedMultiplier = multiplier;
        if (sr != null) sr.color = frozenTint;

        yield return new WaitForSeconds(duration);

        runAndFly.speedMultiplier = 1f;
        if (sr != null) sr.color = originalColor;
        activeSlowRoutine = null;
    }
}