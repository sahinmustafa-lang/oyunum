using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public TeamData SelectedTeam { get; private set; }
    public TournamentData CurrentTournament { get; private set; }
    public MatchData CurrentMatch { get; private set; }
    public bool LastMatchWon { get; private set; }

    [Header("Database")]
    public TeamDatabase teamDatabase;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SelectTeam(TeamData team)
    {
        SelectedTeam = team;
    }

    public void StartNewTournament()
    {
        TournamentManager.Instance.GenerateTournament(SelectedTeam, teamDatabase.GetAllTeams());
        CurrentTournament = TournamentManager.Instance.GetTournamentData();
    }

    public void SetCurrentMatch(MatchData match)
    {
        CurrentMatch = match;
    }

    public void CompleteMatch(bool playerWon, int playerScore, int opponentScore)
    {
        LastMatchWon = playerWon;
        if (CurrentMatch != null)
        {
            CurrentMatch.teamAScore = playerScore;
            CurrentMatch.teamBScore = opponentScore;
            CurrentMatch.winner = playerWon ? SelectedTeam : CurrentTournament.currentOpponent;
        }
    }

    public void ResetGame()
    {
        SelectedTeam = null;
        CurrentTournament = null;
        CurrentMatch = null;
    }
}
