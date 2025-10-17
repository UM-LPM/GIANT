using AgentOrganizations;
using Evaluators.CompetitionOrganizations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluators.CompetitionOrganizations
{
    public class CompetitionTeamOrganizator : MonoBehaviour
    {
        [SerializeField] private int TeamSize = 2;

        public List<CompetitionTeam> OrganizeTeams(Individual[] individuals, CompetitionPlayer[] competitionPlayers)
        {
            if (individuals.Length % TeamSize != 0)
            {
                throw new System.Exception($"Number of individuals must be dividable by {TeamSize} for OneTeamTwoIndividuals organization.");
            }

            // If one individual per team, use the simpler organizator
            if (TeamSize == 1)
            {
                return OrganizeTeamsOneTeamOneIndividual(individuals);
            }
            else
            {
                List<CompetitionTeam> teams = new List<CompetitionTeam>();

                // Case: no ratingPlayers -> assign sequentially
                if (competitionPlayers == null || competitionPlayers.Length != individuals.Length)
                {
                    for (int i = 0; i < individuals.Length; i += TeamSize)
                    {
                        int teamId = teams.Count;
                        var teamMembers = individuals.Skip(i).Take(TeamSize).ToArray();
                        teams.Add(ScriptableObject.CreateInstance<CompetitionTeam>()
                            .Initialize(teamId, "Team " + teamId, teamMembers) as CompetitionTeam);
                    }
                    return teams;
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
                    teams.Add(ScriptableObject.CreateInstance<CompetitionTeam>()
                        .Initialize(teamId, "Team " + teamId, teamMembers) as CompetitionTeam);
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

                return teams;
            }
        }

        public List<CompetitionTeam> OrganizeTeamsOneTeamOneIndividual(Individual[] individuals)
        {
            List<CompetitionTeam> teams = new List<CompetitionTeam>();

            for (int i = 0; i < individuals.Length; i++)
            {
                teams.Add(ScriptableObject.CreateInstance<CompetitionTeam>().Initialize(i, "Team " + i, new Individual[] { individuals[i] }) as CompetitionTeam);
            }

            return teams;
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