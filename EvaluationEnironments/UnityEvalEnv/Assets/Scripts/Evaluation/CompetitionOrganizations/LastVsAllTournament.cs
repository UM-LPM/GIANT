using AgentOrganizations;
using Base;
using Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluators.CompetitionOrganizations
{
    public class LastVsAllTournament : CompetitionOrganization
    {
        List<Match> tournamentMatches;
        int currentMatchID;

        public LastVsAllTournament(CompetitionTeamOrganizator teamOrganizator, Individual[][] individuals, bool regenerateTeamsEachRound, int rounds = 1)
            : base(teamOrganizator, individuals, regenerateTeamsEachRound)
        {
            if (Teams.Length != 1)
            {
                throw new Exception("Invalid number of team groups! LastVsAllTournament requires exactly 1 group of teams.");
            }

            Rounds = rounds < 1 ? (int)Math.Ceiling(Math.Log(Teams[0].Length, 2)) : rounds;
            ExecutedRounds = 0;
            PlayedMatches = new List<MatchFitness>();

            tournamentMatches = new List<Match>();
        }

        public override Match[] GenerateCompetitionMatches()
        {
            if (IsCompetitionFinished())
                return new Match[] { };

            tournamentMatches.Clear();
            currentMatchID = 0;

            var teamGroup0 = Teams[0];

            Team lastTeam = teamGroup0[teamGroup0.Length - 1];

            for (int i = 0; i < teamGroup0.Length - 1; i++)
            {
                if (Coordinator.Instance.Random.NextDouble() > 0.5)
                    tournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(currentMatchID++, new Team[] { teamGroup0[i], lastTeam }));
                else
                    tournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(currentMatchID++, new Team[] { lastTeam, teamGroup0[i] }));
            }

            // If enabled: For each match that already exists, add another match with the teams swapped
            if (Coordinator.Instance.SwapCompetitionMatchTeams)
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

        public override void UpdateTeamsScore(List<MatchFitness> competitionMatchFitnesses, List<CompetitionPlayer> players = null)
        {
            // 1. Call base UpdateTeamsScore
            base.UpdateTeamsScore(competitionMatchFitnesses, players);

            // 2. Optional: Update team scores based on their players' ratings (sum of player ratings from each team)
            if (players != null && players.Count > 0)
            {
                foreach (var team in Teams[0])
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
    }
}