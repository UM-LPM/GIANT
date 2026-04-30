using AgentOrganizations;
using Base;
using Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluators.CompetitionOrganizations
{
    public class RoundRobinTournament : CompetitionOrganization
    {

        List<Match> tournamentMatches;
        int currentMatchID;

        public RoundRobinTournament(CompetitionTeamOrganizator teamOrganizator, Individual[][] individuals, bool regenerateTeamsEachRound, int rounds = 1)
            : base(teamOrganizator, individuals, regenerateTeamsEachRound)
        {
            if (Teams.Length != 1)
            {
                throw new Exception("Invalid number of team groups! RoundRobinTournament requires exactly 1 group of teams.");
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

            for (int i = 0; i < Teams[0].Length; i++)
            {
                for (int j = i + 1; j < Teams[0].Length; j++)
                {
                    if (Coordinator.Instance.Random.NextDouble() > 0.5)
                        tournamentMatches.Add(new Match(currentMatchID++, new Team[] { Teams[0][i], Teams[0][j] }));
                    else
                        tournamentMatches.Add(new Match(currentMatchID++, new Team[] { Teams[0][j], Teams[0][i] }));
                }
            }

            // Shuffle the TournamentMatches randomly
            for (int i = 0; i < tournamentMatches.Count; i++)
            {
                int randomIndex = Coordinator.Instance.Random.Next(i, tournamentMatches.Count);
                Match temp = tournamentMatches[i];
                tournamentMatches[i] = tournamentMatches[randomIndex];
                tournamentMatches[randomIndex] = temp;
            }

            // If enabled: For each match that already exists, add another match with the teams swapped
            if (Coordinator.Instance.SwapCompetitionMatchTeams)
            {
                List<Match> matchesSwapped = new List<Match>();
                for (int i = 0; i < tournamentMatches.Count; i++)
                {
                    Match match = tournamentMatches[i];
                    matchesSwapped.Add(new Match(currentMatchID++, new Team[] { match.Teams[1], match.Teams[0] }));
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