using UnityEngine;
using System.Collections;

public class WarningLine : MonoBehaviour
{
    public float lifeTime = 1f;
    public float blinkSpeed = 0.1f;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        StartCoroutine(Blink());

        Destroy(gameObject, lifeTime);
    }

    IEnumerator Blink()
    {
        while (true)
        {
            sr.enabled = !sr.enabled;

            yield return new WaitForSeconds(blinkSpeed);
        }
    }
}