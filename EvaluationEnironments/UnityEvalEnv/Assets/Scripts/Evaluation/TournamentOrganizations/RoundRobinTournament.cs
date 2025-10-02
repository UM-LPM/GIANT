using AgentOrganizations;
using Base;
using Fitnesses;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Evaluators.TournamentOrganizations
{
    public class RoundRobinTournament : TournamentOrganization
    {

        List<Match> tournamentMatches;
        int currentMatchID;

        public RoundRobinTournament(TournamentTeamOrganizator teamOrganizator, Individual[] individuals, bool regenerateTeamsEachRound, int rounds = 1)
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

            for (int i = 0; i < Teams.Count; i++)
            {
                for (int j = i + 1; j < Teams.Count; j++)
                {
                    if (Coordinator.Instance.Random.NextDouble() > 0.5)
                        tournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(currentMatchID++, new Team[] { Teams[i], Teams[j] }));
                    else
                        tournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(currentMatchID++, new Team[] { Teams[j], Teams[i] }));
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