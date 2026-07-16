using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenEffects : MonoBehaviour
{
    public static ScreenEffects Instance;

    [Header("Camera Shake")]
    public Transform cameraTransform;   //  Main Camera vào
    public float shakeDuration = 0.3f;
    public float shakeMagnitude = 0.3f;

    [Header("White Flash")]
    public Image flashImage;            //  Image full màn hình vào
    public float flashDuration = 0.6f;  //  gian mờ dần từ trắng về trong suốt

    Coroutine shakeRoutine;
    Coroutine flashRoutine;

    void Awake()
    {
        Instance = this;

        if (flashImage != null)
    {
        Color c = flashImage.color;
        c.a = 0f;
        flashImage.color = c;
    }

    }

    public void PlayExplosionEffect()
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeCamera());

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashScreen());
    }

    IEnumerator ShakeCamera()
    {
        Vector3 originalPos = cameraTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            cameraTransform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraTransform.localPosition = originalPos;
    }

    IEnumerator FlashScreen()
    {
        Color c = flashImage.color;
        c.a = 1f; // chói trắng ngay lập tức
        flashImage.color = c;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / flashDuration);
            flashImage.color = c;
            yield return null;
        }

        c.a = 0f;
        flashImage.color = c;
    }
}