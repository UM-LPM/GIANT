using AgentOrganizations;
using Base;
using Fitnesses;
using Moserware.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
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

        public override void DefinePlayers(Individual[][] individuals, RatingSystemRating[] initialPlayerRaitings)
        {
            if (initialPlayerRaitings != null && initialPlayerRaitings.Length < individuals.Length)
            {
                throw new Exception("Initial individual rating array is not the same size as the number of individuals in the competition");
            }

            Players = new CompetitionPlayer[individuals.Length][];
            for (int i = 0; i < individuals.Length; i++)
            {
                Players[i] = new CompetitionPlayer[individuals[i].Length];
                for (int j = 0; j < individuals[i].Length; j++)
                {
                    var individual = individuals[i][j];

                    RatingSystemRating individualRating = initialPlayerRaitings?.FirstOrDefault(x => x.IndividualID == individual.IndividualId);

                    if (individualRating != null && individualRating.AdditionalValues != null)
                    {
                        double rating;
                        double stdDeviation;

                        if (!individualRating.AdditionalValues.TryGetValue("Rating", out rating))
                            rating = GameInfo.DefaultRating.Mean;

                        if (!individualRating.AdditionalValues.TryGetValue("StdDeviation", out stdDeviation))
                            stdDeviation = GameInfo.DefaultRating.StandardDeviation;

                        Players[i][j] = new TrueSkillPlayer(individual.IndividualId, new Player(individual.IndividualId), new Rating(math.abs(rating), stdDeviation));
                    }
                    else
                    {
                        Players[i][j] = new TrueSkillPlayer(individual.IndividualId, new Player(individual.IndividualId), GameInfo.DefaultRating);
                    }
                }
            }
        }

        public override void UpdateRatings(List<MatchFitness> competitionMatchFitnesses)
        {
            // Pre-build a lookup dictionary so GetPlayer() is O(1) instead of O(n) per call
            var playerLookup = new Dictionary<int, TrueSkillPlayer>();
            foreach (var row in Players)
                foreach (var p in row)
                    if (p is TrueSkillPlayer tsp)
                        playerLookup[tsp.IndividualID] = tsp;

            var competitionMatchFitnessesCopy = competitionMatchFitnesses
                .OrderBy(mf => mf.MatchId)
                .ToList();

            var matchFitnesses = new List<MatchFitness>();
            var matchFitnessesSwaped = new List<MatchFitness>();

            while (competitionMatchFitnessesCopy.Count > 0)
            {
                var matchFitness = new MatchFitness();
                MatchFitness.GetMatchFitness(
                    competitionMatchFitnessesCopy, matchFitness,
                    matchFitnesses, matchFitnessesSwaped,
                    Coordinator.Instance.SwapCompetitionMatchTeams);

                if (matchFitness.IsDummy)
                    continue;

                int[] orderRanking = GetFitnessOrder(matchFitness.GetTeamFitnesses());

                var opponentIDsPerTeam = new List<int>[matchFitness.TeamFitnesses.Count];
                for (int i = 0; i < matchFitness.TeamFitnesses.Count; i++)
                {
                    var opponents = new List<int>();
                    for (int k = 0; k < matchFitness.TeamFitnesses.Count; k++)
                    {
                        if (k == i) continue;
                        foreach (var ind in matchFitness.TeamFitnesses[k].IndividualFitness)
                            opponents.Add(ind.IndividualID);
                    }
                    opponentIDsPerTeam[i] = opponents;
                }

                // Set partial-credit weights on Player objects
                for (int i = 0; i < matchFitness.TeamFitnesses.Count; i++)
                {
                    var teamFitness = matchFitness.TeamFitnesses[i];
                    if (teamFitness.IndividualFitness.Count <= 1) continue;

                    float[] contributions = ComputeContributions(
                        teamFitness.IndividualFitness.Select(ind => ind.Value).ToList());

                    bool teamLost = orderRanking[i] != 1;
                    for (int j = 0; j < teamFitness.IndividualFitness.Count; j++)
                    {
                        float contribution = contributions[j]; // direct index — no IndexOf()
                        if (teamLost) contribution = 1f - contribution;

                        if (playerLookup.TryGetValue(teamFitness.IndividualFitness[j].IndividualID, out var tsp))
                            tsp.Player = new Player(tsp.IndividualID, contribution);
                    }
                }

                // Build TrueSkill teams
                var teams = new Moserware.Skills.Team[matchFitness.TeamFitnesses.Count];
                for (int i = 0; i < matchFitness.TeamFitnesses.Count; i++)
                {
                    var team = new Moserware.Skills.Team();
                    foreach (var ind in matchFitness.TeamFitnesses[i].IndividualFitness)
                    {
                        if (playerLookup.TryGetValue(ind.IndividualID, out var tsp))
                            team.AddPlayer(tsp.Player, tsp.Rating);
                    }
                    teams[i] = team;
                }

                // Concat once, calculate ratings
                var teamsConcatenated = Teams.Concat(teams);
                var newRatings = TrueSkillCalculator.CalculateNewRatings(GameInfo, teamsConcatenated, orderRanking);

                // Update ratings and record match results
                for (int i = 0; i < matchFitness.TeamFitnesses.Count; i++)
                {
                    var opponentIDs = opponentIDsPerTeam[i].ToArray(); // shared across all players on this team
                    foreach (var ind in matchFitness.TeamFitnesses[i].IndividualFitness)
                    {
                        if (!playerLookup.TryGetValue(ind.IndividualID, out var tsp)) continue;
                        tsp.UpdateRating(newRatings[tsp.Player]);
                        tsp.AddIndividualMatchResult(matchFitness.MatchName, ind, opponentIDs);
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
            return AllPlayers.FirstOrDefault(p => p.IndividualID.Equals(id) && p is TrueSkillPlayer) as TrueSkillPlayer;
        }

        public override RatingSystemRating[][] GetFinalRatings()
        {
            RatingSystemRating[][] ratings = new RatingSystemRating[Players.Length][];

            for (int i = 0; i < Players.Length; i++)
            {
                ratings[i] = new RatingSystemRating[Players[i].Length];

                for (int j = 0; j < Players[i].Length; j++)
                {
                    var player = Players[i][j] as TrueSkillPlayer;
                    ratings[i][j] = new RatingSystemRating(player.IndividualID, player.IndividualMatchResults, new Dictionary<string, double> { { "Rating", player.Rating.Mean }, { "StdDeviation", player.Rating.StandardDeviation } });
                }
            }

            return ratings;
        }

        // Alternative approach to compute contributions using softmax function.
        /*public static float[] SoftmaxContributions(List<float> fitness, float T = 250f)
        {
            int n = fitness.Count;
            float[] utilities = new float[n];

            for (int i = 0; i < n; i++)
                utilities[i] = -fitness[i];

            float max = utilities.Max();

            float sum = 0f;
            float[] expVals = new float[n];

            for (int i = 0; i < n; i++)
            {
                expVals[i] = Mathf.Exp((utilities[i] - max) / T);
                sum += expVals[i];
            }

            float[] contributions = new float[n];
            float normSum = 0f;

            for (int i = 0; i < n; i++)
            {
                contributions[i] = expVals[i] / sum;
                normSum += contributions[i];
            }

            // normalization
            for (int i = 0; i < n; i++)
                contributions[i] /= normSum;

            return contributions;
        }*/

        float[] ComputeContributions(List<float> fitnesses)
        {
            if (EnvironmentControllerBase.BEST_FITNESS == float.MinValue ||
                EnvironmentControllerBase.WORST_FITNESS == float.MaxValue)
                throw new Exception("Best and worst fitness values must be set...");

            float worst = EnvironmentControllerBase.WORST_FITNESS;
            float range = worst - EnvironmentControllerBase.BEST_FITNESS;

            int n = fitnesses.Count;
            float[] contributions = new float[n];
            float sum = 0f;

            // Pass 1: compute scores in-place and accumulate sum
            for (int i = 0; i < n; i++)
            {
                contributions[i] = (worst - fitnesses[i]) / range;
                sum += contributions[i];
            }

            // Pass 2: normalize in-place
            float fallback = 1f / n;
            for (int i = 0; i < n; i++)
                contributions[i] = sum > 0f ? contributions[i] / sum : fallback;

            return contributions;
        }
    }

    public class TrueSkillPlayer : CompetitionPlayer
    {
        public Player Player { get; set; }
        public Rating Rating { get; set; }

        public TrueSkillPlayer(int individualId, Player player, Rating rating)
            : base(individualId)
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