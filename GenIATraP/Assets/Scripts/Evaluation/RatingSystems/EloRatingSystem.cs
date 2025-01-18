using AgentOrganizations;
using Evaluators.TournamentOrganizations;
using Fitnesses;
using Kezyma.EloRating;
using Moserware.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace Evaluators.RatingSystems
{
    public class EloRatingSystem : RatingSystem
    {
        public List<EloPlayer> Players;

        public decimal DefaultRating;
        public int KFactorBellow2100;
        public int KFactorBetween2100And2400;
        public int KFactorAbove2400;

        private int startKFactor;

        public EloRatingSystem(decimal defaultRating = 1000, int kFactorBellow2100 = 40, int kFactorBetween2100And2400 = 20, int kFactorAbove2400 = 10)
        {
            Players = new List<EloPlayer>();
            DefaultRating = defaultRating;
            KFactorBellow2100 = kFactorBellow2100;
            KFactorBetween2100And2400 = kFactorBetween2100And2400;
            KFactorAbove2400 = kFactorAbove2400;

            // Set the start K factor
            SetStartKFactor();
        }

        public override void DefinePlayers(List<TournamentTeam> teams, RatingSystemRating[] initialPlayerRaitings)
        {
            // Find unique tournament individuals in teams and add them to the list of individuals
            List<Individual> individuals = new List<Individual>();
            foreach (TournamentTeam team in teams)
            {
                foreach (Individual individual in team.Individuals)
                {
                    if (!individuals.Contains(individual))
                    {
                        individuals.Add(individual);
                        if (initialPlayerRaitings != null && initialPlayerRaitings.Length < individuals.Count)
                        {
                            throw new Exception("Initial individual rating array is not the same size as the number of individuals in the tournament");
                        }

                        RatingSystemRating individualRating = initialPlayerRaitings?.FirstOrDefault(x => x.IndividualID == individual.IndividualId);

                        if(individualRating != null && individualRating.AdditionalValues != null)
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
            }
        }

        public override void UpdateRatings(List<MatchFitness> tournamentMatchFitnesses)
        {
            foreach (MatchFitness match in tournamentMatchFitnesses)
            {
                // If the match is a dummy match, skip it (this match is used for teams who got bye on a tournament
                if (match.IsDummy)
                    continue;

                if (match.TeamFitnesses.Count != 2)
                {
                    throw new System.Exception("Elo requires exactly two teams with one individual in each match");
                }

                // Check if any match team has more than one individual
                if (match.TeamFitnesses[0].IndividualFitness.Count != 1 || match.TeamFitnesses[1].IndividualFitness.Count != 1)
                {
                    throw new System.Exception("Elo requires exactly one individual in each team");
                }

                decimal[] matchResult = GetMatchResult(match.GetTeamFitnesses());

                // 1. Get players
                EloPlayer playerA = GetPlayer(match.TeamFitnesses[0].IndividualFitness[0].IndividualID);
                EloPlayer playerB = GetPlayer(match.TeamFitnesses[1].IndividualFitness[0].IndividualID);

                // 2. Calculate new ratings
                decimal[] result = EloCalculator.CalculateElo(playerA.Rating, playerB.Rating, matchResult[0], matchResult[1], playerA.KFactor, playerB.KFactor);

                // 3. Update ratings
                playerA.UpdateRating(result[0]);
                playerB.UpdateRating(result[1]);

                // 4. Add match results
                playerA.AddIndividualMatchResult(match.MatchName, match.TeamFitnesses[0].IndividualFitness[0], new int[] { match.TeamFitnesses[1].IndividualFitness[0].IndividualID });
                playerB.AddIndividualMatchResult(match.MatchName, match.TeamFitnesses[1].IndividualFitness[0], new int[] { match.TeamFitnesses[0].IndividualFitness[0].IndividualID });

                // 5. Update K factors
                playerA.UpdateKFactor(KFactorBellow2100, KFactorBetween2100And2400, KFactorAbove2400);
                playerB.UpdateKFactor(KFactorBellow2100, KFactorBetween2100And2400, KFactorAbove2400);
            }
        }

        public override void DisplayRatings()
        {
            List<EloPlayer> playersSorted = new List<EloPlayer>(Players);
            playersSorted.Sort((player1, player2) => player2.Rating.CompareTo(player1.Rating));

            foreach (EloPlayer player in playersSorted)
            {
                UnityEngine.Debug.Log($"Player {player.IndividualID}: Rating: {player.Rating},  K Factor:{player.KFactor}");
            }
        }

        public override RatingSystemRating[] GetFinalRatings()
        {
            RatingSystemRating[] ratings = new RatingSystemRating[Players.Count];
            for (int i = 0; i < ratings.Length; i++)
            {
                ratings[i] = new RatingSystemRating(Players[i].IndividualID, Players[i].IndividualMatchResults, new Dictionary<string, double> { { "Rating", (double)Players[i].Rating }, { "KFactor", Players[i].KFactor } });
            }

            return ratings;
        }

        public EloPlayer GetPlayer(int id)
        {
            return Players.Find(player => player.IndividualID.Equals(id));
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

    public class EloPlayer
    {
        public int IndividualID { get; set; }
        public decimal Rating { get; set; }
        public int KFactor { get; set; }
        public List<IndividualMatchResult> IndividualMatchResults { get; set; }
        public int PreviousMatchesPlayed{ get; set; }

        public EloPlayer(int IndividualId, decimal initialRating, int kFactor, int previousMatchesPlayed = 0)
        {
            IndividualID = IndividualId;
            Rating = initialRating;
            KFactor = kFactor;
            IndividualMatchResults = new List<IndividualMatchResult>();
            PreviousMatchesPlayed = previousMatchesPlayed;
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
            if(Math.Abs(Rating) < 2100)
            {
                KFactor = KFactorBellow2100;
            }
            
            if((Math.Abs(Rating) >= 2100 && Math.Abs(Rating) < 2400) || IndividualMatchResults.Count == 30)
            {
                KFactor = kFactorBetween2100And2400;
            }

            if(Math.Abs(Rating) > 2400)
            {
                KFactor = kFactorAbove2400;
            }
        }

        public void AddIndividualMatchResult(string matchName, IndividualFitness individualFitness, int[] opponentIDs)
        {
            IndividualMatchResults.Add(new IndividualMatchResult()
            {
                MatchName = matchName,
                Value = individualFitness.Value,
                IndividualValues = individualFitness.IndividualValues,
                OpponentsIDs = opponentIDs
            }); ;
        }
    }
}