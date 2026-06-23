using System;

[Serializable]
public class PenaltyShotData
{
    public ShotZone targetZone;
    public ShotZone fakeZone;
    public float power;
    public float timingQuality;
    public float accuracy;
    public bool isPlayerShot;
    public TeamData shootingTeam;
    public int wideMissSide;  // 0=over-bar, -1=wide left, +1=wide right
}
