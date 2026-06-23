using UnityEngine;
using System.Collections.Generic;

public class TournamentManager : MonoBehaviour
{
    public static TournamentManager Instance { get; private set; }

    private TournamentData tournament;

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

    public void GenerateTournament(TeamData playerTeam, List<TeamData> allTeams)
    {
        tournament = new TournamentData();
        tournament.playerTeam = playerTeam;
        tournament.currentRound = TournamentRound.QuarterFinal;

        tournament.teams = new List<TeamData> { playerTeam };
        List<TeamData> opponents = new List<TeamData>(allTeams);
        opponents.RemoveAll(t => t.teamId == playerTeam.teamId);

        List<TeamData> shuffled = ShuffleList(opponents);
        for (int i = 0; i < 7 && i < shuffled.Count; i++)
            tournament.teams.Add(shuffled[i]);

        GenerateRoundMatches();
    }

    void GenerateRoundMatches()
    {
        tournament.currentRoundMatches = new List<MatchData>();
        List<TeamData> shuffled = ShuffleList(tournament.teams);

        bool playerAssigned = false;
        for (int i = 0; i < shuffled.Count - 1; i += 2)
        {
            MatchData match = new MatchData();
            match.round = tournament.currentRound;

            bool playerInThisPair = shuffled[i].teamId == tournament.playerTeam.teamId ||
                                    shuffled[i + 1].teamId == tournament.playerTeam.teamId;

            if (playerInThisPair && !playerAssigned)
            {
                match.teamA = tournament.playerTeam;
                match.teamB = (shuffled[i].teamId == tournament.playerTeam.teamId) ? shuffled[i + 1] : shuffled[i];
                match.isPlayerMatch = true;
                tournament.currentOpponent = match.teamB;
                playerAssigned = true;
            }
            else
            {
                match.teamA = shuffled[i];
                match.teamB = shuffled[i + 1];
                match.isPlayerMatch = false;
            }

            tournament.currentRoundMatches.Add(match);
        }
    }

    public MatchData GetCurrentPlayerMatch()
    {
        return tournament.currentRoundMatches.Find(m => m.isPlayerMatch);
    }

    public void CompletePlayerMatch(bool playerWon, int playerScore, int opponentScore)
    {
        MatchData match = GetCurrentPlayerMatch();
        if (match == null) return;

        match.teamAScore = playerScore;
        match.teamBScore = opponentScore;
        match.winner = playerWon ? tournament.playerTeam : tournament.currentOpponent;

        SimulateOtherMatches();
    }

    void SimulateOtherMatches()
    {
        foreach (MatchData match in tournament.currentRoundMatches)
        {
            if (!match.isPlayerMatch && match.winner == null)
                match.winner = MatchSimulator.Simulate(match);
        }
    }

    public bool AdvanceRound()
    {
        List<TeamData> winners = new List<TeamData>();
        foreach (MatchData match in tournament.currentRoundMatches)
        {
            if (match.winner != null)
                winners.Add(match.winner);
        }

        if (winners.Count < 2)
        {
            tournament.isCompleted = true;
            return false;
        }

        tournament.teams = winners;

        if (tournament.currentRound == TournamentRound.QuarterFinal)
            tournament.currentRound = TournamentRound.SemiFinal;
        else if (tournament.currentRound == TournamentRound.SemiFinal)
            tournament.currentRound = TournamentRound.Final;
        else
        {
            tournament.isCompleted = true;
            return false;
        }

        GenerateRoundMatches();
        return true;
    }

    public bool IsTournamentWon()
    {
        if (tournament.currentRound != TournamentRound.Final) return false;
        MatchData finalMatch = GetCurrentPlayerMatch();
        return finalMatch != null && finalMatch.winner != null &&
               finalMatch.winner.teamId == tournament.playerTeam.teamId;
    }

    public TournamentData GetTournamentData() => tournament;

    public string GetCurrentRoundName()
    {
        switch (tournament.currentRound)
        {
            case TournamentRound.QuarterFinal: return "Quarter Final";
            case TournamentRound.SemiFinal:    return "Semi Final";
            case TournamentRound.Final:        return "Final";
            default: return "";
        }
    }

    List<TeamData> ShuffleList(List<TeamData> list)
    {
        List<TeamData> copy = new List<TeamData>(list);
        for (int i = copy.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            TeamData temp = copy[i];
            copy[i] = copy[j];
            copy[j] = temp;
        }
        return copy;
    }
}
