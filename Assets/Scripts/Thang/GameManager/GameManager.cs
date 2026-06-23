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
        SceneManager.LoadScene("Game");
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
            gameOverUI.SetActive(true); // ! Đã bỏ comment
            Time.timeScale = 0f;
        }
    }
    public void RestartGame()
    {
        isGameOver = false;
        // UpdateScoreText();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }
    public bool IsGameOver()
    {
        return isGameOver;
    }
}
