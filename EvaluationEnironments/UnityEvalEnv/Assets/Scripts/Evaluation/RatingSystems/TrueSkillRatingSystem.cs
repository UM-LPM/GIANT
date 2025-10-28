using Evaluators.CompetitionOrganizations;
using System.Collections.Generic;
using Moserware.Skills;
using Moserware.Skills.TrueSkill;
using Unity.Mathematics;
using System;
using AgentOrganizations;
using Fitnesses;
using System.Linq;
using Base;
using Utils;

namespace Evaluators.CompetitionOrganizations
{
    public class TrueSkillRatingSystem : RatingSystem
    {
        public GameInfo GameInfo;
        public float MinRating;
        public float MaxRating;

        public TrueSkillRatingSystem()
        {
            GameInfo = GameInfo.DefaultGameInfo;
            MinRating = 0;
            MaxRating = 100;
        }

        public override void DefinePlayers(Individual[] individuals, RatingSystemRating[] initialPlayerRaitings)
        {
            if (initialPlayerRaitings != null && initialPlayerRaitings.Length < individuals.Length)
            {
                throw new Exception("Initial individual rating array is not the same size as the number of individuals in the competition");
            }

            // Find unique competition individuals in teams and add them to the list of individuals
            foreach (Individual individual in individuals)
            {
                RatingSystemRating individualRating = initialPlayerRaitings?.FirstOrDefault(x => x.IndividualID == individual.IndividualId);

                if (individualRating != null && individualRating.AdditionalValues != null)
                {
                    double rating;
                    double stdDeviation;

                    if (!individualRating.AdditionalValues.TryGetValue("Rating", out rating))
                        rating = GameInfo.DefaultRating.Mean;

                    if (!individualRating.AdditionalValues.TryGetValue("StdDeviation", out stdDeviation))
                        stdDeviation = GameInfo.DefaultRating.StandardDeviation;

                    Players.Add(new TrueSkillPlayer(individual.IndividualId, new Player(individual.IndividualId), new Rating(math.abs(rating), stdDeviation)));
                }
                else
                {
                    Players.Add(new TrueSkillPlayer(individual.IndividualId, new Player(individual.IndividualId), GameInfo.DefaultRating));
                }
            }
        }

