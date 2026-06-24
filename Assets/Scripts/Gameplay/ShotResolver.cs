using UnityEngine;

public static class ShotResolver
{
    public static ShotOutcome Resolve(PenaltyShotData shot, ShotZone keeperZone,
                                      int shooterStrength, int keeperStrength)
    {
        // ── Miss check (player shots only) ───────────────────────────────────
        // AI shots never miss — red dot always shows real target
        if (shot.isPlayerShot)
        {
            // Over bar OR wide miss → guaranteed miss (direction handled by BallController)
            if (shot.timingQuality < 0.05f || shot.wideMissSide != 0)
                return ShotOutcome.Miss;

            float missChance = 0.08f;                               // base miss
            missChance += (1f - shot.timingQuality) * 0.32f;       // timing penalty (ağırlaştırıldı)
            missChance += (1f - shooterStrength / 100f) * 0.12f;   // strength penalty

            if (Random.value < missChance)
                return ShotOutcome.Miss;
        }

        // ── Post check (rare, both player and AI) ────────────────────────────
        bool isCorner = shot.targetZone == ShotZone.HighLeft  ||
                        shot.targetZone == ShotZone.HighRight  ||
                        shot.targetZone == ShotZone.LowLeft    ||
                        shot.targetZone == ShotZone.LowRight;

        float postChance = isCorner ? 0.08f : 0.03f;               // slightly higher post chance
        if (Random.value < postChance)
            return ShotOutcome.Post;

        // ── Save check ───────────────────────────────────────────────────────
        float saveChance;

        // Köşe bölgelerinde kaleci tam uzanmış — parmak uçlarına denk gelirse top içeri girebilir
        bool keeperStretched = keeperZone == ShotZone.LowLeft   || keeperZone == ShotZone.LowRight  ||
                               keeperZone == ShotZone.HighLeft  || keeperZone == ShotZone.HighRight;

        if (keeperZone == shot.targetZone)
        {
            if (keeperStretched)
                saveChance = 0.55f + (keeperStrength / 100f) * 0.18f; // 55-73% — parmak ucu
            else
                saveChance = 0.80f + (keeperStrength / 100f) * 0.18f; // 80-98% — gövde
        }
        else if (IsAdjacent(keeperZone, shot.targetZone))
            saveChance = 0.06f + (keeperStrength / 100f) * 0.05f;   // komşu ama yanlış yön: 6-11%
        else
            saveChance = 0.02f;                                       // tamamen yanlış taraf: 2%

        if (Random.value < saveChance)
            return ShotOutcome.Save;

        return ShotOutcome.Goal;
    }

    static bool IsAdjacent(ShotZone a, ShotZone b)
    {
        int ai = (int)a, bi = (int)b;
        int diff = Mathf.Abs(ai - bi);
        // Dikey komşu: aynı sütun, bir satır fark (diff=3)
        if (diff == 3) return true;
        // Yatay komşu: aynı satır, bir sütun fark (diff=1) — ama satır sarması yasak
        // Satır sonu: indeks % 3 == 2 (LowRight=2, MidRight=5, HighRight=8)
        if (diff == 1 && Mathf.Min(ai, bi) % 3 != 2) return true;
        return false;
    }
}
