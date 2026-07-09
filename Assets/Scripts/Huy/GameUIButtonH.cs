using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class GameUIButtonH : MonoBehaviour
{
    public enum ActionType
    {
        Retry,
        Menu
    }

    [SerializeField] private ActionType action;
    [SerializeField] private GameUIH gameUI;

    void Awake()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(HandleClick);
    }

    void HandleClick()
    {
        if (gameUI == null)
            gameUI = FindAnyObjectByType<GameUIH>();

        if (gameUI == null)
            return;

        if (action == ActionType.Menu)
            gameUI.BackToMenu();
        else
            gameUI.Retry();
    }
}
