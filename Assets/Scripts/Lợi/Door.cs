using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Door : MonoBehaviour
{
    [SerializeField] private string nextScene;
    private bool isLoading;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading || !other.CompareTag("Player"))
            return;

        StartCoroutine(LoadSceneAfterSound());
    }

    private IEnumerator LoadSceneAfterSound()
    {
        isLoading = true;

        GameAudioH.PlaySceneMove();

        float soundDelay = GameAudioH.GetSceneMoveDuration();
        if (soundDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(soundDelay);
        }

        SceneManager.LoadScene(nextScene);
    }
}
