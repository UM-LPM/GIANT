using Evaluators.TournamentOrganizations;
using System.Collections.Generic;
using Moserware.Skills;
using Moserware.Skills.TrueSkill;
using Unity.Mathematics;
using System;
using AgentOrganizations;
using Fitnesses;
using System.Linq;

namespace Evaluators.RatingSystems
{
    public class TrueSkillRatingSystem : RatingSystem
    {
        public List<TrueSkillPlayer> Players;
        public GameInfo GameInfo;
        public SkillCalculator SkillCalculator;
        public float MinRating;
        public float MaxRating;

        public TrueSkillRatingSystem()
        {
            Players = new List<TrueSkillPlayer>();
            GameInfo = GameInfo.DefaultGameInfo;
            SkillCalculator = new TwoTeamTrueSkillCalculator();
            MinRating = 0;
            MaxRating = 100;
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
                        if (initialPlayerRaitings != null && ((!double.IsInfinity(individualRating.Mean) && !double.IsInfinity(individualRating.StandardDeviation)) && (double.MaxValue != individualRating.Mean && double.MaxValue != individualRating.StandardDeviation)))
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

                if(match.TeamFitnesses.Count < 2)
                {
                    throw new System.Exception("TrueSkillRatingSystem requires at least two teams in each match");
                }

                // 1. Create teams 
                Moserware.Skills.Team[] teams = new Moserware.Skills.Team[match.TeamFitnesses.Count];

                for (int i = 0; i < match.TeamFitnesses.Count; i++)
                {
                    List<TrueSkillPlayer> teamPlayers = new List<TrueSkillPlayer>();

                    foreach (IndividualFitness player in match.TeamFitnesses[i].IndividualFitness)
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

                // 2. Define order ranking
                int[] orderRanking = GetFitnessOrder(match.GetTeamFitnesses());

                // 3. Calculate new ratings
                var teamsConcatinated = Teams.Concat(teams);
                var newRatings = SkillCalculator.CalculateNewRatings(GameInfo, teamsConcatinated, orderRanking);

                // 4. Update ratings
                for (int i = 0; i < match.TeamFitnesses.Count; i++)
                {
                    for (int j = 0; j < match.TeamFitnesses[i].IndividualFitness.Count; j++)
                    {
                        TrueSkillPlayer trueSkillPlayer = GetPlayer(match.TeamFitnesses[i].IndividualFitness[j].IndividualID);
                        trueSkillPlayer.UpdateRating(newRatings[GetPlayer(match.TeamFitnesses[i].IndividualFitness[j].IndividualID).Player]);
                        
                        // Get opponentIDs
                        List<int> opponentIDs = new List<int>();
                        for (int k = 0; k < match.TeamFitnesses.Count; k++)
                        {
                            if (k != i)
                            {
                                foreach (IndividualFitness individualFitness in match.TeamFitnesses[k].IndividualFitness)
                                {
                                    if ((!opponentIDs.Contains(individualFitness.IndividualID)) && (individualFitness.IndividualID != match.TeamFitnesses[i].IndividualFitness[j].IndividualID))
                                        opponentIDs.Add(individualFitness.IndividualID);
                                }
                            }
                        }

                        trueSkillPlayer.AddIndividualMatchResult(match.MatchName, match.TeamFitnesses[i].IndividualFitness[j], opponentIDs.ToArray());
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
            return Players.Find(player => player.Player.Id.Equals(id));
        }

        public override void DisplayRatings()
        {
            List<TrueSkillPlayer> playersSorted = new List<TrueSkillPlayer>(Players);
            playersSorted.Sort((player1, player2) => player2.Rating.Mean.CompareTo(player1.Rating.Mean));

            foreach (TrueSkillPlayer player in playersSorted)
            {
                UnityEngine.Debug.Log($"Player {player.Player.Id}: {player.Rating}");
            }
        }

        public override RatingSystemRating[] GetFinalRatings()
        {
            RatingSystemRating[] ratings = new RatingSystemRating[Players.Count];
            for (int i = 0; i < ratings.Length; i++)
            {
                ratings[i] = new RatingSystemRating(Players[i].IndividualID, Players[i].Rating.Mean, Players[i].Rating.StandardDeviation, Players[i].IndividualMatchResults);
            }

            return ratings;
        }
    }

    public class TrueSkillPlayer
    {
        public int IndividualID { get; set; }
        public Player Player { get; set; }
        public Rating Rating { get; set; }

        public List<IndividualMatchResult> IndividualMatchResults { get; set; }

        public TrueSkillPlayer(int IndividualId, Player player, Rating rating)
        {
            IndividualID = IndividualId;
            Player = player;
            Rating = rating;
            IndividualMatchResults = new List<IndividualMatchResult>();
        }

        public void UpdateRating(Rating newRating)
        {
            Rating = newRating;
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