using AgentOrganizations;
using Evaluators.RatingSystems;
using System.Collections.Generic;
using UnityEngine;

namespace Evaluators.TournamentOrganizations
{
    public abstract class TournamentTeamOrganizator : MonoBehaviour
    {
        public abstract List<TournamentTeam> OrganizeTeams(Individual[] individuals, RatingPlayer[] ratingPlayers);
    }
}