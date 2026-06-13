using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;

    public Image[] hearts; // Mảng chứa các UI Image trái tim
    public Sprite fullHeart;  // Kéo hình trái tim đầy vào đây
    public Sprite bigerHeart;
    public Sprite emptyHeart; // Kéo hình trái tim rỗng vào đây

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHeartsUI();
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

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

    void GameOver()
    {
        Debug.Log("Hết máu! Thua cuộc.");
    }
}