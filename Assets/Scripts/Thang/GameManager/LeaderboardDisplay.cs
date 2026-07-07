using Unity.Services.Leaderboards;
using UnityEngine;
using TMPro;

public class LeaderBoardDisplay : MonoBehaviour
{
    public string leaderboardId = "HighscoreBoardBirt2D"; // Thay bằng ID của mày
    public GameObject rowPrefab; // Prefab hiển thị 1 người chơi
    public Transform contentParent; // Cục Content trong Scroll View

    void Start()
    {
        gameObject.SetActive(false);
    }

    // Gắn hàm này vào một nút "Làm mới" hoặc gọi lúc mở panel Leaderboard
    public async void TaiDanhSachTop()
    {
        gameObject.SetActive(true);
        try
        {
            // Xóa rác cũ trước khi tải list mới
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }

            // Kéo top 10 đứa cao điểm nhất về
            var response = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId, new GetScoresOptions { Limit = 10 });

            foreach (var entry in response.Results)
            {
                // Đẻ ra 1 hàng mới từ Prefab
                GameObject row = Instantiate(rowPrefab, contentParent);

                // Tìm các TMP_Text trong con (Mày phải sắp xếp Hierarchy chuẩn)
                // Ví dụ: Text thứ 1 là Hạng, thứ 2 là Tên, thứ 3 là Điểm
                TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();

                if (texts.Length >= 3)
                {
                    texts[0].text = "#" + (entry.Rank + 1); // Rank bắt đầu từ 0
                    texts[1].text = string.IsNullOrWhiteSpace(entry.PlayerName) ? "No Name" : entry.PlayerName;
                    texts[2].text = entry.Score.ToString();
                }
            }

            Debug.Log("Đã tải xong bảng xếp hạng!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi kéo Leaderboard: " + e.Message);
        }

    }
}