        public override void UpdateRatings(List<MatchFitness> competitionMatchFitnesses)
        {
            foreach(MatchFitness matchFitness in competitionMatchFitnesses)
            {
                // 1. Check if the match is dummy
                if (matchFitness.IsDummy)
                {
                    continue;
                }

                // 2. Calculate new ratings
                // 2.1 Define order ranking
                int[] orderRanking = GetFitnessOrder(matchFitness.GetTeamFitnesses());

                // 2.2 Calculate how much each player contributed to the team fitness
                for (int i = 0; i < matchFitness.TeamFitnesses.Count; i++)
                {
                    if (matchFitness.TeamFitnesses[i].IndividualFitness.Count > 1)
                    {
                        float teamFitness = math.abs(matchFitness.TeamFitnesses[i].GetTeamFitness());
                        foreach (IndividualFitness individualFitness in matchFitness.TeamFitnesses[i].IndividualFitness)
                        {
                            float contribution = teamFitness > 0 ? (math.abs(individualFitness.Value) / teamFitness) : 1;
                            TrueSkillPlayer trueSkillPlayer = GetPlayer(individualFitness.IndividualID);
                            if (trueSkillPlayer != null)
                            {
                                // Check if current Team won 
                                if (orderRanking[i] != 1)
                                {
                                    contribution = 1 - contribution; // If the team lost, the contribution is inverted (i.e., a player who contributed less to the team fitness should be penalized more)
                                }

                                trueSkillPlayer.Player = new Player(trueSkillPlayer.IndividualID, contribution);
                            }
                        }
                    }
                }

                // 2.3. Create teams 
                Moserware.Skills.Team[] teams = new Moserware.Skills.Team[matchFitness.TeamFitnesses.Count];

                for (int i = 0; i < matchFitness.TeamFitnesses.Count; i++)
                {
                    List<TrueSkillPlayer> teamPlayers = new List<TrueSkillPlayer>();

                    foreach (IndividualFitness player in matchFitness.TeamFitnesses[i].IndividualFitness)
                    {
                        teamPlayers.Add(GetPlayer(player.IndividualID));
                    }

                    Moserware.Skills.Team team = new Moserware.Skills.Team();
                    foreach (TrueSkillPlayer player in teamPlayers)
                    {
                        team.AddPlayer(player.Player, player.Rating);
                    }

                    teams[i] = team;
                }

                // 2.4 Calculate new ratings
                var teamsConcatinated = Teams.Concat(teams);
                var newRatings = TrueSkillCalculator.CalculateNewRatings(GameInfo, teamsConcatinated, orderRanking);

                // 2.5 Update ratings
                for (int i = 0; i < matchFitness.TeamFitnesses.Count; i++)
                {
                    for (int j = 0; j < matchFitness.TeamFitnesses[i].IndividualFitness.Count; j++)
                    {
                        TrueSkillPlayer trueSkillPlayer = GetPlayer(matchFitness.TeamFitnesses[i].IndividualFitness[j].IndividualID);
                        trueSkillPlayer.UpdateRating(newRatings[GetPlayer(matchFitness.TeamFitnesses[i].IndividualFitness[j].IndividualID).Player]);

                        // Get opponentIDs
                        List<int> opponentIDs = new List<int>();
                        for (int k = 0; k < matchFitness.TeamFitnesses.Count; k++)
                        {
                            if (k != i)
                            {
                                foreach (IndividualFitness individualFitness in matchFitness.TeamFitnesses[k].IndividualFitness)
                                {
                                    if ((!opponentIDs.Contains(individualFitness.IndividualID)) && (individualFitness.IndividualID != matchFitness.TeamFitnesses[i].IndividualFitness[j].IndividualID))
                                        opponentIDs.Add(individualFitness.IndividualID);
                                }
                            }
                        }

                        trueSkillPlayer.AddIndividualMatchResult(matchFitness.MatchName, matchFitness.TeamFitnesses[i].IndividualFitness[j], opponentIDs.ToArray());
                    }
                }
            }
        }

        public static int[] GetFitnessOrder(float[] fitnesses)
        {
            // Create an array of indices and sort them based on the fitness values
            int[] indices = Enumerable.Range(0, fitnesses.Length)
                                      .OrderBy(i => fitnesses[i]) // Ascending order
                                      .ToArray();

            int[] orders = new int[fitnesses.Length];
            int rank = 1; // Start with rank 1

            for (int i = 0; i < indices.Length; i++)
            {
                int originalIndex = indices[i];

                orders[originalIndex] = rank;

                if (i < indices.Length - 1 && fitnesses[indices[i]] == fitnesses[indices[i + 1]])
                {
                    // If tied, do not increment rank
                    continue;
                }

                rank = i + 2;
            }

            return orders;
        }

        public TrueSkillPlayer GetPlayer(int id)
        {
            return Players.FirstOrDefault(p => p.IndividualID.Equals(id) && p is TrueSkillPlayer) as TrueSkillPlayer;
        }

        public override RatingSystemRating[] GetFinalRatings()
        {
            RatingSystemRating[] ratings = new RatingSystemRating[Players.Count];

            var i = 0;
            foreach (TrueSkillPlayer ratingPlayer in Players.OfType<TrueSkillPlayer>())
            {
                ratings[i] = new RatingSystemRating(Players[i].IndividualID, Players[i].IndividualMatchResults, new Dictionary<string, double> { { "Rating", ratingPlayer.Rating.Mean }, { "StdDeviation", ratingPlayer.Rating.StandardDeviation } });
                i++;
            }

            return ratings;
        }
    }

    public class TrueSkillPlayer : CompetitionPlayer
    {
        public Player Player { get; set; }
        public Rating Rating { get; set; }

        public TrueSkillPlayer(int individualId, Player player, Rating rating)
            :base(individualId)
        {
            Player = player;
            Rating = rating;
        }

        public void UpdateRating(Rating newRating)
        {
            Rating = newRating;
        }

        public override void DisplayScore()
        {
            DebugSystem.LogDetailed($"Player {Player.Id}: {Rating}");
        }

        public override double GetScore()
        {
            return Rating.Mean;
        }
    }
}