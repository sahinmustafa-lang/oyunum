using UnityEngine;

[CreateAssetMenu(fileName = "TeamData", menuName = "Penalty Game/Team Data")]
public class TeamData : ScriptableObject
{
    public string teamId;
    public string teamName;
    public Sprite logo;
    public Sprite kit;
    public Color primaryColor = Color.white;
    public Color secondaryColor = Color.black;
    [Range(1, 100)] public int shooterStrength = 50;
    [Range(1, 100)] public int keeperStrength = 50;
    [Range(1, 100)] public int aiStrength = 50;
}
