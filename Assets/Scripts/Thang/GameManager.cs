using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    private int score = 0;
    [SerializeField] private Text scoreText;
    //[SerializeField] private GameObject gameOverUI;
    private bool isGameOver = false;
    [SerializeField] private Tag[] tags;


    void Start()
    {
        UpdateScoreText();
        //gameOverUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
    //public void AddScore(int point)
    //{
    //    if (!isGameOver)
    //    {
    //        score += point;
    //        UpdateScoreText();
    //    }

    //}

    public void addScoreByTag(string itemTag, int scoreValue)
    {
        print(itemTag);
        score += scoreValue;
        UpdateScoreText();
        return;
    }
    public void UpdateScoreText()
    {
        scoreText.text = score.ToString();
    }
    public void GameOver()
    {
        if (!isGameOver)
        {
            isGameOver = true;
            //gameOverUI.SetActive(true);
            Time.timeScale = 0f;
        }
    }
    public void RestartGame()
    {
        isGameOver = false;
        UpdateScoreText();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }
    public bool IsGameOver()
    {
        return isGameOver;
    }
}
