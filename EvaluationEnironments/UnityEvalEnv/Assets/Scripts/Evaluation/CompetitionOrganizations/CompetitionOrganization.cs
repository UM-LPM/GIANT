using Fitnesses;
using System.Collections.Generic;
using AgentOrganizations;
using System.Linq;
using Unity.VisualScripting;
using Google.Protobuf.WellKnownTypes;
using System;
using Base;
using Evaluators.CompetitionOrganizations;
using Utils;

namespace Evaluators.CompetitionOrganizations
{
    public abstract class CompetitionOrganization
    {
        public CompetitionTeamOrganizator TeamOrganizator { get; set; }
        public bool CreateNewTeamsEachRound { get; set; } = false;
        public Individual[] Individuals { get; set; }
        public int TeamsPerMatch { get; set; } = 2; // Default is 2 (1v1 matches)

        public List<CompetitionTeam> Teams { get; set; }
        public int Rounds { get; set; }
        public int ExecutedRounds { get; set; }
        public List<MatchFitness> PlayedMatches { get; set; }

        protected TeamFitness teamFitnessRes1;
        protected TeamFitness teamFitnessRes2;

        protected float teamFitness1;
        protected float teamFitness2;

        public CompetitionOrganization(CompetitionTeamOrganizator teamOrganizator, Individual[] individuals, bool regenerateTeamsEachRound, int teamsPerMatch = 2)
        {
            TeamOrganizator = teamOrganizator;
            Individuals = individuals;
            CreateNewTeamsEachRound = regenerateTeamsEachRound;
            TeamsPerMatch = teamsPerMatch < 2 ? 2 : teamsPerMatch; // Minimum is 2 (1v1 matches)
            OrganizeTeams(null);
        }

        public abstract Match[] GenerateCompetitionMatches();

        public void OrganizeTeams(CompetitionPlayer[] ratingPlayers)
        {
            Teams = TeamOrganizator.OrganizeTeams(Individuals, ratingPlayers);
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
                    {
                        var team = Teams.Find(t => t.TeamId == tf.TeamID);
                        if (team != null)
                            team.Score += TeamsPerMatch;
                    }
                    continue;
                }

                // 2. Collect team results for this match
                int[] ranking = GetTeamOrders(matchFitness.TeamFitnesses);

                // Point schemes
                int[] pointsScheme = Enumerable.Range(0, TeamsPerMatch).Select(i => 2 * (TeamsPerMatch - ranking[i])).ToArray(); ;

                // 3. Assign points
                for (int i = 0; i < matchFitness.TeamFitnesses.Count; i++)
                {
                    var team = Teams.Find(t => t.TeamId == matchFitness.TeamFitnesses[i].TeamID);
                    int points = i < pointsScheme.Length ? pointsScheme[i] : 0;
                    Teams.Find(t => t.TeamId == team.TeamId).Score += points;

                    // 4. Record individual match results
                    var opponents = Teams
                        .Where(x => x.TeamId != team.TeamId)
                        .SelectMany(x => x.Individuals.Select(ind => ind.IndividualId))
                        .ToArray();

                    team.IndividualMatchResults.Add(new IndividualMatchResult()
                    {
                        MatchName = matchFitness.MatchName,
                        OpponentsIDs = opponents,
                        Value = matchFitness.TeamFitnesses[i].GetTeamFitness(),
                        IndividualValues = matchFitness.TeamFitnesses[i].GetTeamIndividualValues()
                    });
                }
            }
            // Increment the number of executed rounds
            ExecutedRounds++;
        }

        public abstract void ResetCompetition();

        public virtual bool IsCompetitionFinished()
        {
            return ExecutedRounds >= Rounds;
        }

        public void DisplayStandings()
        {
            DebugSystem.LogDetailed("Standings:");
            foreach (var team in Teams)
            {
                DebugSystem.LogDetailed($"{team.GetTeamName()} - {team.Score} points");
            }
        }

        public void ClearTeams()
        {
            Teams.Clear();
        }

        public void AddTeam(CompetitionTeam team)
        {
            Teams.Add(team);
        }

        public void AddTeams(List<CompetitionTeam> teams)
        {
            Teams.AddRange(teams);
        }

        public void SetTeams(List<CompetitionTeam> teams)
        {
            Teams = teams;
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
    }

    public enum CompetitionOrganizationType
    {
        RoundRobin, // Tournament where each team plays against every other team
        SwissSystem, // Tournament where teams are paired based on their current score
        LastVsAll, // Special competition for the creation of convergence graph
        SingleElimination, // Tournament where the loser of each match is immediately eliminated from the tournament
        DoubleElimination, // Tournament where a team is not eliminated until it has lost two matches
        KRandomOpponents, // Competition where each team plays K random opponents
        SimilarStrengthOpponentSelection // Competition where teams are paired based on similar strength (score)
    }
}