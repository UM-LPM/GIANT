using AgentOrganizations;
using Evaluators.CompetitionOrganizations;
using Fitnesses;
using Moserware.Skills;
using System.Collections.Generic;
using Glicko2;
using System.Linq;
using System;
using Unity.Mathematics;
using Kezyma.EloRating;
using Base;

namespace Evaluators.CompetitionOrganizations
{
    public class Glicko2RatingSystem : RatingSystem
    {
        public double DefaultRating;
        public double DefaultRatingDeviation;
        public double DefaultVolatility;

        public Glicko2RatingSystem(double defaultRating = 1500, double defaultRatingDeviation = 350, double defaultVolatility = 0.06)
        {
            DefaultRating = defaultRating;
            DefaultRatingDeviation = defaultRatingDeviation;
            DefaultVolatility = defaultVolatility;
        }


        public override void DefinePlayers(Individual[] individuals, RatingSystemRating[] initialPlayerRaitings)
        {
            if (initialPlayerRaitings != null && initialPlayerRaitings.Length < individuals.Length)
            {
                throw new Exception("Initial individual rating array is not the same size as the number of individuals in the competition");
            }

            foreach (Individual individual in individuals)
            {
                RatingSystemRating individualRating = initialPlayerRaitings?.FirstOrDefault(x => x.IndividualID == individual.IndividualId);

                if (individualRating != null && individualRating.AdditionalValues != null)
                {
                    double rating;
                    double ratingDeviation;
                    double volatility;

                    if (!individualRating.AdditionalValues.TryGetValue("Rating", out rating))
                        rating = DefaultRating;

                    if (!individualRating.AdditionalValues.TryGetValue("RatingDeviation", out ratingDeviation))
                        ratingDeviation = DefaultRatingDeviation;

                    if (!individualRating.AdditionalValues.TryGetValue("Volatility", out volatility))
                        volatility = DefaultVolatility;

                    Players.Add(new Glicko2Player(individual.IndividualId, rating, ratingDeviation, volatility));
                }
                else
                {
                    Players.Add(new Glicko2Player(individual.IndividualId, DefaultRating, DefaultRatingDeviation, DefaultVolatility));
                }
            }
        }

        public override void UpdateRatings(List<MatchFitness> competitionMatchFitnesses)
        {
            List<MatchFitness> competitionMatchFitnessesCopy = new List<MatchFitness>(competitionMatchFitnesses);

            MatchFitness matchFitness;
            List<MatchFitness> matchFitnesses = new List<MatchFitness>();
            List<MatchFitness> matchFitnessesSwaped = new List<MatchFitness>();
            while (competitionMatchFitnessesCopy.Count > 0)
            { 
                // 1. Get all matchFitness data
                matchFitness = new MatchFitness();
                MatchFitness.GetMatchFitness(competitionMatchFitnessesCopy, matchFitness, matchFitnesses, matchFitnessesSwaped, Coordinator.Instance.SwapCompetitionMatchTeams);

                if (matchFitness.IsDummy)
                {
                    continue;
                }

                // If the matchFitness is a dummy matchFitness, skip it (this matchFitness is used for teams who got bye on a competition
                if (matchFitness.IsDummy)
                    continue;

                if (matchFitness.TeamFitnesses.Count != 2)
                {
                    throw new System.Exception("Glicko2 requires exactly two teams with one individual in each matchFitness");
                }

                // Check if any matchFitness team has more than one individual
                if (matchFitness.TeamFitnesses[0].IndividualFitness.Select(x => x.IndividualID).Distinct().ToList().Count != 1 || matchFitness.TeamFitnesses[1].IndividualFitness.Select(x => x.IndividualID).Distinct().ToList().Count != 1)
                {
                    throw new System.Exception("Glicko2 requires exactly one individual in each team");
                }

                // 2. Get players
                Glicko2Player playerA = GetPlayer(matchFitness.TeamFitnesses[0].IndividualFitness[0].IndividualID);
                Glicko2Player playerB = GetPlayer(matchFitness.TeamFitnesses[1].IndividualFitness[0].IndividualID);

                // 2. Calculate new ratings & update players
                float[] matchResult = GetMatchResult(matchFitness.GetTeamFitnesses());

                var playerAOpponents = new List<GlickoOpponent>
                {
                    new GlickoOpponent(playerB.Player, matchResult[0])
                };

                playerA.Player = GlickoCalculator.CalculateRanking(playerA.Player, playerAOpponents);

                var playerBOpponents = new List<GlickoOpponent>
                {
                    new GlickoOpponent(playerA.Player, matchResult[1])
                };

                playerB.Player = GlickoCalculator.CalculateRanking(playerB.Player, playerBOpponents);

                // 3. Add matchFitness results
                playerA.AddIndividualMatchResult(matchFitness.MatchName, matchFitness.TeamFitnesses[0].IndividualFitness[0], new int[] { matchFitness.TeamFitnesses[1].IndividualFitness[0].IndividualID });
                playerB.AddIndividualMatchResult(matchFitness.MatchName, matchFitness.TeamFitnesses[1].IndividualFitness[0], new int[] { matchFitness.TeamFitnesses[0].IndividualFitness[0].IndividualID });
            }
        }

        public override RatingSystemRating[] GetFinalRatings()
        {
            RatingSystemRating[] ratings = new RatingSystemRating[Players.Count];

            var i = 0;
            foreach (Glicko2Player ratingPlayer in Players.OfType<Glicko2Player>())
            {
                ratings[i] = new RatingSystemRating(Players[i].IndividualID, Players[i].IndividualMatchResults, new Dictionary<string, double> { { "Rating", ratingPlayer.Player.Rating }, { "RatingDeviation", ratingPlayer.Player.RatingDeviation }, { "Volatility", ratingPlayer.Player.Volatility } });
                i++;
            }

            return ratings;
        }

        public Glicko2Player GetPlayer(int id)
        {
            var player = Players.FirstOrDefault(p => p.IndividualID.Equals(id) && p is Glicko2Player);
            return player as Glicko2Player;
        }

        private float[] GetMatchResult(float[] teamFitnesses)
        {
            // 1 win, 0.5 draw, 0 loss
            if (teamFitnesses[0] < teamFitnesses[1])
            {
                return new float[] { 1, 0 };
            }
            else if (teamFitnesses[0] > teamFitnesses[1])
            {
                return new float[] { 0, 1 };
            }
            else
            {
                return new float[] { 0.5f, 0.5f };
            }
        }
    }

    public class Glicko2Player : CompetitionPlayer
    {
        public GlickoPlayer Player { get; set; }

        public Glicko2Player(int individualId, double rating, double ratingDeviation, double volatility)
            :base(individualId)
        {
            Player = new GlickoPlayer(rating, ratingDeviation, volatility);
        }

        public void UpdateRating(GlickoPlayer player)
        {
            Player = player;
        }

        public override void DisplayScore()
        {
            UnityEngine.Debug.Log("Player: " + IndividualID + " Rating: " + Player.Rating + " RD: " + Player.RatingDeviation + " Volatility: " + Player.Volatility);
        }

        public override double GetScore()
        {
            return Player.Rating;
        }
    }
}