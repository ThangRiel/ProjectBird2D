using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WinTriggerH : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    void Reset()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!string.IsNullOrWhiteSpace(playerTag)
            && !other.CompareTag(playerTag))
            return;

        GameManager manager =
            FindAnyObjectByType<GameManager>();

        if (manager != null)
            manager.WinGame();
    }
}
