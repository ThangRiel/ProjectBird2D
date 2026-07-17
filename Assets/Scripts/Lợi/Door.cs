using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Door : MonoBehaviour
{
    [SerializeField] private string nextScene;

    private bool isLoading;

    private SpriteRenderer sr;
    private Collider2D col;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        GameManager.OnAnyBossDied += OpenDoor;
    }

    private void OnDisable()
    {
        GameManager.OnAnyBossDied -= OpenDoor;
    }

    private void Start()
    {
        bool hasBoss =
            FindFirstObjectByType<BossAI>() != null ||
            FindFirstObjectByType<BossAI2>() != null;

        if (hasBoss)
        {
            // Có boss -> khóa cửa
            sr.enabled = false;
            col.enabled = false;
        }
        else
        {
            // Không có boss -> mở cửa luôn
            sr.enabled = true;
            col.enabled = true;
        }
    }

    private void OpenDoor()
    {
        Debug.Log("Boss chết -> Mở cửa");

        sr.enabled = true;
        col.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading)
            return;

        if (!other.CompareTag("Player"))
            return;

        StartCoroutine(LoadSceneAfterSound());
    }

    IEnumerator LoadSceneAfterSound()
    {
        isLoading = true;

        GameAudioH.PlaySceneMove();

        yield return new WaitForSecondsRealtime(GameAudioH.GetSceneMoveDuration());

        SceneManager.LoadScene(nextScene);
    }
}