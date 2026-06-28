using UnityEngine;

public class SkillItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Phát hiện nhân vật chính va chạm
        if (collision.CompareTag("Player"))
        {
            LoiPlayer player = collision.GetComponent<LoiPlayer>();

            if (player != null)
            {
                player.UnlockSkill(); // Gọi hàm mở khóa skill vĩnh viễn trong LoiPlayer
                Destroy(gameObject);  // Tự hủy vật phẩm trên bản đồ sau khi nhặt
            }
        }
    }
}