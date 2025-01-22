using Fitnesses;
using System.Collections.Generic;
using AgentOrganizations;
using System.Linq;
using Unity.VisualScripting;
using Google.Protobuf.WellKnownTypes;
using System;
using Base;

namespace Evaluators.TournamentOrganizations
{
    public abstract class TournamentOrganization
    {
        public List<TournamentTeam> Teams { get; set; }
        public int Rounds { get; set; }
        public int ExecutedRounds { get; set; }
        public List<MatchFitness> PlayedMatches { get; set; }

        protected TeamFitness teamFitnessRes1;
        protected TeamFitness teamFitnessRes2;

        protected float teamFitness1;
        protected float teamFitness2;

        public abstract Match[] GenerateTournamentMatches();

        public virtual void UpdateTeamsScore(List<MatchFitness> tournamentMatchFitnesses)
        {
            List<MatchFitness> tournamentMatchFitnessesCopy = new List<MatchFitness>(tournamentMatchFitnesses);
            // Add played TournamentMatches to the list of played TournamentMatches (add only matchFitnesses that are not dummy)
            PlayedMatches.AddRange(tournamentMatchFitnessesCopy.FindAll(matchFitness => !matchFitness.IsDummy));

            MatchFitness matchFitness;
            List<MatchFitness> matchFitnesses = new List<MatchFitness>();
            List<MatchFitness> matchFitnessesSwaped = new List<MatchFitness>();
            while (tournamentMatchFitnessesCopy.Count > 0)
            {
                // 1. Get all match data
                matchFitness = new MatchFitness();
                MatchFitness.GetMatchFitness(tournamentMatchFitnessesCopy, matchFitness, matchFitnesses, matchFitnessesSwaped, Coordinator.Instance.SwapTournamentMatchTeams);

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

        public abstract void ResetTournament();

        public virtual bool IsTournamentFinished()
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

        public void AddTeam(TournamentTeam team)
        {
            Teams.Add(team);
        }

        public void AddTeams(List<TournamentTeam> teams)
        {
            Teams.AddRange(teams);
        }

        public void SetTeams(List<TournamentTeam> teams)
        {
            Teams = teams;
        }
    }

    public enum TournamentOrganizationType
    {
        RoundRobin,
        SwissSystem,
        LastVsAll, // Special Tournament for the creation of convergence graph
        SingleElimination,
        DoubleElimination
    }
}