using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damage = 1;

    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        if (
            collision.gameObject.CompareTag(
                "Player"
            )
        )
        {
            HealthManagerH hp =
                collision
                .gameObject
                .GetComponent<HealthManagerH>();

            if (hp != null)
            {
                hp.TakeDamage(
                    damage
                );
            }
        }
    }
}