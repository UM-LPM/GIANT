using AgentOrganizations;
using Evaluators.CompetitionOrganizations;
using System.Collections.Generic;
using UnityEngine;

namespace Evaluators.CompetitionOrganizations
{
    public abstract class CompetitionTeamOrganizator : MonoBehaviour
    {
        public abstract List<CompetitionTeam> OrganizeTeams(Individual[] individuals, CompetitionPlayer[] ratingPlayers);
    }
}