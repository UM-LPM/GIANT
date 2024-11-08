using AgentOrganizations;
using Newtonsoft.Json;

namespace Evaluators.TournamentOrganizations
{
    public class TournamentTeam : Team
    {
        public int Score { get; set; }
        public bool HasBye { get; set; }

        public TournamentTeam(int teamId, string teamName, Individual[] individuals) : this(teamId, teamName, individuals, 0, false)
        {
        }

        [JsonConstructor]
        public TournamentTeam(int teamId, string teamName, Individual[] individuals, int score, bool hasBye) : base(teamId, teamName, individuals)
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
    }
}