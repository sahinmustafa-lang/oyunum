using UnityEngine;
using UnityEngine.UI;

public class ChampionUI : MonoBehaviour
{
    void Start()
    {
        TeamData team = GameManager.Instance.SelectedTeam;
        if (team != null)
        {
            var championText = FindText("ChampionText");
            var teamNameText = FindText("TeamNameText");
            if (championText) championText.text = "CHAMPION!";
            if (teamNameText) teamNameText.text = team.teamName;
        }

        BindButton("PlayAgainButton", OnPlayAgainPressed);
        BindButton("MainMenuButton",  OnMainMenuPressed);
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

    void OnPlayAgainPressed()
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
