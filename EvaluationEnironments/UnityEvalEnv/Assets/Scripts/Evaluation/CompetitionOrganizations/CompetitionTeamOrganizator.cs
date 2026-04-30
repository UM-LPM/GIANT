using AgentOrganizations;
using Evaluators.CompetitionOrganizations;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Evaluators.CompetitionOrganizations
{
    public class CompetitionTeamOrganizator : MonoBehaviour
    {
        [SerializeField] private int TeamSize = 2;

        public CompetitionTeam[][] OrganizeTeams(Individual[][] individuals, CompetitionPlayer[][] competitionPlayers)
        {
            // Check if individuals is null or empty or individuals[x] is dividable by TeamSize, otherwise throw exception
            if (individuals == null ||
                individuals.Length == 0 ||
                individuals.Any(group => group == null || group.Length == 0 || group.Length % TeamSize != 0)
                )
            {
                throw new System.Exception("Invalid individuals array! Ensure it's not null, not empty, and each group has a number of individuals divisible by the team size.");
            }

            CompetitionTeam[][] result = new CompetitionTeam[individuals.Length][];

            int teamIdCounter = 0; // Counter to assign unique team IDs across all groups
            for (int g = 0; g < individuals.Length; g++)
            {
                Individual[] groupIndividuals = individuals[g];
                CompetitionPlayer[] groupPlayers = null;

                if (competitionPlayers != null && g < competitionPlayers.Length)
                    groupPlayers = competitionPlayers[g];

                result[g] = OrganizeTeams(groupIndividuals, groupPlayers, teamIdCounter);
                teamIdCounter += result[g].Length; // Increment team ID counter by the number of teams created in this group
            }

            return result;
        }

        private CompetitionTeam[] OrganizeTeams(Individual[] individuals, CompetitionPlayer[] competitionPlayers, int teamIdCounter)
        {
            // If one individual per team, use the simpler organizator
            if (TeamSize == 1)
            {
                return OrganizeTeamsOneTeamOneIndividual(individuals, teamIdCounter);
            }
            else
            {
                List<CompetitionTeam> teams = new List<CompetitionTeam>();

                // Case: no ratingPlayers -> assign sequentially
                if (competitionPlayers == null || competitionPlayers.Length != individuals.Length)
                {
                    for (int i = 0; i < individuals.Length; i += TeamSize)
                    {
                        int teamId = teamIdCounter + teams.Count;
                        var teamMembers = individuals.Skip(i).Take(TeamSize).ToArray();
                        //teams.Add(ScriptableObject.CreateInstance<CompetitionTeam>().Initialize(teamId, "Team " + teamId, teamMembers) as CompetitionTeam);
                        teams.Add(new CompetitionTeam(teamId, "Team " + teamId, teamMembers));
                    }
                    return teams.ToArray();
                }

                // Case: use ratingPlayers to balance teams
                double averagePlayerRating = competitionPlayers.Sum(p => p.GetScore()) / competitionPlayers.Length;
                double teamRating = averagePlayerRating * TeamSize;

                bool[] individualAssigned = new bool[individuals.Length];

                for (int i = 0; i < individuals.Length; i++)
                {
                    if (individualAssigned[i])
                        continue;

                    var currentTeam = new List<int> { i };
                    individualAssigned[i] = true;

                    while (currentTeam.Count < TeamSize)
                    {
                        double bestDifference = double.MaxValue;
                        int bestCandidateIndex = -1;

                        foreach (int j in Enumerable.Range(0, individuals.Length))
                        {
                            if (individualAssigned[j]) continue;

                            // calculate rating with candidate added
                            double potentialRating = currentTeam
                                .Sum(idx => competitionPlayers[idx].GetScore()) + competitionPlayers[j].GetScore();

                            double difference = System.Math.Abs(potentialRating - teamRating);
                            if (difference < bestDifference)
                            {
                                bestDifference = difference;
                                bestCandidateIndex = j;
                            }
                        }

                        if (bestCandidateIndex == -1)
                            break;

                        currentTeam.Add(bestCandidateIndex);
                        individualAssigned[bestCandidateIndex] = true;
                    }

                    int teamId = teams.Count;
                    var teamMembers = currentTeam.Select(idx => individuals[idx]).ToArray();
                    //teams.Add(ScriptableObject.CreateInstance<CompetitionTeam>().Initialize(teamId, "Team " + teamId, teamMembers) as CompetitionTeam);
                    teams.Add(new CompetitionTeam(teamId, "Team " + teamId, teamMembers));
                }

                // Assign team scores
                foreach (var team in teams)
                {
                    team.Score = team.Individuals.Sum(ind =>
                    {
                        var player = competitionPlayers.FirstOrDefault(p => p.IndividualID == ind.IndividualId);
                        return player != null ? player.GetScore() : 0;
                    });
                }

                return teams.ToArray();
            }
        }

        public CompetitionTeam[] OrganizeTeamsOneTeamOneIndividual(Individual[] individuals, int teamIdCounter)
        {
            List<CompetitionTeam> teams = new List<CompetitionTeam>();

            for (int i = 0; i < individuals.Length; i++)
            {
                //teams.Add(ScriptableObject.CreateInstance<CompetitionTeam>().Initialize(teamIdCounter + i, "Team " + (teamIdCounter + i), new Individual[] { individuals[i] }) as CompetitionTeam);
                teams.Add(new CompetitionTeam(teamIdCounter + i, "Team " + (teamIdCounter + i), new Individual[] { individuals[i] }));
            }

            return teams.ToArray();
        }

        public int GetTeamSize()
        {
            return TeamSize;
        }

        public void SetTeamSize(int teamSize)
        {
            TeamSize = teamSize;
        }
    }
}