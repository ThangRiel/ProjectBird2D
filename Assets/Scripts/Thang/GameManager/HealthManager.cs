using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public int maxHealth = 4;
    private int currentHealth;

    public Image[] hearts; // Mảng chứa các UI Image trái tim
    public Sprite fullHeart;  // Kéo hình trái tim đầy vào đây
    public Sprite bigerHeart;
    public Sprite emptyHeart; // Kéo hình trái tim rỗng vào đây

    public bool isImmortal = false; //* bất tử để test game, game khó với cả dev =))

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHeartsUI();
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0 || isImmortal) return;

        currentHealth -= damage;
        UpdateHeartsUI();

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    void UpdateHeartsUI()
    {
        
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth)
            {
                hearts[i].sprite = fullHeart; // Hiện tim đầy
            }
            else
            {
                hearts[i].sprite = emptyHeart; // Đổi thành tim rỗng
            }
        }
    }

    // ! Đã chỉnh sửa
    void GameOver()
    {
        // 1. Dừng Bird
        RunAndFly player = GetComponent<RunAndFly>();
        if (player != null)
        {
            player.Die();
            return;
        }

        // 2. Delay 1 giây rồi hiện Game Over UI
        StartCoroutine(ShowGameOverAfterDelay(1f));
    }

    System.Collections.IEnumerator ShowGameOverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameManager gm = FindAnyObjectByType<GameManager>();
        if (gm != null) gm.GameOver();
    }

}
