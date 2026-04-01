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
    public enum DoubleEliminationTournamentStep
    {
        Init,
        WinnersBracket,
        LosersBracket,
        GrandFinals,
        Finished
    }

    public class DoubleEliminationTournament : CompetitionOrganization
    {
        private DoubleEliminationTournamentStep TournamentStep;

        private int TeamsWhoGotBye;
        public List<CompetitionTeam> EliminatedTeams = new List<CompetitionTeam>();
        public List<CompetitionTeam> WinnersBracket = new List<CompetitionTeam>();
        public List<CompetitionTeam> LosersBracket = new List<CompetitionTeam>();

        // Temp properties
        public List<CompetitionTeam> LoserTeams = new List<CompetitionTeam>();
        public List<Match> TournamentMatches = new List<Match>();
        List<CompetitionTeam> UnpairedTeams = new List<CompetitionTeam>();
        int CurrentMatchID;
        int CurrentScore = 0;

        public DoubleEliminationTournament(CompetitionTeamOrganizator teamOrganizator, Individual[][] individuals, bool regenerateTeamsEachRound, int rounds)
            : base(teamOrganizator, individuals, regenerateTeamsEachRound)
        {
            if (Teams.Length != 1)
            {
                throw new Exception("Invalid number of team groups! DoubleEliminationTournament requires exactly 1 group of teams.");
            }

            Rounds = rounds < 1 ? (int)Math.Ceiling(Math.Log(Teams[0].Length, 2)) : rounds;
            ExecutedRounds = 0;
            PlayedMatches = new List<MatchFitness>();
            TournamentStep = DoubleEliminationTournamentStep.Init;

            EliminatedTeams = new List<CompetitionTeam>();
            WinnersBracket = new List<CompetitionTeam>();
            LosersBracket = new List<CompetitionTeam>();
            LoserTeams = new List<CompetitionTeam>();
            CurrentScore = 0;
        }

        public override Match[] GenerateCompetitionMatches()
        {
            if (IsCompetitionFinished())
                return new Match[] { };
            if (TeamsWhoGotBye == Teams[0].Length)
            {
                ResetTeamByes();
            }

            switch (TournamentStep)
            {
                case DoubleEliminationTournamentStep.Init:
                    InitTournamentMatches();
                    break;
                case DoubleEliminationTournamentStep.WinnersBracket:
                    GenerateWinnersBracketMatches();
                    break;
                case DoubleEliminationTournamentStep.LosersBracket:
                    GenerateLosersBracketMatches();
                    break;
                case DoubleEliminationTournamentStep.GrandFinals:
                    GenerateGrandFinalsMatches();
                    break;
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
                    LoserTeams.Add(team2);
                }
                else if (teamFitness1 > teamFitness2)
                {
                    LoserTeams.Add(team1);
                }
                else
                {
                    // Random choose the winner if the scores are equal
                    if (Coordinator.Instance.Random.NextDouble() > 0.5)
                    {
                        LoserTeams.Add(team2);
                    }
                    else
                    {
                        LoserTeams.Add(team1);
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

            UpdateWinnerLoserBrackets(players);

            UpdateTournamentStep(players);

            // Increment the number of executed rounds
            ExecutedRounds++;
        }

        public void UpdateWinnerLoserBrackets(List<CompetitionPlayer> players = null)
        {
            int counter = 0;
            bool teamsEliminated = false;
            for (int i = 0; i < LoserTeams.Count; i++)
            {
                if(!LosersBracket.Contains(LoserTeams[i]) && WinnersBracket.Count == 1 && LosersBracket.Count == 1)
                {
                    WinnersBracket.Remove(LoserTeams[i]);
                    EliminatedTeams.Add(LoserTeams[i]);

                    if (players != null && players.Count > 0)
                    {
                        double teamRating = 0;
                        foreach (var individual in LoserTeams[i].Individuals)
                        {
                            var player = players.FirstOrDefault(p => p.IndividualID == individual.IndividualId);
                            if (player != null)
                            {
                                teamRating += player.GetScore();
                            }
                        }

                        LoserTeams[i].Score = teamRating;
                    }
                    else
                    {
                        LoserTeams[i].Score = CurrentScore;
                    }
                    teamsEliminated = true;
                }
                else if (LosersBracket.Contains(LoserTeams[i]))
                {
                    LosersBracket.Remove(LoserTeams[i]);
                    EliminatedTeams.Add(LoserTeams[i]);

                    if (players != null && players.Count > 0)
                    {
                        double teamRating = 0;
                        foreach (var individual in LoserTeams[i].Individuals)
                        {
                            var player = players.FirstOrDefault(p => p.IndividualID == individual.IndividualId);
                            if (player != null)
                            {
                                teamRating += player.GetScore();
                            }
                        }

                        LoserTeams[i].Score = teamRating;
                    }
                    else
                    {
                        LoserTeams[i].Score = CurrentScore;
                    }

                    teamsEliminated = true;
                }
                else
                {
                    WinnersBracket.Remove(LoserTeams[i]);
                    if (counter > LosersBracket.Count)
                        LosersBracket.Add(LoserTeams[i]);
                    else
                        LosersBracket.Insert(counter, LoserTeams[i]);

                    counter += 2;
                }
            }

            if(teamsEliminated)
            {
                CurrentScore += 1;
            }

            LoserTeams.Clear();
        }

        public void UpdateTournamentStep(List<CompetitionPlayer> players)
        {
            if (WinnersBracket.Count == 1 && LosersBracket.Count == 1)
            {
                TournamentStep = DoubleEliminationTournamentStep.GrandFinals;
                return;
            }
            else if(WinnersBracket.Count == 1 && LosersBracket.Count > 1)
            {
                TournamentStep = DoubleEliminationTournamentStep.LosersBracket;
                return;
            }

            switch (TournamentStep)
            {
                case DoubleEliminationTournamentStep.Init:
                    TournamentStep = DoubleEliminationTournamentStep.LosersBracket;
                    break;
                case DoubleEliminationTournamentStep.WinnersBracket:
                    TournamentStep = DoubleEliminationTournamentStep.LosersBracket;
                    break;
                case DoubleEliminationTournamentStep.LosersBracket:
                    TournamentStep = DoubleEliminationTournamentStep.WinnersBracket;
                    break;
                case DoubleEliminationTournamentStep.GrandFinals:
                    UpdateTournamentWinnerScore(players);
                    TournamentStep = DoubleEliminationTournamentStep.Finished;
                    break;
            }
        }

        public override bool IsCompetitionFinished()
        {
            if(TournamentStep == DoubleEliminationTournamentStep.Finished)
            {
                return true;
            }
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

        private void InitTournamentMatches()
        {
            PairTeams(Teams[0].ToList());
            WinnersBracket.AddRange(Teams[0]);
        }

        private void GenerateWinnersBracketMatches()
        {
            PairTeams(WinnersBracket);
        }

        private void GenerateLosersBracketMatches()
        {
            PairTeams(LosersBracket);
        }

        private void GenerateGrandFinalsMatches()
        {
            PairTeams(new List<CompetitionTeam>() { WinnersBracket[0], LosersBracket[0] });
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

        private void UpdateTournamentWinnerScore(List<CompetitionPlayer> players)
        {
            if (WinnersBracket.Count == 1)
            {
                if (players != null && players.Count > 0)
                {
                    double teamRating = 0;
                    foreach (var individual in WinnersBracket[0].Individuals)
                    {
                        var player = players.FirstOrDefault(p => p.IndividualID == individual.IndividualId);
                        if (player != null)
                        {
                            teamRating += player.GetScore();
                        }
                    }

                    WinnersBracket[0].Score = teamRating;
                }
                else
                {
                    WinnersBracket[0].Score = CurrentScore;
                }
            }
            else if(LosersBracket.Count == 1)
            {
                if (players != null && players.Count > 0)
                {
                    double teamRating = 0;
                    foreach (var individual in LosersBracket[0].Individuals)
                    {
                        var player = players.FirstOrDefault(p => p.IndividualID == individual.IndividualId);
                        if (player != null)
                        {
                            teamRating += player.GetScore();
                        }
                    }

                    LosersBracket[0].Score = teamRating;
                }
                else
                {
                    LosersBracket[0].Score = CurrentScore;
                }
            }
            else
            {
                throw new Exception("No winner of the tournament is defined"); 
            }
        }

    }
}