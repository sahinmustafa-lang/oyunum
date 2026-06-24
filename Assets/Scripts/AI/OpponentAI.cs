using UnityEngine;

public static class OpponentAI
{
    public static ShotZone ChooseKeeperZone(ShotZone fakeZone, int aiStrength)
    {
        float roll = Random.value;
        float followFakeChance = Mathf.Lerp(0.6f, 0.3f, aiStrength / 100f);
        float randomChance = Mathf.Lerp(0.3f, 0.2f, aiStrength / 100f);

        if (roll < followFakeChance)
            return fakeZone;

        if (roll < followFakeChance + randomChance)
            return (ShotZone)Random.Range(0, 9);

        // Smart guess - pick adjacent to fake
        int fakeIndex = (int)fakeZone;
        int[] adjacents = GetAdjacentIndices(fakeIndex);
        return (ShotZone)adjacents[Random.Range(0, adjacents.Length)];
    }

    public static PenaltyShotData GenerateShot(TeamData team, int roundDifficulty)
    {
        PenaltyShotData shot = new PenaltyShotData();
        shot.shootingTeam = team;
        shot.isPlayerShot = false;

        float strengthFactor = team.shooterStrength / 100f;
        float difficultyBonus = roundDifficulty * 0.05f;

        // AI always has good timing — red dot reliably shows real target
        shot.timingQuality = Mathf.Clamp01(Random.Range(0.75f, 1f) * (1f + difficultyBonus));
        shot.power         = Random.Range(0.65f, 0.95f);   // never max power (avoids any edge effects)
        shot.accuracy      = Mathf.Clamp01(strengthFactor + Random.Range(0f, 0.25f));

        // Tüm 9 bölgeye eşit dağılım; güçlü takımlar biraz köşe tercih eder
        float cornerBias = (strengthFactor + difficultyBonus - 0.5f) * 0.3f; // 0–0.15
        if (Random.value < Mathf.Clamp01(cornerBias))
        {
            ShotZone[] corners = { ShotZone.HighLeft, ShotZone.HighRight,
                                   ShotZone.LowLeft,  ShotZone.LowRight };
            shot.targetZone = corners[Random.Range(0, corners.Length)];
        }
        else
        {
            shot.targetZone = (ShotZone)Random.Range(0, 9);
        }

        shot.fakeZone = shot.targetZone;
        return shot;
    }

    static int[] GetAdjacentIndices(int index)
    {
        System.Collections.Generic.List<int> adj = new System.Collections.Generic.List<int>();
        if (index > 0) adj.Add(index - 1);
        if (index < 8) adj.Add(index + 1);
        if (index - 3 >= 0) adj.Add(index - 3);
        if (index + 3 <= 8) adj.Add(index + 3);
        return adj.ToArray();
    }
}
