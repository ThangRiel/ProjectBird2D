using UnityEngine;
using System.Collections;

public class Coin : MonoBehaviour
{
    private Transform playerTransform; // Tham chiếu đến Transform của nhân vật
    public float attractDelay = 1.0f; // Thời gian trễ trước khi bị hút
    public float attractSpeed = 5.0f; // Tốc độ hút
    private CircleCollider2D coinCollider; // Collider của đồng xu

    private bool isAttracting = false;

    private void Start()
    {
        StartCoroutine(StartAttractDelay());
        coinCollider = GetComponent<CircleCollider2D>();
        coinCollider.isTrigger = false;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // 2. Bắt đầu Coroutine để tạo độ trễ
        
    }

    private void FixedUpdate() // FixedUpdate tốt cho vật lý
    {
        // 3. Nếu đang trong trạng thái được hút và tìm thấy nhân vật
        if (isAttracting && playerTransform != null)
        {
            // Di chuyển đồng xu về phía nhân vật
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, attractSpeed * Time.deltaTime);

            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer < 0.2f)
            {
                GameManager gameManager = FindObjectOfType<GameManager>();
                gameManager.addScoreByTag("SilverCoin");
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator StartAttractDelay()
    {

        yield return new WaitForSeconds(attractDelay);
        coinCollider.isTrigger = true;
        isAttracting = true;
    }
}