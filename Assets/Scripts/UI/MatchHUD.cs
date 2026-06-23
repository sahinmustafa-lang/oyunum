using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MatchHUD : MonoBehaviour
{
    [Header("Score")]
    public Text roundText;
    public Text playerTeamText;
    public Text opponentTeamText;
    public Text scoreText;

    [Header("Instructions")]
    public Text instructionText;
    public Text outcomeText;

    [Header("Penalty Icons")]
    public Transform playerIconsParent;
    public Transform opponentIconsParent;
    public GameObject penaltyIconPrefab;

    [Header("Icon Colors")]
    public Color goalColor = Color.green;
    public Color missColor = Color.red;

    void Awake()
    {
        if (roundText == null)        roundText        = FindText("RoundText");
        if (playerTeamText == null)   playerTeamText   = FindText("PlayerTeamText");
        if (opponentTeamText == null) opponentTeamText = FindText("OpponentTeamText");
        if (scoreText == null)        scoreText        = FindText("ScoreText");
        if (instructionText == null)  instructionText  = FindText("InstructionText");
        if (outcomeText == null)      outcomeText      = FindText("OutcomeText");

        if (playerIconsParent == null)
        {
            var go = GameObject.Find("PlayerIcons");
            if (go != null) playerIconsParent = go.transform;
        }
        if (opponentIconsParent == null)
        {
            var go = GameObject.Find("OpponentIcons");
            if (go != null) opponentIconsParent = go.transform;
        }

        if (penaltyIconPrefab == null)
            penaltyIconPrefab = Resources.Load<GameObject>("PenaltyIcon");
    }

    Text FindText(string name)
    {
        var go = GameObject.Find(name);
        return go != null ? go.GetComponent<Text>() : null;
    }

    public void Setup(TeamData player, TeamData opponent, string roundName)
    {
        if (roundText) roundText.text = roundName;
        if (playerTeamText) playerTeamText.text = player.teamName;
        if (opponentTeamText) opponentTeamText.text = opponent.teamName;
        if (scoreText) scoreText.text = "0 - 0";
        if (outcomeText) outcomeText.gameObject.SetActive(false);
    }

    public void ShowInstruction(string text)
    {
        if (instructionText) instructionText.text = text;
    }

    public void ShowOutcome(ShotOutcome outcome)
    {
        if (outcomeText == null) return;
        outcomeText.gameObject.SetActive(true);

        switch (outcome)
        {
            case ShotOutcome.Goal: outcomeText.text = "GOAL!";  outcomeText.color = goalColor; break;
            case ShotOutcome.Save: outcomeText.text = "SAVED!"; outcomeText.color = Color.blue; break;
            case ShotOutcome.Miss: outcomeText.text = "MISS!";  outcomeText.color = missColor; break;
            case ShotOutcome.Post: outcomeText.text = "POST!";  outcomeText.color = Color.yellow; break;
        }
    }

    public void HideOutcome()
    {
        if (outcomeText) outcomeText.gameObject.SetActive(false);
    }

    public void UpdateScores(int playerGoals, int opponentGoals,
        List<ShotOutcome> playerHistory, List<ShotOutcome> opponentHistory)
    {
        if (scoreText) scoreText.text = $"{playerGoals} - {opponentGoals}";
        RefreshIcons(playerIconsParent, playerHistory);
        RefreshIcons(opponentIconsParent, opponentHistory);
    }

    void RefreshIcons(Transform parent, List<ShotOutcome> history)
    {
        if (parent == null || penaltyIconPrefab == null) return;

        foreach (Transform child in parent)
            Destroy(child.gameObject);

        foreach (ShotOutcome outcome in history)
        {
            GameObject icon = Instantiate(penaltyIconPrefab, parent);
            Image img = icon.GetComponent<Image>();
            if (img == null) continue;
            img.color = outcome == ShotOutcome.Goal ? goalColor : missColor;
        }
    }
}
