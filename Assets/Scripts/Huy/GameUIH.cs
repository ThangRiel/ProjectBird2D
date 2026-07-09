using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIH : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private Text legacyScoreText;

    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winningPanel;
    [SerializeField] private GameObject pausePanel;

    void Awake()
    {
        CacheScenePanels();
        HideAllPanels();
    }

    public void SetScore(int score)
    {
        if (legacyScoreText != null)
            legacyScoreText.text = score.ToString();
    }

    public void HideAllPanels()
    {
        SetPanelActive(gameOverPanel, false);
        SetPanelActive(winningPanel, false);
        SetPanelActive(pausePanel, false);
    }

    public void ShowGameOver()
    {
        HideAllPanels();
        SetPanelActive(gameOverPanel, true);
    }

    public void ShowWin()
    {
        HideAllPanels();
        SetPanelActive(winningPanel, true);
    }

    public void Retry()
    {
        GameManager manager = FindAnyObjectByType<GameManager>();
        if (manager != null)
            manager.RestartGame();
    }

    public void BackToMenu()
    {
        GameManager manager = FindAnyObjectByType<GameManager>();
        if (manager != null)
            manager.BackToMenu();
    }

    void CacheScenePanels()
    {
        if (gameOverPanel == null)
            gameOverPanel = FindPanelObject("GameOverPanel");

        if (winningPanel == null)
            winningPanel = FindPanelObject("WinningPanel");

        if (pausePanel == null)
            pausePanel = FindPanelObject("PausePanel");
    }

    GameObject FindPanelObject(string objectName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == objectName)
                return child.gameObject;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] roots = activeScene.GetRootGameObjects();

        foreach (GameObject root in roots)
        {
            Transform[] sceneChildren = root.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in sceneChildren)
            {
                if (child.name == objectName)
                    return child.gameObject;
            }
        }

        return null;
    }

    void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }
}
