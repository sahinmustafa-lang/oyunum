using UnityEngine;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    void Start()
    {
        bool won = GameManager.Instance.LastMatchWon;
        MatchData match = GameManager.Instance.CurrentMatch;

        var resultText = FindText("ResultText");
        var scoreText  = FindText("ScoreText");

        if (resultText) resultText.text = won ? "YOU WON!" : "ELIMINATED!";
        if (match != null && scoreText)
            scoreText.text = $"{match.teamAScore} - {match.teamBScore}";

        var continueBtn = GameObject.Find("ContinueButton");
        var retryBtn    = GameObject.Find("RetryButton");

        if (continueBtn) continueBtn.SetActive(won);
        if (retryBtn)    retryBtn.SetActive(!won);

        BindButton("ContinueButton", OnContinuePressed);
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

    Text FindText(string name)
    {
        var go = GameObject.Find(name);
        return go != null ? go.GetComponent<Text>() : null;
    }

    void OnContinuePressed()
    {
        AudioManager.Instance.PlayButtonClick();
        SceneLoader.Instance.LoadBracket();
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
