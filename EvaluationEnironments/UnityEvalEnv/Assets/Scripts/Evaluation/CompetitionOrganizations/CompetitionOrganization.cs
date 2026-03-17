using AgentOrganizations;
using Base;
using Evaluators.CompetitionOrganizations;
using Fitnesses;
using Google.Protobuf.WellKnownTypes;
using Moserware.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Utils;

namespace Evaluators.CompetitionOrganizations
{
    public abstract class CompetitionOrganization
    {
        public CompetitionTeamOrganizator TeamOrganizator { get; set; }
        public bool CreateNewTeamsEachRound { get; set; } = false;
        public Individual[][] Individuals { get; set; }
        public int TeamsPerMatch { get; set; } = 2; // Default is 2 (1v1 matches)

        public CompetitionTeam[][] Teams { get; set; }
        public List<CompetitionTeam> AllTeams { get; private set; }
        public Dictionary<int, CompetitionTeam> TeamLookup { get; private set; }

        public int Rounds { get; set; }
        public int ExecutedRounds { get; set; }
        public List<MatchFitness> PlayedMatches { get; set; }

        protected TeamFitness teamFitnessRes1;
        protected TeamFitness teamFitnessRes2;

        protected float teamFitness1;
        protected float teamFitness2;

        public CompetitionOrganization(CompetitionTeamOrganizator teamOrganizator, Individual[][] individuals, bool regenerateTeamsEachRound, int teamsPerMatch = 2)
        {
            TeamOrganizator = teamOrganizator;
            Individuals = individuals;
            CreateNewTeamsEachRound = regenerateTeamsEachRound;
            TeamsPerMatch = teamsPerMatch < 2 ? 2 : teamsPerMatch; // Minimum is 2 (1v1 matches)
            OrganizeTeams(null);
        }

        public abstract Match[] GenerateCompetitionMatches();

        public void OrganizeTeams(CompetitionPlayer[][] ratingPlayers)
        {
            Teams = TeamOrganizator.OrganizeTeams(Individuals, ratingPlayers);

            BuildTeamLookup();
        }

        public virtual void UpdateTeamsScore(List<MatchFitness> competitionMatchFitnesses, List<CompetitionPlayer> players = null)
        {
            var competitionMatchFitnessesCopy = new List<MatchFitness>(competitionMatchFitnesses);

            // Add played matches (ignore dummy matches)
            PlayedMatches.AddRange(competitionMatchFitnessesCopy.Where(mf => !mf.IsDummy));

            var matchFitnesses = new List<MatchFitness>();
            var matchFitnessesSwapped = new List<MatchFitness>();

            while (competitionMatchFitnessesCopy.Count > 0)
            {
                // 1. Get match data
                var matchFitness = new MatchFitness();
                MatchFitness.GetMatchFitness(
                    competitionMatchFitnessesCopy,
                    matchFitness,
                    matchFitnesses,
                    matchFitnessesSwapped,
                    Coordinator.Instance.SwapCompetitionMatchTeams
                );

                if (matchFitness.IsDummy)
                {
                    // Bye -> award fixed points ( = TeamsPerMatch)
                    foreach (var tf in matchFitness.TeamFitnesses)
                        TeamLookup[tf.TeamID].Score += TeamsPerMatch;

                    continue;
                }

                // 2. Collect team results for this match
                int[] ranking = GetTeamOrders(matchFitness.TeamFitnesses);

                // Point schemes
                int[] points = Enumerable.Range(0, TeamsPerMatch).Select(i => 2 * (TeamsPerMatch - ranking[i])).ToArray(); ;

                // 3. Assign points
                for (int i = 0; i < matchFitness.TeamFitnesses.Count; i++)
                {
                    var tf = matchFitness.TeamFitnesses[i];
                    var team = TeamLookup[tf.TeamID];

                    team.Score += (i < points.Length ? points[i] : 0);

                    // 4. Record individual match results
                    var opponents = matchFitness.TeamFitnesses
                       .Where(x => x.TeamID != tf.TeamID)
                       .SelectMany(x => TeamLookup[x.TeamID]
                           .Individuals
                           .Select(ind => ind.IndividualId))
                       .ToArray();

                    team.IndividualMatchResults.Add(new IndividualMatchResult()
                    {
                        MatchName = matchFitness.MatchName,
                        OpponentsIDs = opponents,
                        Value = tf.GetTeamFitness(),
                        IndividualValues = tf.GetTeamIndividualValues()
                    });
                }
            }

            // Increment the number of executed rounds
            ExecutedRounds++;
        }

        public virtual bool IsCompetitionFinished()
        {
            return ExecutedRounds >= Rounds;
        }

        public void DisplayStandings()
        {
            DebugSystem.LogDetailed("Standings:");
            foreach (var team in AllTeams)
            {
                DebugSystem.LogDetailed($"{team.GetTeamName()} - {team.Score} points");
            }
        }

        public void AddTeams(List<CompetitionTeam> teams)
        {
            Teams.AddRange(teams);
        }

        public int[] GetTeamOrders(List<TeamFitness> teamFitnesses)
        {
            int[] indices = Enumerable.Range(0, teamFitnesses.Count)
                                      .OrderBy(i => teamFitnesses[i].GetTeamFitness()) // Ascending order
                                      .ToArray();
            int[] orders = new int[teamFitnesses.Count];
            int rank = 1; // Start with rank 1

            for (int i = 0; i < indices.Length; i++)
            {
                int originalIndex = indices[i];

                orders[originalIndex] = rank;

                if (i < indices.Length - 1 && teamFitnesses[indices[i]].GetTeamFitness() == teamFitnesses[indices[i + 1]].GetTeamFitness())
                {
                    // If tied, do not increment rank
                    continue;
                }

                rank = i + 2;
            }

            return orders;
        }

        private void BuildTeamLookup()
        {
            AllTeams = Teams.SelectMany(g => g).ToList();
            TeamLookup = AllTeams.ToDictionary(t => t.TeamId);
        }
    }

    public enum CompetitionOrganizationType
    {
        RoundRobin = 0, // Tournament where each team plays against every other team
        SwissSystem = 1, // Tournament where teams are paired based on their current score
        LastVsAll = 2, // Special competition for the creation of convergence graph
        SingleElimination = 3, // Tournament where the loser of each match is immediately eliminated from the tournament
        DoubleElimination= 4, // Tournament where a team is not eliminated until it has lost two matches
        KRandomOpponents = 5, // Competition where each team plays K random opponents
        SimilarStrengthOpponentSelection = 6, // Competition where teams are paired based on similar strength (score)
        MatrixFactorizationInteractionScheme = 7, // Competition where teams are paired based on a matrix factorization of past match results, to predict the most informative matches (e.g. for active learning)
        BestPreviousCompetitors = 8, // Competition where teams are paired against best previous competitors from last X generations
    }
}