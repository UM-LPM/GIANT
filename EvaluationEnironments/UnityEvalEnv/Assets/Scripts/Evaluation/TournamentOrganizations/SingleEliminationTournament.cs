using AgentOrganizations;
using Base;
using Evaluators.RatingSystems;
using Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluators.TournamentOrganizations
{
    public class SingleEliminationTournament : TournamentOrganization
    {
        private int TeamsWhoGotBye;

        public List<TournamentTeam> EliminatedTeams = new List<TournamentTeam>();
        public List<Match> TournamentMatches = new List<Match>();

        List<TournamentTeam> UnpairedTeams = new List<TournamentTeam>();
        int CurrentMatchID;

        public SingleEliminationTournament(TournamentTeamOrganizator teamOrganizator, Individual[] individuals, bool regenerateTeamsEachRound, int rounds)
            : base(teamOrganizator, individuals, regenerateTeamsEachRound)
        {
            Rounds = rounds < 1 ? (int)Math.Ceiling(Math.Log(Teams.Count, 2)) : rounds;
            ExecutedRounds = 0;
            PlayedMatches = new List<MatchFitness>();
        }

        public override void ResetTournament()
        {
            Teams.Clear();
            ExecutedRounds = 0;
            PlayedMatches.Clear();
            TeamsWhoGotBye = 0;
            EliminatedTeams.Clear();
        }

        public override Match[] GenerateTournamentMatches()
        {
            if (IsTournamentFinished())
                return new Match[] { };
            if (TeamsWhoGotBye == Teams.Count)
            {
                ResetTeamByes();
            }

            // Get all Teams that were yet not eliminated
            List<TournamentTeam> teams = new List<TournamentTeam>(Teams);
            teams.RemoveAll(team => EliminatedTeams.Contains(team));

            return PairTeams(teams);
        }

        private Match[] PairTeams(List<TournamentTeam> teams)
        {
            TournamentMatches.Clear();
            UnpairedTeams = new List<TournamentTeam>(teams);
            CurrentMatchID = 0;

            // If there's an odd number of players, one player gets a bye
            if (UnpairedTeams.Count % 2 != 0)
            {
                TournamentTeam byeTeam = UnpairedTeams.FirstOrDefault(p => !p.HasBye);
                if (byeTeam != null)
                {
                    byeTeam.HasBye = true;
                    TeamsWhoGotBye++;
                    UnpairedTeams.Remove(byeTeam);
                    TournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(CurrentMatchID++, new Team[] { byeTeam, ScriptableObject.CreateInstance<TournamentTeam>().Initialize(-1, "Dummy", new Individual[] { }) })); // Add a bye pairing with dummy team
                }
            }

            // Pair remaining players
            while (UnpairedTeams.Count > 1)
            {
                TournamentTeam t1 = UnpairedTeams[0];
                UnpairedTeams.RemoveAt(0);

                TournamentTeam t2 = UnpairedTeams[0];
                UnpairedTeams.RemoveAt(0);

                if (Coordinator.Instance.Random.NextDouble() > 0.5)
                    TournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(CurrentMatchID++, new Team[] { t1, t2 }));
                else
                    TournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(CurrentMatchID++, new Team[] { t2, t1 }));
            }

            // If enabled: For each match that already exists, add another match with the teams swapped
            if (Coordinator.Instance.SwapTournamentMatchTeams)
            {
                List<Match> matchesSwapped = new List<Match>();
                for (int i = 0; i < TournamentMatches.Count; i++)
                {
                    Match match = TournamentMatches[i];
                    matchesSwapped.Add(ScriptableObject.CreateInstance<Match>().Initialize(CurrentMatchID++, new Team[] { match.Teams[1], match.Teams[0] }));
                }

                TournamentMatches.AddRange(matchesSwapped);
            }

            return TournamentMatches.ToArray();
        }

        public override void UpdateTeamsScore(List<MatchFitness> tournamentMatchFitnesses, List<RatingPlayer> players = null)
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
                    EliminatedTeams.Add(team2);
                }
                else if (teamFitness1 > teamFitness2)
                {
                    team2.Score += 2;
                    EliminatedTeams.Add(team1);
                }
                else
                {
                    // Random choose the winner if the scores are equal
                    if (Coordinator.Instance.Random.NextDouble() > 0.5)
                    {
                        team1.Score += 2;
                        EliminatedTeams.Add(team2);
                    }
                    else
                    {
                        team2.Score += 2;
                        EliminatedTeams.Add(team1);
                    }
                }

                // Add individual match results to the teams
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

        public override bool IsTournamentFinished()
        {
            if (EliminatedTeams.Count == Teams.Count - 1)
                return true;
            else
                return false;
        }

        private void ResetTeamByes()
        {
            foreach (var team in Teams)
            {
                team.HasBye = false;
            }
            TeamsWhoGotBye = 0;
        }
    }
}