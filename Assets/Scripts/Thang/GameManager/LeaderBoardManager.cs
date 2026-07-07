using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using UnityEngine;
using System.Threading.Tasks;
using System;
using TMPro;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    private bool isReady = false;
    public string leaderboardId = "HighscoreBoardBirt2D"; //! Nhớ check lại đúng ID nha
    public TMP_Text inputScore;
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
        if (string.IsNullOrWhiteSpace(playername))
        {
            playername = "NoName";
        }
        else
        {
            playername = SanitizePlayerName(playername);
        }

        Debug.Log("Tên người chơi: " + playername);
        if (inputScore == null || string.IsNullOrWhiteSpace(inputScore.text))
        {
            Debug.LogWarning("0");
            return;
        }
        // Ép kiểu chữ từ InputField thành số nguyên (int)
        if (int.TryParse(inputScore.text, out int score))
        {
            Debug.Log("Điểm: " + score);
            SendScore(playername, score);
        }
        else
        {
            Debug.LogWarning("Mày nhập điểm bị sai, phải nhập số nguyên!");
        }

    }

    private string SanitizePlayerName(string input) //* Chuẩn hóa tên người chơi, loại bỏ ký tự đặc biệt và khoảng trắng
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return "NoName";
        }

        string sanitized = input.Trim();
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"\s+", "");
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"[^a-zA-Z0-9]", "");

        return string.IsNullOrWhiteSpace(sanitized) ? "NoName" : sanitized;
    }

    public async void SendScore(string name, int score)
    {

        if (!isReady)
        {
            Debug.LogWarning("UGS chưa sẵn sàng!");
            return;
        }

        try
        {

            await AuthenticationService.Instance.UpdatePlayerNameAsync(name);
            Debug.Log("Đã set tên thành: " + name);

            // 2. Gửi điểm lên Leaderboard
            var response = await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
            Debug.Log($"Đã gửi điểm! Tên: {name} - Điểm: {response.Score}");
            Debug.Log(score);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khi set tên hoặc gửi điểm: " + e.Message);
        }
    }
}