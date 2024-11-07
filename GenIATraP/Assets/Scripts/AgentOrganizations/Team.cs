using System;
using UnityEngine;

namespace AgentOrganizations
{
    [Serializable]
    [CreateAssetMenu(fileName = "Team", menuName = "AgentOrganizations/Team")]
    public class Team: ScriptableObject
    {
        public int TeamId;
        public string TeamName;
        public Individual[] Individuals;

        public Team(int teamId)
        {
            TeamId = teamId;
            name = "dummyTeam";
            TeamName = "dummyTeam";
            Individuals = null;
        }

        public Team(int teamId, string teamName, Individual[] individuals)
        {
            TeamId = teamId;
            name = teamName;
            TeamName = teamName;
            Individuals = individuals;
        }

        public string GetTeamName()
        {
            string teamName = "";
            foreach (var individual in Individuals)
            {
                teamName += individual.name + "_";
            }
            return teamName.Substring(0, teamName.Length - 1);
        }
    }
}