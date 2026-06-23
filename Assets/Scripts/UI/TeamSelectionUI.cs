using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TeamSelectionUI : MonoBehaviour
{
    [Header("References")]
    public TeamDatabase teamDatabase;
    public Transform teamGrid;
    public GameObject teamCardPrefab;

    private TeamData selectedTeam;

    void Start()
    {
        PopulateTeams();
    }

    void PopulateTeams()
    {
        foreach (TeamData team in teamDatabase.GetAllTeams())
        {
            GameObject card = Instantiate(teamCardPrefab, teamGrid);
            card.GetComponent<TeamCardUI>().Setup(team, OnTeamSelected);
        }
    }

    void OnTeamSelected(TeamData team)
    {
        AudioManager.Instance.PlayButtonClick();
        selectedTeam = team;
        GameManager.Instance.SelectTeam(team);
        SceneLoader.Instance.LoadTeamConfirmation();
    }
}
