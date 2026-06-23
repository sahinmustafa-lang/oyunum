using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TeamDatabase", menuName = "Penalty Game/Team Database")]
public class TeamDatabase : ScriptableObject
{
    public List<TeamData> allTeams = new List<TeamData>();

    public List<TeamData> GetAllTeams() => new List<TeamData>(allTeams);

    public TeamData GetTeamById(string id)
    {
        return allTeams.Find(t => t.teamId == id);
    }

    public List<TeamData> GetRandomOpponents(int count, TeamData excludeTeam)
    {
        List<TeamData> pool = allTeams.FindAll(t => t.teamId != excludeTeam.teamId);
        List<TeamData> result = new List<TeamData>();

        while (result.Count < count && pool.Count > 0)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }
}
