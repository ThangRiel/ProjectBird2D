using UnityEngine;
using System.Linq;
using System;

public class PlayerCollision : MonoBehaviour
{
    private GameManager gameManager;
    //[SerializeField] private Tag[] tags;
    private OpenChest openChest;

    [Obsolete]
    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        openChest = FindAnyObjectByType<OpenChest>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //bool isTargeted = tags.Any(t => collision.CompareTag(t.tagName));
        if (collision.CompareTag("Chest"))
        {
            openChest.touchChest();
            Debug.Log("Touch chest");
        }
        else if (collision.CompareTag("Bullet"))
        {
            Debug.Log("Hit by bullet");
        }
        else if (collision.CompareTag("Obstacle") || collision.CompareTag("SlowZone") || collision.CompareTag("Tornado") || collision.CompareTag("Door"))
        {
            // Đã có ObstacleHandler xử lý riêng, bỏ qua ở đây
            return;
        }else if (collision.CompareTag("Switch"))
        {
            gameManager.SwitchController();
            Debug.Log("Switch controller");
        }
        else if (gameManager.tag.Contains(collision.tag))
        {
            Tag tag = gameManager.tags.FirstOrDefault(t => collision.CompareTag(t.tagName));
            Debug.Log("Collided with " + tag.tagName + ", score: " + tag.scoreValue);
            Destroy(collision.gameObject);
            gameManager.addScoreByTag(tag.tagName);
        }
        else if (collision.CompareTag("Trap") || collision.CompareTag("Enemy"))
        {
            gameManager.GameOver();
        }
        else
        {
            Debug.Log("Collided with unhandled object: " + collision.gameObject.name);
        }
        //if (isTargeted)
        {
            //Tag matchedTag = tags.First(t => collision.CompareTag(t.tagName));
            Tag tag = gameManager.tags.FirstOrDefault(t => collision.CompareTag(t.tagName));
            Debug.Log("Collided with " + tag.tagName + ", score: " + tag.scoreValue);
            Destroy(collision.gameObject);
            gameManager.addScoreByTag(tag.tagName);

        }

        //else if (collision.CompareTag("Trap") || collision.CompareTag("Enemy"))
        //{
        //    gameManager.GameOver();
        //}

    }
}
