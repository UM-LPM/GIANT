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
    public class SingleEliminationTournament : CompetitionOrganization
    {
        private int TeamsWhoGotBye;

        public List<CompetitionTeam> EliminatedTeams = new List<CompetitionTeam>();
        public List<Match> TournamentMatches = new List<Match>();

        List<CompetitionTeam> UnpairedTeams = new List<CompetitionTeam>();
        int CurrentMatchID;

        public SingleEliminationTournament(CompetitionTeamOrganizator teamOrganizator, Individual[][] individuals, bool regenerateTeamsEachRound, int rounds)
            : base(teamOrganizator, individuals, regenerateTeamsEachRound)
        {
            if (Teams.Length != 1)
            {
                throw new Exception("Invalid number of team groups! SingleEliminationTournament requires exactly 1 group of teams.");
            }

            Rounds = rounds < 1 ? (int)Math.Ceiling(Math.Log(Teams[0].Length, 2)) : rounds;
            ExecutedRounds = 0;
            PlayedMatches = new List<MatchFitness>();
        }

        public override Match[] GenerateCompetitionMatches()
        {
            if (IsCompetitionFinished())
                return new Match[] { };
            if (TeamsWhoGotBye == Teams[0].Length)
            {
                ResetTeamByes();
            }

            // Get all Teams that were yet not eliminated
            List<CompetitionTeam> teams = new List<CompetitionTeam>(Teams[0]);
            teams.RemoveAll(team => EliminatedTeams.Contains(team));

            return PairTeams(teams);
        }

        private Match[] PairTeams(List<CompetitionTeam> teams)
        {
            TournamentMatches.Clear();
            UnpairedTeams = new List<CompetitionTeam>(teams);
            CurrentMatchID = 0;

            // If there's an odd number of players, one player gets a bye
            if (UnpairedTeams.Count % 2 != 0)
            {
                CompetitionTeam byeTeam = UnpairedTeams.FirstOrDefault(p => !p.HasBye);
                if (byeTeam != null)
                {
                    byeTeam.HasBye = true;
                    TeamsWhoGotBye++;
                    UnpairedTeams.Remove(byeTeam);
                    TournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(CurrentMatchID++, new Team[] { byeTeam, ScriptableObject.CreateInstance<CompetitionTeam>().Initialize(-1, "Dummy", new Individual[] { }) })); // Add a bye pairing with dummy team
                }
            }

            // Pair remaining players
            while (UnpairedTeams.Count > 1)
            {
                CompetitionTeam t1 = UnpairedTeams[0];
                UnpairedTeams.RemoveAt(0);

                CompetitionTeam t2 = UnpairedTeams[0];
                UnpairedTeams.RemoveAt(0);

                if (Coordinator.Instance.Random.NextDouble() > 0.5)
                    TournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(CurrentMatchID++, new Team[] { t1, t2 }));
                else
                    TournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(CurrentMatchID++, new Team[] { t2, t1 }));
            }

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

        public override void UpdateTeamsScore(List<MatchFitness> tournamentMatchFitnesses, List<CompetitionPlayer> players = null)
        {
            bool usePlayers = players != null && players.Count > 0;

            List<MatchFitness> tournamentMatchFitnessesCopy = new List<MatchFitness>(tournamentMatchFitnesses);
            // Add played TournamentMatches to the list of played TournamentMatches (add only matchFitnesses that are not dummy)
            PlayedMatches.AddRange(tournamentMatchFitnessesCopy.FindAll(matchFitness => !matchFitness.IsDummy));

            MatchFitness matchFitness;
            List<MatchFitness> matchFitnesses = new List<MatchFitness>();
            List<MatchFitness> matchFitnessesSwaped = new List<MatchFitness>();
            while (tournamentMatchFitnessesCopy.Count > 0)
            {
                // 1. Get all match data
                matchFitness = new MatchFitness();
                MatchFitness.GetMatchFitness(tournamentMatchFitnessesCopy, matchFitness, matchFitnesses, matchFitnessesSwaped, Coordinator.Instance.SwapCompetitionMatchTeams);

                if (matchFitness.IsDummy)
                {
                    // Bye
                    foreach (var tf in matchFitness.TeamFitnesses)
                        if (tf.TeamID != -1 && (!usePlayers))
                            TeamLookup[tf.TeamID].Score += 2;

                    continue;
                }

                teamFitnessRes1 = matchFitness.TeamFitnesses[0];
                teamFitnessRes2 = matchFitness.TeamFitnesses[1];

                if (usePlayers)
                {
                    teamFitness1 = 0;
                    teamFitness2 = 0;

                    foreach (var individualFitness in teamFitnessRes1.IndividualFitness)
                    {
                        var player = players.Where(p => p.IndividualID == individualFitness.IndividualID).FirstOrDefault();
                        if (player != null)
                        {
                            teamFitness1 -= (float)player.GetScore();
                        }
                        else
                        {
                            throw new Exception("Invalid player ID in match fitness! Player IDs must match the IDs of the players in the competition organization.");
                        }
                    }

                    foreach (var individualFitness in teamFitnessRes2.IndividualFitness)
                    {
                        var player = players.Where(p => p.IndividualID == individualFitness.IndividualID).FirstOrDefault();
                        if (player != null)
                        {
                            teamFitness2 -= (float)player.GetScore();
                        }
                        else
                        {
                            throw new Exception("Invalid player ID in match fitness! Player IDs must match the IDs of the players in the competition organization.");
                        }
                    }
                }
                else
                {
                    teamFitness1 = teamFitnessRes1.GetTeamFitness();
                    teamFitness2 = teamFitnessRes2.GetTeamFitness();
                }

                var team1 = Teams[0].Where(team => team.TeamId == teamFitnessRes1.TeamID).First();
                var team2 = Teams[0].Where(team => team.TeamId == teamFitnessRes2.TeamID).First();

                if (team1 == null || team2 == null)
                {
                    throw new Exception("Invalid team IDs in match fitness! Team IDs must match the IDs of the teams in the competition organization.");
                }

                if (teamFitness1 < teamFitness2)
                {
                    
                    team1.Score = usePlayers? teamFitness1 * - 1 : team1.Score + 2;
                    team2.Score = usePlayers? teamFitness2 * - 1 : team2.Score;
                    EliminatedTeams.Add(team2);
                }
                else if (teamFitness1 > teamFitness2)
                {
                    team2.Score = usePlayers? teamFitness2 * -1 : team2.Score + 2;
                    team1.Score = usePlayers ? teamFitness1 * -1 : team1.Score;
                    EliminatedTeams.Add(team1);
                }
                else
                {
                    // Random choose the winner if the scores are equal
                    if (Coordinator.Instance.Random.NextDouble() > 0.5)
                    {
                        team1.Score = usePlayers ? teamFitness1 * -1 : team1.Score + 2;
                        team2.Score = usePlayers ? teamFitness2 * -1 : team2.Score;
                        EliminatedTeams.Add(team2);
                    }
                    else
                    {
                        team2.Score = usePlayers ? teamFitness2 * -1 : team2.Score + 2;
                        team1.Score = usePlayers ? teamFitness1 * -1 : team1.Score;
                        EliminatedTeams.Add(team1);
                    }
                }

                // Add individual match results to the teams
                team1.IndividualMatchResults.Add(new IndividualMatchResult()
                {
                    MatchName = matchFitness.MatchName,
                    OpponentsIDs = team2.Individuals.Select(individual => individual.IndividualId).ToArray(),
                    Value = teamFitness1,
                    IndividualValues = teamFitnessRes1.GetTeamIndividualValues()
                });

                team2.IndividualMatchResults.Add(new IndividualMatchResult()
                {
                    MatchName = matchFitness.MatchName,
                    OpponentsIDs = team1.Individuals.Select(individual => individual.IndividualId).ToArray(),
                    Value = teamFitness2,
                    IndividualValues = teamFitnessRes2.GetTeamIndividualValues()
                });
            }

            // Increment the number of executed rounds
            ExecutedRounds++;
        }

        public override bool IsCompetitionFinished()
        {
            if (EliminatedTeams.Count == Teams[0].Length - 1)
                return true;
            else
                return false;
        }

        private void ResetTeamByes()
        {
            foreach (var team in Teams[0])
            {
                team.HasBye = false;
            }
            TeamsWhoGotBye = 0;
        }
    }
}