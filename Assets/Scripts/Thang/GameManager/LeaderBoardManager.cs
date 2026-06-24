using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using UnityEngine;
using System.Threading.Tasks;
using System;
using TMPro;

public class LeaderboardManager : MonoBehaviour
{
    private bool isReady = false;
    public string leaderboardId = "HighscoreBoard"; //! Nhớ check lại đúng ID nha
    public TMP_InputField inputScore;
    public TMP_InputField inputName;
    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            isReady = true;
            Debug.Log("Đăng nhập UGS thành công!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khởi tạo: " + e.Message);
        }
    }
    public void Run()
    {
        string playername = inputName.text;
        // Ép kiểu chữ từ InputField thành số nguyên (int)
        if (int.TryParse(inputScore.text, out int score))
        {
            DatTenVaGuiDiem(playername, score);
        }
        else
        {
            Debug.LogWarning("Mày nhập điểm bị sai, phải nhập số nguyên!");
        }
        
    }
    public async void DatTenVaGuiDiem(string tenNguoiChoi, int diemSo)
    {
        if (!isReady)
        {
            Debug.LogWarning("UGS chưa sẵn sàng!");
            return;
        }

        try
        {
            if (string.IsNullOrWhiteSpace(tenNguoiChoi))
            {
                tenNguoiChoi = "No Name";
            }

            await AuthenticationService.Instance.UpdatePlayerNameAsync(tenNguoiChoi);
            Debug.Log("Đã set tên thành: " + tenNguoiChoi);

            // 2. Gửi điểm lên Leaderboard

            var response = await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, diemSo);
            Debug.Log($"Đã gửi điểm! Tên: {tenNguoiChoi} - Điểm: {response.Score}");
            Debug.Log(diemSo);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khi set tên hoặc gửi điểm: " + e.Message);
        }
    }
}