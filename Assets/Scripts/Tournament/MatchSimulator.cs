using UnityEngine;

public static class MatchSimulator
{
    public static TeamData Simulate(MatchData match)
    {
        float strengthA = match.teamA.shooterStrength + match.teamA.keeperStrength;
        float strengthB = match.teamB.shooterStrength + match.teamB.keeperStrength;
        float total = strengthA + strengthB;

        float roll = Random.Range(0f, total);
        return roll < strengthA ? match.teamA : match.teamB;
    }
}
