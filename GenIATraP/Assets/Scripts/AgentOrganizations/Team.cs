using Fitnesses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AgentOrganizations
{
    [Serializable]
    [CreateAssetMenu(fileName = "Team", menuName = "AgentOrganizations/Team")]
    public class Team: ScriptableObject
    {
        public int TeamId;
        public Individual[] Individuals;

        public List<IndividualMatchResult> IndividualMatchResults { get; set; }

        public Team(int teamId) : this (teamId, "dummyTeam", null)
        {
        }

        [JsonConstructor]
        public Team(int teamId, string teamName, Individual[] individuals)
        {
            TeamId = teamId;
            name = teamName;
            Individuals = individuals;
            IndividualMatchResults = new List<IndividualMatchResult>();
        }

        public virtual Team Initialize(int teamId, string teamName, Individual[] individuals)
        {
            TeamId = teamId;
            name = teamName;
            Individuals = individuals;
            IndividualMatchResults = new List<IndividualMatchResult>();
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