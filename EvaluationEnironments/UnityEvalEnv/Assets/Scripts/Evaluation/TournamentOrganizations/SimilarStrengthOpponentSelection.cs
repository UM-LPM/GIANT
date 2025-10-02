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
    public class SimilarStrengthOpponentSelection : TournamentOrganization
    {
        private int TeamsWhoGotBye;

        List<Match> tournamentMatches = new List<Match>();
        List<TournamentTeam> unpairedTeams = new List<TournamentTeam>();
        int currentMatchID;
        List<int> opponentTeamIDs;

        public SimilarStrengthOpponentSelection(TournamentTeamOrganizator teamOrganizator, Individual[] individuals, bool regenerateTeamsEachRound, int rounds)
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
        }

        public override Match[] GenerateTournamentMatches()
        {
            if (IsTournamentFinished())
                return new Match[] { };
            if (TeamsWhoGotBye == Teams.Count)
            {
                ResetTeamByes();
            }

            // 1. Sort teams by score
            List<TournamentTeam> teamsSorted = new List<TournamentTeam>(Teams);
            teamsSorted.Sort((team1, team2) => team2.Score.CompareTo(team1.Score));

            // 2. Pair teams with the closest score (starting from the top). If there's an odd number of players, one player gets a bye (no opponent)
            return PairTeams(teamsSorted);
        }

        private Match[] PairTeams(List<TournamentTeam> teams)
        {
            tournamentMatches.Clear();
            unpairedTeams = new List<TournamentTeam>(teams);
            currentMatchID = 0;

            // If there's an odd number of players, one player gets a bye
            if (unpairedTeams.Count % 2 != 0)
            {
                TournamentTeam byeTeam = unpairedTeams.FirstOrDefault(p => !p.HasBye);
                if (byeTeam != null)
                {
                    byeTeam.HasBye = true;
                    TeamsWhoGotBye++;
                    unpairedTeams.Remove(byeTeam);
                    tournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(currentMatchID++, new Team[] { byeTeam, ScriptableObject.CreateInstance<TournamentTeam>().Initialize(-1, "Dummy", new Individual[] { }) })); // Add a bye pairing with dummy team
                }
            }

            // Pair remaining teams by closest score difference
            while (unpairedTeams.Count > 1)
            {
                TournamentTeam t1 = unpairedTeams[0];
                unpairedTeams.RemoveAt(0);

                opponentTeamIDs = PlayedMatches.Where(match => match.TeamFitnesses.Any(team => team.TeamID == t1.TeamId))
                    .Select(PlayedMatches => PlayedMatches.TeamFitnesses.First(team => team.TeamID != t1.TeamId).TeamID).Distinct()
                    .ToList();

                // Try to find an opponent who hasn't played with t1 yet and has a similar score
                TournamentTeam t2 = unpairedTeams
                    .Where(t => !opponentTeamIDs.Contains(t.TeamId)) 
                    .OrderBy(t => Math.Abs(t1.Score - t.Score))  // Closest score
                    .FirstOrDefault();

                // If no unplayed opponents with similar scores are found, pick the next available
                if (t2 == null)
                {
                    t2 = unpairedTeams[0];
                }
                unpairedTeams.Remove(t2);

                if (Coordinator.Instance.Random.NextDouble() > 0.5)
                    tournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(currentMatchID++, new Team[] { t1, t2 }));
                else
                    tournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(currentMatchID++, new Team[] { t2, t1 }));
            }

            // If enabled: For each match that already exists, add another match with the teams swapped
            if (Coordinator.Instance.SwapTournamentMatchTeams)
            {
                List<Match> matchesSwapped = new List<Match>();
                for (int i = 0; i < tournamentMatches.Count; i++)
                {
                    Match match = tournamentMatches[i];
                    matchesSwapped.Add(ScriptableObject.CreateInstance<Match>().Initialize(currentMatchID++, new Team[] { match.Teams[1], match.Teams[0] }));
                }

                tournamentMatches.AddRange(matchesSwapped);
            }

            return tournamentMatches.ToArray();
        }

        public override void UpdateTeamsScore(List<MatchFitness> tournamentMatchFitnesses, List<RatingPlayer> players = null)
        {
            if (players == null || players.Count == 0)
            {
                throw new ArgumentException("Players list cannot be null or empty.");
            }

            base.UpdateTeamsScore(tournamentMatchFitnesses);

            // Overwrite team scores based on players' ratings
            // 1. Reset all team scores
            foreach (var team in Teams)
            {
                team.Score = 0;
            }

            // 2. Update team scores based on their players' ratings (sum of player ratings from each team)
            foreach (var team in Teams)
            {
                double teamRating = 0;
                foreach (var individual in team.Individuals)
                {
                    var player = players.FirstOrDefault(p => p.IndividualID == individual.IndividualId);
                    if (player != null)
                    {
                        teamRating += player.GetRating();
                    }
                }
                team.Score = teamRating;
            }
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