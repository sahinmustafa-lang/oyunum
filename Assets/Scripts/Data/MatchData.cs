using System;

[Serializable]
public class MatchData
{
    public TeamData teamA;
    public TeamData teamB;
    public TeamData winner;
    public int teamAScore;
    public int teamBScore;
    public bool isPlayerMatch;
    public TournamentRound round;
}
