using UnityEngine;
//Dùng Mathf.PingPong
public class BossMovement : MonoBehaviour
{
    public float amplitude = 2f;
    public float speed = 1f;

    private float startY;

    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        Vector3 pos = transform.position;

        pos.y = startY + Mathf.PingPong(Time.time * speed, amplitude);

        transform.position = pos;
    }
}