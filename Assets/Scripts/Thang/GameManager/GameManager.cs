using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.Assertions.Must;
public class GameManager : MonoBehaviour
{
    private int score = 0;
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject gameOverUI; // ! Đã bỏ comment
    [SerializeField] public string gameSceneName = "Game1";
    [SerializeField] public string MenuName = "UI";
    private bool isGameOver = false;
    [SerializeField] public Tag[] tags;


    void Start()
    {
        UpdateScoreText();
        gameOverUI.SetActive(false); // ! Đã bỏ comment
    }

    void Update()
    {

    }
    //* thêm hàm để mở scene từ game StartUi
    public void StartGame()
    {
        Debug.Log("game started");
        SceneManager.LoadScene(gameSceneName);
        Time.timeScale = 1f;
    }
    public void BackToMenu()
    {
        Debug.Log("back to menu");
        SceneManager.LoadScene(MenuName);
        Time.timeScale = 1f;
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
        score += scoreValue;
        UpdateScoreText();
        return;
    }
    public void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }

    }
    public void GameOver()
    {
        if (!isGameOver)
        {
            isGameOver = true;
            gameOverUI.SetActive(true);
            Time.timeScale = 0f;
        }
    }
    public void RestartGame()
    {
        isGameOver = false;
        // UpdateScoreText();
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }
    public bool IsGameOver()
    {
        return isGameOver;
    }
}
