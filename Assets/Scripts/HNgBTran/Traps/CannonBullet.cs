using UnityEngine;

public class CannonBullet : MonoBehaviour
{
    public float speed = 8f;
    public float lifeTime = 5f; // tự huỷ sau X giây nếu bay ra ngoài mà không trúng gì

    Vector2 direction = Vector2.left;

    public void SetDirection(Vector2 dir)
    {
        direction = dir;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Trúng Player: máu/knockback đã do ObstacleHandler tự xử lý (tag "Obstacle")
        if (other.GetComponent<PlayerCollision>() != null)
        {
            Destroy(gameObject);
        }
    }
}