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
        // Ban đầu cửa bị khóa
        sr.enabled = false;
        col.enabled = false;
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