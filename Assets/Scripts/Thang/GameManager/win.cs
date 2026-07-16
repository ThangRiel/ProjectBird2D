using UnityEngine;

public class win : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.WinGame();
        }
    }
}
