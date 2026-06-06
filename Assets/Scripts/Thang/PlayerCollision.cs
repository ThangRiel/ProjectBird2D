using UnityEngine;
using System.Linq;

public class PlayerCollision : MonoBehaviour
{
    private GameManager gameManager;
    [SerializeField] private Tag[] tags;
    
    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool isTargeted = tags.Any(t => collision.CompareTag(t.tagName));
        if (isTargeted)
        {
            Tag matchedTag = tags.First(t => collision.CompareTag(t.tagName));
            Destroy(collision.gameObject);
            gameManager.addScoreByTag(matchedTag.tagName, matchedTag.scoreValue);
        }
        //else if (collision.CompareTag("Trap") || collision.CompareTag("Enemy"))
        //{
        //    gameManager.GameOver();
        //}
    }
}
