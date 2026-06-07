using UnityEngine;
using System.Collections;

public class OpenChest : MonoBehaviour
{
    private Animator animator;
    private bool isOpen = false;
    public GameObject coinPrefab; // Gán Prefab đồng xu vào đây
    public int numberOfCoins = 10; // Số lượng đồng xu bắn ra
    public float forceMagnitude = 5f;

    public void Awake()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("isOpen", false);
    }

    public void touchChest()
    {
        if (!isOpen)
        {
            animator.SetBool("isOpen", true);
            isOpen = true;
            SpawnCoins();
            Debug.Log("Chest opened");
        }
    }

    private void SpawnCoins()
    {
        for (int i = 0; i < numberOfCoins; i++)
        {
            // 1. Tạo một vị trí ngẫu nhiên xung quanh rương
            Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f),Random.Range(-7f, 7f)).normalized;

            // 2. Tạo đồng xu tại vị trí của rương
            GameObject coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);

            // 3. Tác dụng lực lên đồng xu
            Rigidbody coinRigidbody = coin.GetComponent<Rigidbody>();
            if (coinRigidbody != null)
            {
                coinRigidbody.AddForce(randomDirection * forceMagnitude, ForceMode.Impulse);
            }
            StartAttractDelay();
        }
    }
    private IEnumerator StartAttractDelay()
    {
        yield return new WaitForSeconds(10f);
       
    }
}
