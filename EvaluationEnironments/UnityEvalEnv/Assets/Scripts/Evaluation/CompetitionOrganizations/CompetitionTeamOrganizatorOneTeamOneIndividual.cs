using AgentOrganizations;
using Evaluators.CompetitionOrganizations;
using System.Collections.Generic;
using UnityEngine;

namespace Evaluators.CompetitionOrganizations
{
    public class CompetitionTeamOrganizatorOneTeamOneIndividual : CompetitionTeamOrganizator
    {
        public override List<CompetitionTeam> OrganizeTeams(Individual[] individuals, CompetitionPlayer[] ratingPlayers)
        {
            List<CompetitionTeam> teams = new List<CompetitionTeam>();

            for (int i = 0; i < individuals.Length; i++)
            {
                teams.Add(ScriptableObject.CreateInstance<CompetitionTeam>().Initialize(i, "Team " + i, new Individual[] { individuals[i] }) as CompetitionTeam);
            }

            return teams;
        }
    }
}