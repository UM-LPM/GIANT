using AgentOrganizations;
using Fitnesses;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Evaluators.TournamentOrganizations
{
    public class RoundRobinTournament : TournamentOrganization
    {

        List<Match> tournamentMatches;

        public RoundRobinTournament(List<TournamentTeam> teams, int rounds = 1)
        {
            Teams = teams;
            Rounds = rounds < 1 ? (int)Math.Ceiling(Math.Log(teams.Count, 2)) : rounds;
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

            int counter = 0;
            for (int i = 0; i < Teams.Count; i++)
            {
                for (int j = i + 1; j < Teams.Count; j++)
                {
                    if (Coordinator.Instance.Random.NextDouble() > 0.5)
                        tournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(counter++, new Team[] { Teams[i], Teams[j] }));
                    else
                        tournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(counter++, new Team[] { Teams[j], Teams[i] }));
                }
            }

            // Shuffle the matches randomly
            for (int i = 0; i < tournamentMatches.Count; i++)
            {
                int randomIndex = Coordinator.Instance.Random.Next(i, tournamentMatches.Count);
                Match temp = tournamentMatches[i];
                tournamentMatches[i] = tournamentMatches[randomIndex];
                tournamentMatches[randomIndex] = temp;
            }

            return tournamentMatches.ToArray();
        }
    }
}