using AgentOrganizations;
using Base;
using Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluators.CompetitionOrganizations
{
    public class KRandomOpponentsTournament : CompetitionOrganization
    {
        List<Match> TournamentMatches = new List<Match>();
        int CurrentMatchID;
        List<int> matchedOpponentTeamIDs;
        List<int> freeOpponentTeamIDs;

        const int MAX_ATTEMPTS = 1000; // Maximum number of attempts to find valid matches

        public KRandomOpponentsTournament(CompetitionTeamOrganizator teamOrganizator, Individual[][] individuals, bool regenerateTeamsEachRound, int rounds)
            : base(teamOrganizator, individuals, regenerateTeamsEachRound)
        {
            if (Teams.Length != 1)
            {
                throw new Exception("Invalid number of team groups! KRandomOpponentsTournament requires exactly 1 group of teams.");
            }

            Rounds = rounds == -1 ? Teams[0].Length -1 : rounds >  Teams[0].Length -1 ? Teams[0].Length -1 : rounds; // If rounds is not set round robin will be performed
            ExecutedRounds = 0;
            PlayedMatches = new List<MatchFitness>();
        }

        public override Match[] GenerateCompetitionMatches()
        {
            if (IsCompetitionFinished())
                return new Match[] { };

            for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
            {
                var teamGroup0 = Teams[0];

                int N = teamGroup0.Length;
                int targetMatches = (N * Rounds) / 2;

                if(N % 2 != 0)
                {
                    throw new Exception("Invalid number of teams! KRandomOpponentsTournament requires an even number of teams to ensure that all teams can be paired.");
                }

                int[] degree = new int[N];
                List<(int, int)> pairs = new List<(int, int)>();

                // 1. Generate all possible pairs of teams
                for (int i = 0; i < N; i++)
                {
                    for (int j = i + 1; j < N; j++)
                    {
                        pairs.Add((i, j));
                    }
                }

                // 2. Shuffle the pairs randomly using Fisher-Yates shuffle algorithm
                for (int i = 0; i < pairs.Count; i++)
                {
                    int randomIndex = Coordinator.Instance.Random.Next(i, pairs.Count);
                    var temp = pairs[i];
                    pairs[i] = pairs[randomIndex];
                    pairs[randomIndex] = temp;
                }

                TournamentMatches.Clear();
                CurrentMatchID = 0;

                // 3. Iterate through the shuffled pairs and add them to the tournament matches if both teams have played less than Rounds matches, until we reach the target number of matches
                foreach (var (team1, team2) in pairs)
                {
                    if (degree[team1] < Rounds && degree[team2] < Rounds)
                    {
                        TournamentMatches.Add(
                            ScriptableObject.CreateInstance<Match>().Initialize(CurrentMatchID++, new Team[] { teamGroup0[team1], teamGroup0[team2] }));
                        degree[team1]++;
                        degree[team2]++;

                        if (TournamentMatches.Count >= targetMatches)
                            break;
                    }
                }

                if (TournamentMatches.Count == targetMatches)
                {
                    // If enabled: For each match that already exists, add another match with the teams swapped
                    if (Coordinator.Instance.SwapCompetitionMatchTeams)
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
            }

            throw new Exception("Failed to generate valid matches after " + MAX_ATTEMPTS + " attempts!");
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

        public override bool IsCompetitionFinished()
        {
            if (ExecutedRounds == 1)
                return true;
            else
                return false;
        }
    }
}