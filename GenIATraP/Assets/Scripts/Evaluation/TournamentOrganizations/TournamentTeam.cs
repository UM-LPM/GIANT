using AgentOrganizations;
using System.ComponentModel;

namespace Evaluators.TournamentOrganizations
{
    public class TournamentTeam : Team
    {
        public int Score { get; set; }
        public bool HasBye { get; set; }

        public TournamentTeam(int teamId, string teamName, Individual[] individuals) : this(teamId, teamName, individuals, 0, false)
        {
        }

        public TournamentTeam(int teamId, string teamName, Individual[] individuals, int score, bool hasBye) : base(teamId, teamName, individuals)
        {
            Score = score;
            HasBye = hasBye;
        }
    }
}