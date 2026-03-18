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

        public override void UpdateTeamsScore(List<MatchFitness> competitionMatchFitnesses, List<CompetitionPlayer> players = null)
        {
            var competitionMatchFitnessesCopy = new List<MatchFitness>(competitionMatchFitnesses);

            // Add played matches (ignore dummy matches)
            PlayedMatches.AddRange(competitionMatchFitnessesCopy.Where(mf => !mf.IsDummy));

            var matchFitnesses = new List<MatchFitness>();
            var matchFitnessesSwapped = new List<MatchFitness>();

            while (competitionMatchFitnessesCopy.Count > 0)
            {
                // 1. Get match data
                var matchFitness = new MatchFitness();
                MatchFitness.GetMatchFitness(
                    competitionMatchFitnessesCopy,
                    matchFitness,
                    matchFitnesses,
                    matchFitnessesSwapped,
                    Coordinator.Instance.SwapCompetitionMatchTeams
                );

                if (matchFitness.IsDummy)
                {
                    continue;
                }

                // 2. Assign Score
                for (int i = 0; i < matchFitness.TeamFitnesses.Count; i++)
                {
                    var tf = matchFitness.TeamFitnesses[i];
                    var team = TeamLookup[tf.TeamID];

                    team.Score += tf.GetTeamFitness(); // Use actual fitness as score (instead of points based on ranking)

                    // 3. Record individual match results
                    var opponents = matchFitness.TeamFitnesses
                       .Where(x => x.TeamID != tf.TeamID)
                       .SelectMany(x => TeamLookup[x.TeamID]
                           .Individuals
                           .Select(ind => ind.IndividualId))
                       .ToArray();

                    for (int j = 0; j < tf.IndividualFitness.Count; j++)
                    {
                        team.IndividualMatchResults.Add(new IndividualMatchResult()
                        {
                            MatchName = matchFitness.MatchName,
                            OpponentsIDs = opponents,
                            Value = tf.IndividualFitness[j].Value,
                            IndividualValues = tf.IndividualFitness[j].IndividualValues
                        });
                    }
                }
            }

            // Get teams that participated in the current matches
            var matchedTeamIds = competitionMatchFitnesses.SelectMany(mf => mf.TeamFitnesses.Select(tf => tf.TeamID)).Distinct().ToList();

            // Average the score of each team over all matches they participated in
            foreach (var matchedTeamId in matchedTeamIds)
            {
                var team = TeamLookup[matchedTeamId];
                int matchesPlayed = team.IndividualMatchResults.Count;
                if (matchesPlayed > 0)
                {
                    team.Score = Math.Abs(team.Score / matchesPlayed); // Average score over matches played
                }
            }

            // Increment the number of executed rounds
            ExecutedRounds++;
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