using UnityEngine;

public class ScoreHolder : MonoBehaviour
{
    public int score = 0;
    public static ScoreHolder Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Có thằng khác tương tự rồi thì tự hủy
        }
    }
}
