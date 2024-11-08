using Newtonsoft.Json;
using System;
using UnityEngine;

namespace AgentOrganizations
{
    [Serializable]
    [CreateAssetMenu(fileName = "Team", menuName = "AgentOrganizations/Team")]
    public class Team: ScriptableObject
    {
        public int TeamId;
        public Individual[] Individuals;

        public Team(int teamId)
        {
            TeamId = teamId;
            name = "dummyTeam";
            Individuals = null;
        }

        [JsonConstructor]
        public Team(int teamId, string teamName, Individual[] individuals)
        {
            TeamId = teamId;
            name = teamName;
            Individuals = individuals;
        }

        public virtual Team Initialize(int teamId, string teamName, Individual[] individuals)
        {
            TeamId = teamId;
            name = teamName;
            Individuals = individuals;
            return this;
        }

        public string GetTeamName()
        {
            string teamName = "Team_" + TeamId;
            foreach (var individual in Individuals)
            {
                teamName += "_" + individual.name;
            }
            return teamName.Substring(0, teamName.Length - 1);
        }
    }
}