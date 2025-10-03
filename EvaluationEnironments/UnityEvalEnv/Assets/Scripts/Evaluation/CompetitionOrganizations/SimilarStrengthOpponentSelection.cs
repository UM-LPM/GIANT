using AgentOrganizations;
using Base;
using Evaluators.CompetitionOrganizations;
using Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluators.CompetitionOrganizations
{
    public class SimilarStrengthOpponentSelection : CompetitionOrganization
    {
        private int TeamsWhoGotBye;

        List<Match> competitionMatches = new List<Match>();
        List<CompetitionTeam> unpairedTeams = new List<CompetitionTeam>();
        int currentMatchID;
        List<int> opponentTeamIDs;

        public SimilarStrengthOpponentSelection(CompetitionTeamOrganizator teamOrganizator, Individual[] individuals, bool regenerateTeamsEachRound, int rounds, int teamsPerMatch)
            : base(teamOrganizator, individuals, regenerateTeamsEachRound, teamsPerMatch)
        {
            Rounds = rounds < 1 ? (int)Math.Ceiling(Math.Log(Teams.Count, 2)) : rounds;
            ExecutedRounds = 0;
            PlayedMatches = new List<MatchFitness>();
        }

        public override void ResetCompetition()
        {
            Teams.Clear();
            ExecutedRounds = 0;
            PlayedMatches.Clear();
            TeamsWhoGotBye = 0;
        }

        public override Match[] GenerateCompetitionMatches()
        {
            if (IsCompetitionFinished())
                return new Match[] { };
            if (TeamsWhoGotBye == Teams.Count)
            {
                ResetTeamByes();
            }

            // 1. Sort teams by score
            List<CompetitionTeam> teamsSorted = new List<CompetitionTeam>(Teams);
            teamsSorted.Sort((team1, team2) => team2.Score.CompareTo(team1.Score));

            // 2. Group teams with the closest score (starting from the top). If the number of teams is not divisible by TeamsPerMatch, the remaining get a bye (no opponent)
            return GroupTeams(teamsSorted);
        }

        private Match[] GroupTeams(List<CompetitionTeam> teams)
        {
            competitionMatches.Clear();
            unpairedTeams = new List<CompetitionTeam>(teams);
            currentMatchID = 0;

            // Handle byes if teams don't divide evenly into groups
            int remainder = unpairedTeams.Count % TeamsPerMatch;
            if (remainder > 0)
            {
                var byeTeams = unpairedTeams
                    .Where(t => !t.HasBye)
                    .Take(remainder)
                    .ToList();

                foreach (var byeTeam in byeTeams)
                {
                    byeTeam.HasBye = true;
                    TeamsWhoGotBye++;
                    unpairedTeams.Remove(byeTeam);

                    // Dummy opponents fill the match
                    var dummyTeams = Enumerable.Range(0, TeamsPerMatch - 1)
                        .Select(_ => ScriptableObject.CreateInstance<CompetitionTeam>().Initialize(-1, "Dummy", Array.Empty<Individual>()))
                        .ToArray();

                    var teamsForByeMatch = new List<Team> { byeTeam };
                    teamsForByeMatch.AddRange(dummyTeams);

                    competitionMatches.Add(
                        ScriptableObject.CreateInstance<Match>()
                            .Initialize(currentMatchID++, teamsForByeMatch.ToArray()));
                }
            }

            // Group remaining teams by closest score difference
            while (unpairedTeams.Count > (TeamsPerMatch - 1))
            {
                // Take first team
                var t1 = unpairedTeams[0];
                unpairedTeams.RemoveAt(0);

                // Find closest (TeamsPerMatch - 1) teams by score
                var selected = unpairedTeams
                    .OrderBy(t => Math.Abs(t1.Score - t.Score))
                    .Take(TeamsPerMatch - 1)
                    .ToList();

                foreach (var t in selected) unpairedTeams.Remove(t);

                var matchTeams = new List<Team> { t1 };
                matchTeams.AddRange(selected);

                if (Coordinator.Instance.Random.NextDouble() < 0.5)
                    matchTeams.Reverse();

                competitionMatches.Add(
                    ScriptableObject.CreateInstance<Match>()
                        .Initialize(currentMatchID++, matchTeams.ToArray()));
            }

            // Optional: create mirrored matches with swapped order
            if (Coordinator.Instance.SwapCompetitionMatchTeams)
            {
                var swapped = new List<Match>();
                foreach (var match in competitionMatches)
                {
                    var reversedTeams = match.Teams.Reverse().ToArray();
                    swapped.Add(ScriptableObject.CreateInstance<Match>().Initialize(currentMatchID++, reversedTeams));
                }
                competitionMatches.AddRange(swapped);
            }

            return competitionMatches.ToArray();
        }

        public override void UpdateTeamsScore(List<MatchFitness> competitionMatchFitnesses, List<CompetitionPlayer> players = null)
        {
            // 1. Call base UpdateTeamsScore
            base.UpdateTeamsScore(competitionMatchFitnesses, players);

            // 2. Optional: Update team scores based on their players' ratings (sum of player ratings from each team)
            if (players != null && players.Count > 0)
            {
                foreach (var team in Teams)
                {
                    double teamRating = 0;
                    foreach (var individual in team.Individuals)
                    {
                        var player = players.FirstOrDefault(p => p.IndividualID == individual.IndividualId);
                        if (player != null)
                        {
                            teamRating += player.GetScore();
                        }
                    }
                    team.Score = teamRating;
                }
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