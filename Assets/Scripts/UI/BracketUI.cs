using UnityEngine;
using UnityEngine.UI;

public class BracketUI : MonoBehaviour
{
    public Text roundNameText;
    public Text matchupText;
    public Text otherMatchesText;

    void Start()
    {
        if (roundNameText == null) roundNameText = FindText("RoundNameText");
        if (matchupText == null)   matchupText   = FindText("MatchupText");
        if (otherMatchesText == null) otherMatchesText = FindText("OtherMatchesText");

        TournamentData t = GameManager.Instance.CurrentTournament;
        if (t != null)
        {
            if (roundNameText) roundNameText.text = TournamentManager.Instance.GetCurrentRoundName();
            if (matchupText)   matchupText.text   = $"{t.playerTeam.teamName}\nvs\n{t.currentOpponent.teamName}";

            string others = "Other Matches:\n";
            foreach (MatchData match in t.currentRoundMatches)
                if (!match.isPlayerMatch)
                    others += $"{match.teamA.teamName} vs {match.teamB.teamName}\n";

            if (otherMatchesText) otherMatchesText.text = others;
        }

        BindButton("StartMatchButton", OnStartMatchPressed);
        BindButton("MainMenuButton",   OnMainMenuPressed);
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

    void OnStartMatchPressed()
    {
        AudioManager.Instance.PlayButtonClick();
        MatchData playerMatch = TournamentManager.Instance.GetCurrentPlayerMatch();
        GameManager.Instance.SetCurrentMatch(playerMatch);
        SceneLoader.Instance.LoadPenaltyMatch();
    }

    void OnMainMenuPressed()
    {
        AudioManager.Instance.PlayButtonClick();
        GameManager.Instance.ResetGame();
        SceneLoader.Instance.LoadMainMenu();
    }
}
