using AgentOrganizations;
using Base;
using Evaluators.CompetitionOrganizations;
using Fitnesses;
using Kezyma.EloRating;
using Moserware.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace Evaluators.CompetitionOrganizations
{
    public class EloRatingSystem : RatingSystem
    {
        public decimal DefaultRating;
        public int KFactorBellow2100;
        public int KFactorBetween2100And2400;
        public int KFactorAbove2400;

        private int startKFactor;

        public EloRatingSystem(decimal defaultRating = 1000, int kFactorBellow2100 = 40, int kFactorBetween2100And2400 = 20, int kFactorAbove2400 = 10)
        {
            DefaultRating = defaultRating;
            KFactorBellow2100 = kFactorBellow2100;
            KFactorBetween2100And2400 = kFactorBetween2100And2400;
            KFactorAbove2400 = kFactorAbove2400;

            // Set the start K factor
            SetStartKFactor();
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
                    double kFactor;

                    if (!individualRating.AdditionalValues.TryGetValue("Rating", out rating))
                        rating = (double)DefaultRating;

                    if (!individualRating.AdditionalValues.TryGetValue("KFactor", out kFactor))
                        kFactor = GetKFactor(rating);

                    Players.Add(new EloPlayer(individual.IndividualId, (decimal)rating, (int)kFactor));
                }
                else
                {
                    Players.Add(new EloPlayer(individual.IndividualId, DefaultRating, startKFactor));
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
                    throw new System.Exception("Elo requires exactly two teams with one individual in each matchFitness");
                }

                // Check if any matchFitness team has more than one individual
                if (matchFitness.TeamFitnesses[0].IndividualFitness.Select(x => x.IndividualID).Distinct().ToList().Count != 1 || matchFitness.TeamFitnesses[1].IndividualFitness.Select(x => x.IndividualID).Distinct().ToList().Count != 1)
                {
                    throw new System.Exception("Elo requires exactly one individual in each team");
                }

                decimal[] matchResult = GetMatchResult(matchFitness.GetTeamFitnesses());

                // 1. Get players
                EloPlayer playerA = GetPlayer(matchFitness.TeamFitnesses[0].IndividualFitness[0].IndividualID);
                EloPlayer playerB = GetPlayer(matchFitness.TeamFitnesses[1].IndividualFitness[0].IndividualID);

                // 2. Calculate new ratings
                decimal[] result = EloCalculator.CalculateElo(playerA.Rating, playerB.Rating, matchResult[0], matchResult[1], playerA.KFactor, playerB.KFactor);

                // 3. Update ratings
                playerA.UpdateRating(result[0]);
                playerB.UpdateRating(result[1]);

                // 4. Add matchFitness results
                playerA.AddIndividualMatchResult(matchFitness.MatchName, matchFitness.TeamFitnesses[0].IndividualFitness[0], new int[] { matchFitness.TeamFitnesses[1].IndividualFitness[0].IndividualID });
                playerB.AddIndividualMatchResult(matchFitness.MatchName, matchFitness.TeamFitnesses[1].IndividualFitness[0], new int[] { matchFitness.TeamFitnesses[0].IndividualFitness[0].IndividualID });

                // 5. Update K factors
                playerA.UpdateKFactor(KFactorBellow2100, KFactorBetween2100And2400, KFactorAbove2400);
                playerB.UpdateKFactor(KFactorBellow2100, KFactorBetween2100And2400, KFactorAbove2400);
            }
        }

        public override RatingSystemRating[] GetFinalRatings()
        {
            RatingSystemRating[] ratings = new RatingSystemRating[Players.Count];

            var i = 0;
            foreach (EloPlayer ratingPlayer in Players.OfType<EloPlayer>())
            {
                ratings[i] = new RatingSystemRating(Players[i].IndividualID, Players[i].IndividualMatchResults, new Dictionary<string, double> { { "Rating", (double)ratingPlayer.Rating }, { "KFactor", ratingPlayer.KFactor } });
                i++;
            }

            return ratings;
        }

        public EloPlayer GetPlayer(int id)
        {
            var player = Players.FirstOrDefault(p => p.IndividualID.Equals(id) && p is EloPlayer);
            return player as EloPlayer;
        }

        private decimal[] GetMatchResult(float[] teamFitnesses)
        {
            // 1 win, 0.5 draw, 0 loss
            if (teamFitnesses[0] < teamFitnesses[1])
            {
                return new decimal[] { 1, 0 };
            }
            else if (teamFitnesses[0] > teamFitnesses[1])
            {
                return new decimal[] { 0, 1 };
            }
            else
            {
                return new decimal[] { 0.5m, 0.5m };
            }
        }

        private void SetStartKFactor()
        {
            if (DefaultRating < 2100)
            {
                startKFactor = KFactorBellow2100;
            }
            else if (DefaultRating < 2400)
            {
                startKFactor = KFactorBetween2100And2400;
            }
            else
            {
                startKFactor = KFactorAbove2400;
            }
        }

        private int GetKFactor(double rating)
        {
            if (rating < 2100)
            {
                return KFactorBellow2100;
            }
            else if (rating < 2400)
            {
                return KFactorBetween2100And2400;
            }
            else
            {
                return KFactorAbove2400;
            }
        }
    }

    public class EloPlayer : CompetitionPlayer
    {
        public decimal Rating { get; set; }
        public int KFactor { get; set; }

        public EloPlayer(int individualId, decimal initialRating, int kFactor)
            : base(individualId)
        {
            Rating = initialRating;
            KFactor = kFactor;
        }

        public void UpdateRating(decimal newRating)
        {
            Rating = newRating;
        }

        /// <summary>
        /// Update K factor based on FIDE rules
        /// </summary>
        public void UpdateKFactor(int KFactorBellow2100, int kFactorBetween2100And2400, int kFactorAbove2400)
        {
            if (Math.Abs(Rating) < 2100)
            {
                KFactor = KFactorBellow2100;
            }

            if ((Math.Abs(Rating) >= 2100 && Math.Abs(Rating) < 2400) || IndividualMatchResults.Count == 30)
            {
                KFactor = kFactorBetween2100And2400;
            }

            if (Math.Abs(Rating) > 2400)
            {
                KFactor = kFactorAbove2400;
            }
        }

        public override void DisplayScore()
        {
            UnityEngine.Debug.Log($"Player {IndividualID}: Rating: {Rating}, K Factor: {KFactor}");
        }

        public override double GetScore()
        {
            return (double)Rating;
        }
    }
}