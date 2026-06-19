using UnityEngine;

public class EnemyStomp : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private float bounceForce = 8f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();

        if (playerRb == null)
            return;

        // Chỉ chết khi Player đang rơi
        if (playerRb.linearVelocity.y >= 0f)
            return;

        Debug.Log(enemy.gameObject.name + " bị dẫm!");

        playerRb.linearVelocity = new Vector2(
            playerRb.linearVelocity.x,
            bounceForce
        );

        enemy.TakeDamage(9999); // hoặc enemy.Die()
    }
}