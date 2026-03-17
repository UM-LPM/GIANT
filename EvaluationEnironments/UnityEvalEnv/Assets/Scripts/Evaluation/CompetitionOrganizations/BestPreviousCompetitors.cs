using AgentOrganizations;
using Base;
using Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluators.CompetitionOrganizations
{
    public class BestPreviousCompetitors : CompetitionOrganization
    {
        List<Match> TournamentMatches = new List<Match>();
        int currentMatchID;
        List<int> matchedOpponentTeamIDs;
        List<int> freeOpponentTeamIDs;

        public BestPreviousCompetitors(CompetitionTeamOrganizator teamOrganizator, Individual[][] individuals, bool regenerateTeamsEachRound, int rounds)
            : base(teamOrganizator, individuals, regenerateTeamsEachRound)
        {
            if (Teams.Length != 2)
            {
                throw new Exception("Invalid number of team groups! BestPreviousCompetitors requires exactly 2 group of teams.");
            }

            Rounds = 1; // Fixed 
            ExecutedRounds = 0;
            PlayedMatches = new List<MatchFitness>();
        }

        public override Match[] GenerateCompetitionMatches()
        {
            if (IsCompetitionFinished())
                return new Match[] { };

            var teamGroup0 = Teams[0];
            var teamGroup1 = Teams[1];

            // pair all individuals from teamGroup1 (best previous competitors) with the best individual from teamGroup0 (current competitors)

            TournamentMatches.Clear();

            foreach(CompetitionTeam team0 in teamGroup0)
            {
                foreach (CompetitionTeam team1 in teamGroup1)
                {
                    if (Coordinator.Instance.Random.NextDouble() > 0.5)
                        TournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(currentMatchID++, new Team[] { team0, team1}));
                    else
                        TournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(currentMatchID++, new Team[] { team1, team0 }));
                }
            }

            // If enabled: For each match that already exists, add another match with the teams swapped
            if (Coordinator.Instance.SwapCompetitionMatchTeams)
            {
                int currentCount = TournamentMatches.Count;
                for (int i = 0; i < currentCount; i++)
                {
                    Match originalMatch = TournamentMatches[i];
                    Team team1 = originalMatch.Teams[0];
                    Team team2 = originalMatch.Teams[1];
                    TournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(currentMatchID++, new Team[] { team2, team1 }));
                }
            }

            return TournamentMatches.ToArray();
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