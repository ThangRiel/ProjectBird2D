using UnityEngine;

public class CannonShooter : MonoBehaviour
{
    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform firePoint;                    // kéo child "FirePoint" vào đây
    public Vector2 fireDirection = Vector2.left;    // hướng bắn

    [Header("Timing")]
    public float fireRate = 3f; // bắn 1 viên mỗi X giây
    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= fireRate)
        {
            timer = 0f;
            SpawnBullet();
        }
    }

    void SpawnBullet()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        CannonBullet b = bullet.GetComponent<CannonBullet>();
        if (b != null)
            b.SetDirection(fireDirection.normalized);
    }
}