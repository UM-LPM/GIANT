using Fitnesses;
using System.Collections.Generic;
using AgentOrganizations;
using System.Linq;
using Unity.VisualScripting;
using Google.Protobuf.WellKnownTypes;
using System;
using Base;
using Evaluators.CompetitionOrganizations;

namespace Evaluators.CompetitionOrganizations
{
    public abstract class CompetitionOrganization
    {
        public CompetitionTeamOrganizator TeamOrganizator { get; set; }
        public bool CreateNewTeamsEachRound { get; set; } = false;
        public Individual[] Individuals { get; set; }

        public List<CompetitionTeam> Teams { get; set; }
        public int Rounds { get; set; }
        public int ExecutedRounds { get; set; }
        public List<MatchFitness> PlayedMatches { get; set; }

        protected TeamFitness teamFitnessRes1;
        protected TeamFitness teamFitnessRes2;

        protected float teamFitness1;
        protected float teamFitness2;

        public CompetitionOrganization(CompetitionTeamOrganizator teamOrganizator, Individual[] individuals, bool regenerateTeamsEachRound)
        {
            TeamOrganizator = teamOrganizator;
            Individuals = individuals;
            CreateNewTeamsEachRound = regenerateTeamsEachRound;
            OrganizeTeams(null);
        }

        public abstract Match[] GenerateCompetitionMatches();

        public void OrganizeTeams(CompetitionPlayer[] ratingPlayers)
        {
            Teams = TeamOrganizator.OrganizeTeams(Individuals, ratingPlayers);
        }

        public virtual void UpdateTeamsScore(List<MatchFitness> competitionMatchFitnesses, List<CompetitionPlayer> players = null)
        {
            List<MatchFitness> competitionMatchFitnessesCopy = new List<MatchFitness>(competitionMatchFitnesses);
            // Add played CompetitionMatches to the list of played CompetitionMatches (add only matchFitnesses that are not dummy)
            PlayedMatches.AddRange(competitionMatchFitnessesCopy.FindAll(matchFitness => !matchFitness.IsDummy));

            MatchFitness matchFitness;
            List<MatchFitness> matchFitnesses = new List<MatchFitness>();
            List<MatchFitness> matchFitnessesSwaped = new List<MatchFitness>();
            while (competitionMatchFitnessesCopy.Count > 0)
            {
                // 1. Get all match data
                matchFitness = new MatchFitness();
                MatchFitness.GetMatchFitness(competitionMatchFitnessesCopy, matchFitness, matchFitnesses, matchFitnessesSwaped, Coordinator.Instance.SwapCompetitionMatchTeams);

                // 2. Update teams score
                teamFitnessRes1 = matchFitness.TeamFitnesses[0];
                teamFitnessRes2 = matchFitness.TeamFitnesses[1];

                teamFitness1 = teamFitnessRes1.GetTeamFitness();
                teamFitness2 = teamFitnessRes2.GetTeamFitness();

                var team1 = Teams.Find(team => team.TeamId == teamFitnessRes1.TeamID);
                var team2 = Teams.Find(team => team.TeamId == teamFitnessRes2.TeamID);

                if (matchFitness.IsDummy)
                {
                    team1.Score += 2;
                    continue;
                }

                if (teamFitness1 < teamFitness2)
                {
                    team1.Score += 2;
                }
                else if (teamFitness1 > teamFitness2)
                {
                    team2.Score += 2;
                }
                else
                {
                    team1.Score += 1;
                    team2.Score += 1;
                }

                // 3. Add individual match results to the teams
                team1.IndividualMatchResults.Add(new IndividualMatchResult()
                {
                    MatchName = matchFitness.MatchName,
                    OpponentsIDs = team2.Individuals.Select(individual => individual.IndividualId).ToArray(),
                    Value = teamFitness1,
                    IndividualValues = teamFitnessRes1.GetTeamIndividualValues()
                });

                team2.IndividualMatchResults.Add(new IndividualMatchResult()
                {
                    MatchName = matchFitness.MatchName,
                    OpponentsIDs = team1.Individuals.Select(individual => individual.IndividualId).ToArray(),
                    Value = teamFitness2,
                    IndividualValues = teamFitnessRes2.GetTeamIndividualValues()
                });
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
            UnityEngine.Debug.Log("Standings:");
            foreach (var team in Teams)
            {
                UnityEngine.Debug.Log($"{team.GetTeamName()} - {team.Score} points");
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