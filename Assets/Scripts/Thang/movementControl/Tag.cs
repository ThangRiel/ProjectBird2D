using UnityEngine;

[System.Serializable] // Bắt buộc phải có cái này để hiển thị được ngoài Inspector
public class Tag
{
    public string tagName;    // Tên của loại tag (Ví dụ: "Coin", "Diamond", "Poison")
    public int scoreValue;    // Số điểm cộng (hoặc trừ) tương ứng
}