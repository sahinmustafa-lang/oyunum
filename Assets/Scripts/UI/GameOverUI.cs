using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    void Start()
    {
        MatchData match = GameManager.Instance.CurrentMatch;
        var scoreText = GameObject.Find("ScoreText")?.GetComponent<Text>();
        if (match != null && scoreText != null)
            scoreText.text = $"Score: {match.teamAScore} - {match.teamBScore}";

        BindButton("RetryButton",    OnRetryPressed);
        BindButton("MainMenuButton", OnMainMenuPressed);
    }

    void BindButton(string buttonName, UnityEngine.Events.UnityAction action)
    {
        var go = GameObject.Find(buttonName);
        if (go != null)
        {
            var btn = go.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(action);
        }
    }

    void OnRetryPressed()
    {
        AudioManager.Instance.PlayButtonClick();
        GameManager.Instance.ResetGame();
        SceneLoader.Instance.LoadMainMenu();
    }

    void OnMainMenuPressed()
    {
        AudioManager.Instance.PlayButtonClick();
        GameManager.Instance.ResetGame();
        SceneLoader.Instance.LoadMainMenu();
    }
}
