using UnityEngine;

public class ParallaxSway : MonoBehaviour
{
    public float swayAmount = 0.3f; // biên độ đong đưa (world units)
    public float swaySpeed = 0.5f;  // tốc độ đong đưa

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float offsetX = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        transform.position = new Vector3(startPos.x + offsetX, startPos.y, startPos.z);
    }
}