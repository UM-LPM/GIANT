using AgentOrganizations;
using Base;
using Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluators.TournamentOrganizations
{
    public class KRandomOpponentsTournament : TournamentOrganization
    {
        List<Match> TournamentMatches = new List<Match>();
        int currentMatchID;
        List<int> matchedOpponentTeamIDs;
        List<int> freeOpponentTeamIDs;

        public KRandomOpponentsTournament(TournamentTeamOrganizator teamOrganizator, Individual[] individuals, bool regenerateTeamsEachRound, int rounds)
            : base(teamOrganizator, individuals, regenerateTeamsEachRound)
        {
            Rounds = rounds == -1 ? Teams.Count -1 : rounds >  Teams.Count -1 ? Teams.Count -1 : rounds; // If rounds is not set round robin will be performed
            ExecutedRounds = 0;
            PlayedMatches = new List<MatchFitness>();
        }

        public override void ResetTournament()
        {
            Teams.Clear();
            ExecutedRounds = 0;
            PlayedMatches.Clear();
        }

        public override Match[] GenerateTournamentMatches()
        {
            if (IsTournamentFinished())
                return new Match[] { };

            TournamentMatches.Clear();
            currentMatchID = 0;

            for (int i = 0; i < Teams.Count; i++)
            {
                matchedOpponentTeamIDs = TournamentMatches.Where(match => match.Teams.Any(team => team.TeamId == Teams[i].TeamId))
                    .Select(TournamentMatches => TournamentMatches.Teams.First(team => team.TeamId != Teams[i].TeamId).TeamId).Distinct()
                    .ToList();

                // Get all Team IDs except the current team and teams that are in matchedOpponentTeamIDs
                freeOpponentTeamIDs = Teams.Where(team => team.TeamId != Teams[i].TeamId && !matchedOpponentTeamIDs.Contains(team.TeamId))
                    .Select(team => team.TeamId).ToList();

                // If there are no free opponents left, the team has played against all other teams
                if (freeOpponentTeamIDs.Count == 0)
                    continue;

                for (int j = 0; j < Rounds - matchedOpponentTeamIDs.Count; j++)
                {
                    int randomIndex = Coordinator.Instance.Random.Next(0, freeOpponentTeamIDs.Count);
                    int opponentTeamID = freeOpponentTeamIDs[randomIndex];
                    freeOpponentTeamIDs.RemoveAt(randomIndex);

                    if (Coordinator.Instance.Random.NextDouble() > 0.5)
                        TournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(currentMatchID++, new Team[] { Teams[i], Teams.Find(team => team.TeamId == opponentTeamID) }));
                    else
                        TournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(currentMatchID++, new Team[] { Teams.Find(team => team.TeamId == opponentTeamID), Teams[i] }));
                }
            }

            // Shuffle the TournamentMatches randomly
            for (int i = 0; i < TournamentMatches.Count; i++)
            {
                int randomIndex = Coordinator.Instance.Random.Next(i, TournamentMatches.Count);
                Match temp = TournamentMatches[i];
                TournamentMatches[i] = TournamentMatches[randomIndex];
                TournamentMatches[randomIndex] = temp;
            }

            // If enabled: For each match that already exists, add another match with the teams swapped
            if (Coordinator.Instance.SwapTournamentMatchTeams)
            {
                List<Match> matchesSwapped = new List<Match>();
                for (int i = 0; i < TournamentMatches.Count; i++)
                {
                    Match match = TournamentMatches[i];
                    matchesSwapped.Add(ScriptableObject.CreateInstance<Match>().Initialize(currentMatchID++, new Team[] { match.Teams[1], match.Teams[0] }));
                }

                TournamentMatches.AddRange(matchesSwapped);
            }

            return TournamentMatches.ToArray();
        }

        public override bool IsTournamentFinished()
        {
            if (ExecutedRounds == 1)
                return true;
            else
                return false;
        }
    }
}