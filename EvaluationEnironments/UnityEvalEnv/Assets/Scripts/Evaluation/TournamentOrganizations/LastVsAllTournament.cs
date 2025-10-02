using AgentOrganizations;
using Base;
using Fitnesses;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Evaluators.TournamentOrganizations
{
    public class LastVsAllTournament : TournamentOrganization
    {

        List<Match> tournamentMatches;
        int currentMatchID;

        public LastVsAllTournament(TournamentTeamOrganizator teamOrganizator, Individual[] individuals, bool regenerateTeamsEachRound, int rounds = 1)
            : base(teamOrganizator, individuals, regenerateTeamsEachRound)
        {
            Rounds = rounds < 1 ? (int)Math.Ceiling(Math.Log(Teams.Count, 2)) : rounds;
            ExecutedRounds = 0;
            PlayedMatches = new List<MatchFitness>();

            tournamentMatches = new List<Match>();
        }

        public override void ResetTournament()
        {
            Teams.Clear();
            ExecutedRounds = 0;
            PlayedMatches.Clear();

            tournamentMatches.Clear();
        }

        public override Match[] GenerateTournamentMatches()
        {
            if (IsTournamentFinished())
                return new Match[] { };

            tournamentMatches.Clear();
            currentMatchID = 0;

            Team lastTeam = Teams[Teams.Count - 1];

            for (int i = 0; i < Teams.Count - 1; i++)
            {
                if (Coordinator.Instance.Random.NextDouble() > 0.5)
                    tournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(currentMatchID++, new Team[] { Teams[i], lastTeam }));
                else
                    tournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(currentMatchID++, new Team[] { lastTeam, Teams[i] }));
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
    }
}