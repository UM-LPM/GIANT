using AgentOrganizations;
using Evaluators.TournamentOrganizations;
using Fitnesses;
using Moserware.Skills;
using System.Collections.Generic;
using Glicko2;
using System.Linq;
using System;
using Unity.Mathematics;

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
            throw new System.NotImplementedException();
        }

        public override void DisplayRatings()
        {
            throw new System.NotImplementedException();
        }

        public override RatingSystemRating[] GetFinalRatings()
        {
            throw new System.NotImplementedException();
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