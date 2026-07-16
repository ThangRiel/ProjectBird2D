using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.Assertions.Must;
using TMPro;
using System;
public class GameManager : MonoBehaviour
{
    private int score = ScoreHolder.Instance != null ? ScoreHolder.Instance.score : 0;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject camera;
    [SerializeField] private float switchControllerAt = 0;
    private int scoreTickCounter = 0;
    [SerializeField] private Text scoreText;
    [SerializeField] private TMP_Text[] scoreTextTMP;
    [SerializeField] private GameUIH gameUI;
    [SerializeField] private GameObject gameOverUI; // ! Đã bỏ comment
    [SerializeField] private GameObject winningUI;
    [SerializeField] public string gameSceneName = "Game1";
    [SerializeField] public string MenuName = "UI";
    private bool isGameOver = false;
    private bool isGameWon = false;
    public bool stopScoreTick = false;
    [SerializeField] public Tag[] tags;
    public static Action OnAnyBossDied;
    private bool isSwitchingController = false;

    void Start()
    {
        if (gameUI == null)
        {
            gameUI = FindAnyObjectByType<GameUIH>(FindObjectsInactive.Include);
        }

        UpdateScoreText();
        if (gameUI != null)
        {
            gameUI.HideAllPanels();
        }
        else
        {
            if (gameOverUI != null) gameOverUI.SetActive(false); // ! Đã bỏ comment
            if (winningUI != null) winningUI.SetActive(false);
        }
        gameOverUI?.SetActive(false);
        winningUI?.SetActive(false);
    }

    void FixedUpdate()
    {
        if (!isGameOver && !isGameWon)
        {
            scoreTickCounter++;
            if (scoreTickCounter >= 10)
            {
                scoreTickCounter = 0;
                if (!stopScoreTick)
                {
                    ScoreHolder.Instance.score += 1;
                    score = ScoreHolder.Instance.score;
                    UpdateScoreText();
                }
            }
        }

        if (player != null && camera != null && switchControllerAt != 0f && !isSwitchingController)
        {
            if (player.transform.position.x >= switchControllerAt)
            {
                SwitchController();
            }
        }
    }
    //* thêm hàm để mở scene từ game StartUi
    public void StartGame()
    {
        Debug.Log("game started");
        ResetScore();
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);

    }
    public void BackToMenu()
    {
        Debug.Log("back to menu");
        Time.timeScale = 1f;
        SceneManager.LoadScene(MenuName);
    }

    public void addScoreByTag(string itemTag)
    {
        Debug.Log(itemTag);
        int scoreValue = 0;
        foreach (var tag in tags)
        {
            if (tag.tagName == itemTag)
            {
                scoreValue = tag.scoreValue;
                break;
            }
        }
        ScoreHolder.Instance.score += scoreValue;
        score = ScoreHolder.Instance.score;
        UpdateScoreText();
        return;
    }
    public void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
        if (scoreTextTMP != null)
        {
            foreach (var text in scoreTextTMP)
            {
                text.text = score.ToString();
            }
        }
        if (gameUI != null)
        {
            gameUI.SetScore(score);
        }
    }
    public void GameOver()
    {
        if (!isGameOver && !isGameWon)
        {
            isGameOver = true;
            Time.timeScale = 0f;
            if (gameUI != null)
            {
                gameUI.ShowGameOver();
            }
            else if (gameOverUI != null)
            {
                gameOverUI.SetActive(true);
            }
            Debug.LogError("Game Over! Mày đã thua!");
        }
    }
    public void WinGame()
    {
        if (!isGameOver && !isGameWon)
        {
            isGameWon = true;
            Time.timeScale = 0f;
            if (gameUI != null)
            {
                gameUI.ShowWin();
            }
            else if (winningUI != null)
            {
                winningUI.SetActive(true);
            }
            Debug.Log("Chúc mừng! Mày đã thắng!");

        }
    }
    public void RestartGame()
    {
        isGameOver = false;
        isGameWon = false;
        // UpdateScoreText();
        Time.timeScale = 1f;
        ResetScore();
        Debug.Log("Restarting game...");
        SceneManager.LoadScene(gameSceneName);
    }
    public void ResetScore() //! không dùng nếu đã dùng restartGame
    {
        ScoreHolder.Instance.score = 0;
        score = 0;
        Debug.Log("Score reset to 0.");
    }
    public bool IsGameOver()
    {
        return isGameOver;
    }
    public bool IsGameWon()
    {
        return isGameWon;
    }

    public void StopScoreTick(bool value)
    {
        stopScoreTick = value;
    }

    public void SwitchController()
    {
        if (player != null)
        {
            RunAndFly runAndFly = player.GetComponent<RunAndFly>();
            LoiPlayer loiPlayer = player.GetComponent<LoiPlayer>();
            if (runAndFly != null && loiPlayer != null)
            {
                runAndFly.enabled = !runAndFly.enabled;
                loiPlayer.enabled = !loiPlayer.enabled;
            }
            isSwitchingController = true;
        }
        if (camera != null)
        {
            CameraMoveIndependent cameraMoveIndependent = camera.GetComponent<CameraMoveIndependent>();
            CameraFollow cameraFollow = camera.GetComponent<CameraFollow>();
            if (cameraMoveIndependent != null && cameraFollow != null)
            {
                cameraFollow.enabled = !cameraFollow.enabled;
                cameraMoveIndependent.enabled = !cameraMoveIndependent.enabled;
            }
            isSwitchingController = true;
        }
        stopScoreTick = true;
    }
}
