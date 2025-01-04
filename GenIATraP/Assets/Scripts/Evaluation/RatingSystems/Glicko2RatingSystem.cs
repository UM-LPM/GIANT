using AgentOrganizations;
using Evaluators.TournamentOrganizations;
using Fitnesses;
using Moserware.Skills;
using System.Collections.Generic;
using Glicko2;
using System.Linq;
using System;
using Unity.Mathematics;
using Kezyma.EloRating;

namespace Evaluators.RatingSystems
{
    public class Glicko2RatingSystem : RatingSystem
    {
        public List<Glicko2Player> Players;

        public double DefaultRating;
        public double DefaultRatingDeviation;
        public double DefaultVolatility;

        public Glicko2RatingSystem(double defaultRating = 1500, double defaultRatingDeviation = 350, double defaultVolatility = 0.06)
        {
            Players = new List<Glicko2Player>();
            DefaultRating = defaultRating;
            DefaultRatingDeviation = defaultRatingDeviation;
            DefaultVolatility = defaultVolatility;
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

                        if (individualRating != null)
                        {
                            // TODO Add support for initial ratings
                            /*if (initialPlayerRaitings != null && ((!double.IsInfinity(individualRating.Mean) && !double.IsInfinity(individualRating.StandardDeviation)) && (double.MaxValue != individualRating.Mean && double.MaxValue != individualRating.StandardDeviation)))
                            {
                                Players.Add(new TrueSkillPlayer(individual.IndividualId, new Player(individual.IndividualId), new Rating(math.abs(individualRating.Mean), individualRating.StandardDeviation)));
                            }
                            else if (initialPlayerRaitings != null && ((!double.IsInfinity(individualRating.Mean) && double.IsInfinity(individualRating.StandardDeviation))))
                            {
                                Players.Add(new TrueSkillPlayer(individual.IndividualId, new Player(individual.IndividualId), new Rating(math.abs(individualRating.Mean), GameInfo.DefaultRating.StandardDeviation)));
                            }
                            else
                            {
                                Players.Add(new TrueSkillPlayer(individual.IndividualId, new Player(individual.IndividualId), GameInfo.DefaultRating));
                            }*/
                            throw new NotImplementedException();
                        }
                        else
                        {
                            Players.Add(new Glicko2Player(individual.IndividualId, DefaultRating, DefaultRatingDeviation, DefaultVolatility));
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
                    throw new System.Exception("Glicko2 requires exactly two teams with one individual in each match");
                }

                // Check if any match team has more than one individual
                if (match.TeamFitnesses[0].IndividualFitness.Count != 1 || match.TeamFitnesses[1].IndividualFitness.Count != 1)
                {
                    throw new System.Exception("Glicko2 requires exactly one individual in each team");
                }

                // 1. Get players
                Glicko2Player playerA = GetPlayer(match.TeamFitnesses[0].IndividualFitness[0].IndividualID);
                Glicko2Player playerB = GetPlayer(match.TeamFitnesses[1].IndividualFitness[0].IndividualID);

                // 2. Calculate new ratings & update players
                float[] matchResult = GetMatchResult(match.GetTeamFitnesses());

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

                // 3. Add match results
                playerA.AddIndividualMatchResult(match.MatchName, match.TeamFitnesses[0].IndividualFitness[0], new int[] { match.TeamFitnesses[1].IndividualFitness[0].IndividualID });
                playerB.AddIndividualMatchResult(match.MatchName, match.TeamFitnesses[1].IndividualFitness[0], new int[] { match.TeamFitnesses[0].IndividualFitness[0].IndividualID });
            }
        }

        public override void DisplayRatings()
        {
            List<Glicko2Player> playersSorted = new List<Glicko2Player>(Players);
            playersSorted.Sort((player1, player2) => player2.Player.Rating.CompareTo(player1.Player.Rating));

            foreach (Glicko2Player player in playersSorted)
            {
                UnityEngine.Debug.Log("Player: " + player.IndividualID + " Rating: " + player.Player.Rating + " RD: " + player.Player.RatingDeviation + " Volatility: " + player.Player.Volatility);
            }
        }

        public override RatingSystemRating[] GetFinalRatings()
        {
            RatingSystemRating[] ratings = new RatingSystemRating[Players.Count];
            for (int i = 0; i < ratings.Length; i++)
            {
                // TODO Here also volatility should be added
                ratings[i] = new RatingSystemRating(Players[i].IndividualID, Players[i].Player.Rating, Players[i].Player.RatingDeviation, Players[i].IndividualMatchResults);
            }

            return ratings;
        }

        public Glicko2Player GetPlayer(int id)
        {
            return Players.Find(player => player.IndividualID.Equals(id));
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

    public class Glicko2Player
    {
        public int IndividualID { get; set; }
        public GlickoPlayer Player { get; set; }

        public List<IndividualMatchResult> IndividualMatchResults { get; set; }

        public Glicko2Player(int IndividualId, double rating, double ratingDeviation, double volatility)
        {
            IndividualID = IndividualId;
            Player = new GlickoPlayer(rating, ratingDeviation, volatility);
            IndividualMatchResults = new List<IndividualMatchResult>();
        }

        public void UpdateRating(GlickoPlayer player)
        {
            Player = player;
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