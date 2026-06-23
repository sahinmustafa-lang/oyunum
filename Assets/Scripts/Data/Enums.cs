public enum ShotZone
{
    LowLeft,
    MidLeft,
    HighLeft,
    LowCenter,
    MidCenter,
    HighCenter,
    LowRight,
    MidRight,
    HighRight
}

public enum ShotOutcome
{
    Goal,
    Save,
    Miss,
    Post
}

public enum MatchSide
{
    Player,
    Opponent
}

public enum MatchPhase
{
    Intro,
    PlayerShooting,
    OpponentShooting,
    Resolving,
    ShowingResult,
    MatchEnded
}

public enum TournamentRound
{
    QuarterFinal,
    SemiFinal,
    Final
}
