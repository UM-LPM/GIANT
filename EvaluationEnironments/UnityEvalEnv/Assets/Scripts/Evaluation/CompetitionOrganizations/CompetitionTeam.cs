using AgentOrganizations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Evaluators.CompetitionOrganizations
{
    public class CompetitionTeam : Team
    {
        public double Score { get; set; }
        public bool HasBye { get; set; }

        public CompetitionTeam(int teamId, string teamName, Individual[] individuals) : this(teamId, teamName, individuals, 0, false)
        {
        }

        [JsonConstructor]
        public CompetitionTeam(int teamId, string teamName, Individual[] individuals, double score, bool hasBye) : base(teamId, teamName, individuals)
        {
            Score = score;
            HasBye = hasBye;
        }

        public override Team Initialize(int teamId, string teamName, Individual[] individuals)
        {
            base.Initialize(teamId, teamName, individuals);
            Score = 0;
            HasBye = false;

            return this;
        }

        public static implicit operator List<object>(CompetitionTeam v)
        {
            throw new NotImplementedException();
        }
    }
}