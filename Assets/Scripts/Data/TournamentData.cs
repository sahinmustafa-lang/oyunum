using System;
using System.Collections.Generic;

[Serializable]
public class TournamentData
{
    public List<TeamData> teams = new List<TeamData>();
    public List<MatchData> currentRoundMatches = new List<MatchData>();
    public TournamentRound currentRound = TournamentRound.QuarterFinal;
    public TeamData playerTeam;
    public TeamData currentOpponent;
    public bool isCompleted;
}
