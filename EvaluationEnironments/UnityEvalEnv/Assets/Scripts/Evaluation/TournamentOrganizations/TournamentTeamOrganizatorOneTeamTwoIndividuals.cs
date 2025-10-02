using AgentOrganizations;
using Evaluators.RatingSystems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluators.TournamentOrganizations
{
    public class TournamentTeamOrganizatorOneTeamTwoIndividuals : TournamentTeamOrganizator
    {
        public override List<TournamentTeam> OrganizeTeams(Individual[] individuals, RatingPlayer[] ratingPlayers)
        {
            if( individuals.Length % 2 != 0 )
            {
                throw new System.Exception("Number of individuals must be even for OneTeamTwoIndividuals organization.");
            }

            List<TournamentTeam> teams = new List<TournamentTeam>();

            if (ratingPlayers == null || ratingPlayers.Length != individuals.Length)
            {
                // Create teams from top to bottom
                for (int i = 0; i < individuals.Length; i += 2)
                {
                    int teamId = teams.Count;
                    teams.Add(ScriptableObject.CreateInstance<TournamentTeam>().Initialize(teamId, "Team " + teamId, new Individual[] { individuals[i], individuals[i + 1] }) as TournamentTeam);
                }
                return teams;
            }

            // 1. Calculate the average player rating
            double averagePlayerRating = ratingPlayers.Sum(p => p.GetRating()) / ratingPlayers.Length;

            // 2. Calculate average team rating (sum of two individuals' ratings)
            double teamRating = averagePlayerRating * 2;

            // 3. Create teams where each team has two individuals and their combined rating is close to the average team rating
            bool[] individualAssigned = new bool[individuals.Length];

            for (int i = 0; i < individuals.Length; i++)
            {
                if (individualAssigned[i])
                    continue;
                double bestDifference = double.MaxValue;
                int bestTeamateIndex = -1;
                for (int j = 0; j < individuals.Length; j++)
                {
                    if (i == j || individualAssigned[j])
                        continue;
                    double potentialTeamRating = ratingPlayers[i].GetRating() + ratingPlayers[j].GetRating();
                    double difference = System.Math.Abs(potentialTeamRating - teamRating);
                    if (difference < bestDifference)
                    {
                        bestDifference = difference;
                        bestTeamateIndex = j;
                    }
                }
                if (bestTeamateIndex != -1)
                {
                    int teamId = teams.Count;
                    teams.Add(ScriptableObject.CreateInstance<TournamentTeam>().Initialize(teamId, "Team " + teamId, new Individual[] { individuals[i], individuals[bestTeamateIndex] }) as TournamentTeam);
                    individualAssigned[i] = true;
                    individualAssigned[bestTeamateIndex] = true;
                }
            }

            // 4. Set team scores to sum of individual ratings in the team
            foreach (var team in teams)
            {
                team.Score = team.Individuals.Sum(ind => 
                {
                    var player = ratingPlayers.FirstOrDefault(p => p.IndividualID == ind.IndividualId);
                    return player != null ? player.GetRating() : 0;
                });
            }

            return teams;
        }
    }
}