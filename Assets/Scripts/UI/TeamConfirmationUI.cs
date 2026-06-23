using UnityEngine;
using UnityEngine.UI;

public class TeamConfirmationUI : MonoBehaviour
{
    public Image logoImage;
    public Image kitImage;
    public Text teamNameText;

    void Start()
    {
        TeamData team = GameManager.Instance.SelectedTeam;
        if (team != null)
        {
            if (teamNameText) teamNameText.text = $"You selected:\n{team.teamName}";
            if (team.logo != null && logoImage) logoImage.sprite = team.logo;
            if (team.kit != null && kitImage) kitImage.sprite = team.kit;
            if (logoImage) logoImage.color = team.primaryColor;
        }

        BindButton("ConfirmButton", OnConfirmPressed);
        BindButton("BackButton",    OnBackPressed);
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

    void OnConfirmPressed()
    {
        AudioManager.Instance.PlayButtonClick();
        GameManager.Instance.StartNewTournament();
        SceneLoader.Instance.LoadBracket();
    }

    void OnBackPressed()
    {
        AudioManager.Instance.PlayButtonClick();
        SceneLoader.Instance.LoadTeamSelection();
    }
}
