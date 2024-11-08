using AgentOrganizations;
using System.Collections.Generic;
using UnityEngine;

namespace Evaluators.TournamentOrganizations
{
    public class TournamentTeamOrganizatorOneTeamOneIndividual : TournamentTeamOrganizator
    {
        public override List<TournamentTeam> OrganizeTeams(Individual[] individuals)
        {
            List<TournamentTeam> teams = new List<TournamentTeam>();

            for (int i = 0; i < individuals.Length; i++)
            {
                teams.Add(ScriptableObject.CreateInstance<TournamentTeam>().Initialize(i, "Team " + i, new Individual[] { individuals[i] }) as TournamentTeam);
            }

            return teams;
        }
    }
}