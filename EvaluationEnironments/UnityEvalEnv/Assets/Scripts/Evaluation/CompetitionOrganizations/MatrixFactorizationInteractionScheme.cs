using AgentOrganizations;
using Base;
using Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace Evaluators.CompetitionOrganizations
{
    public class MatrixFactorizationInteractionScheme : CompetitionOrganization
    {
        List<Match> TournamentMatches = new List<Match>();
        int CurrentMatchID;
        List<int> freeOpponentTeamIDs;

        public MatrixFactorizationInteractionScheme(CompetitionTeamOrganizator teamOrganizator, Individual[][] individuals, bool regenerateTeamsEachRound, int rounds)
            : base(teamOrganizator, individuals, regenerateTeamsEachRound)
        {
            if (Teams.Length != 1)
            {
                throw new Exception("Invalid number of team groups! MatrixFactorizationInteractionScheme requires exactly 1 group of teams.");
            }

            Rounds = rounds == -1 ? Teams[0].Length -1 : rounds >  Teams[0].Length -1 ? Teams[0].Length -1 : rounds; // If rounds is not set round robin will be performed
            ExecutedRounds = 0;
            PlayedMatches = new List<MatchFitness>();
        }

        public override Match[] GenerateCompetitionMatches()
        {
            if (TeamsPerMatch != 2)
            {
                Debug.LogError("MatrixFactorizationInteractionScheme currently only supports 1v1 matches (TeamsPerMatch = 2). Set TeamsPerMatch to 2 and try again.");
                return new Match[] { };
            }

            if (IsCompetitionFinished())
                return new Match[] { };

            int N = Teams[0].Length;
            int targetMatches = (N * Rounds) / 2;

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

            // 3. Iterate through the shuffled pairs and add them to the tournament matches until we reach the target number of matches for the current round
            foreach (var (team1, team2) in pairs)
            {
                TournamentMatches.Add(
                        ScriptableObject.CreateInstance<Match>().Initialize(CurrentMatchID++, new Team[] { Teams[0][team1], Teams[0][team2] }));

                if (TournamentMatches.Count >= targetMatches)
                    break;
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

        public override void UpdateTeamsScore(List<MatchFitness> competitionMatchFitnesses, List<CompetitionPlayer> players = null)
        {
            // 1. Define matrix G with dimensions [number of teams x number of teams] and initialize with zeros
            double[, ] G = new double[Teams[0].Length, Teams[0].Length];
            bool[,] known = new bool[Teams[0].Length, Teams[0].Length];

            // 2. For each match fitness, fill the corresponding entry in G with the fitness value of team 1 against team 2 (e.g. G[team1Id, team2Id] = team1Fitness)
            foreach (MatchFitness matchFitness in competitionMatchFitnesses)
            {
                if(matchFitness.TeamFitnesses.Count != 2)
                {
                    Debug.LogError($"MatchFitness for match {matchFitness.MatchName} does not contain exactly 2 teams. This implementation of MatrixFactorizationInteractionScheme only supports 1v1 matches. Please check the match fitness data for inconsistencies.");
                    continue; // Skip this match fitness if it doesn't have exactly 2 teams
                }

                int team1Id = matchFitness.TeamFitnesses[0].TeamID;
                int team2Id = matchFitness.TeamFitnesses[1].TeamID;

                if(G[team1Id, team2Id] != 0)
                {
                    Debug.LogError($"Matrix G already contains a fitness value for teams {team1Id} and {team2Id}. This should not happen in a well-defined competition. Please check the match fitness data for duplicates or inconsistencies.");
                }

                // 1. Fill the matrix G and known
                G[team1Id, team2Id] = matchFitness.TeamFitnesses[0].GetTeamFitness();
                known[team1Id, team2Id] = true;

                G[team2Id, team1Id] = matchFitness.TeamFitnesses[1].GetTeamFitness();
                known[team2Id, team1Id] = true;


                // 2. Record individual match results
                // Team 1
                var team1 = Teams[0].Where(t => t.TeamId == team1Id).First();
                if(team1 == null)
                {
                    throw new Exception($"Invalid team ID in match fitness! Team ID {team1Id} does not match the ID of any team in the competition organization.");
                }

                var team1Opponents = Teams[0].Where(t => t.TeamId == team2Id).First().Individuals.Select(ind => ind.IndividualId).ToArray();

                team1.IndividualMatchResults.Add(new IndividualMatchResult()
                {
                    MatchName = matchFitness.MatchName,
                    OpponentsIDs = team1Opponents,
                    Value = matchFitness.TeamFitnesses[0].GetTeamFitness(),
                    IndividualValues = matchFitness.TeamFitnesses[0].GetTeamIndividualValues()
                });

                // Team 2
                var team2 = Teams[0].Where(t => t.TeamId == team2Id).First();
                var team2Opponents = Teams[0].Where(t => t.TeamId == team1Id).First().Individuals.Select(ind => ind.IndividualId).ToArray();

                team2.IndividualMatchResults.Add(new IndividualMatchResult()
                {
                    MatchName = matchFitness.MatchName,
                    OpponentsIDs = team2Opponents,
                    Value = matchFitness.TeamFitnesses[1].GetTeamFitness(),
                    IndividualValues = matchFitness.TeamFitnesses[1].GetTeamIndividualValues()
                });
            }

            // 3. Execute non-negative matrix factorization (NMF) to obtain W and H
            var mfis = new MFIS(
                Teams[0].Length,
                EnvironmentControllerBase.BEST_FITNESS,
                EnvironmentControllerBase.WORST_FITNESS,
                latentDim: 10,
                learningRate: 0.001,
                lambda: 0.0001,
                nmfIterations: 2000
            );

            MFISResult mfisResult = mfis.ComputeFitness(G, known, true);

            // 4. For each team calculate and set fitness as the average fitness (e.g. team1IdFitness = (G''[team1Id, team2Id] + G''[team1Id, team3Id] + ...) / teamSize)
            foreach(CompetitionTeam team in Teams[0])
            {
                int teamId = team.TeamId;
                double fitnessSum = 0;
                int count = 0;
                for (int opponentId = 0; opponentId < Teams[0].Length; opponentId++)
                {
                    if (teamId == opponentId) continue; // Skip self
                    fitnessSum += mfisResult.GFull[teamId, opponentId];
                    count++;
                }
                double averageFitness = count > 0 ? fitnessSum / count : 0;
                team.Score = (float)(averageFitness * (-1)); // Set team score with the average fitness
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

    public class MFIS
    {
        private readonly int nmfCompetitors;
        private readonly int latentDim;     // k, typically - log(lambda)
        private readonly double learningRate; // => y
        private readonly double lambda;     // regularization
        private readonly int nmfIterations;

        private readonly float minFitness;
        private readonly float maxFitness;

        public MFIS(int populationSize, float minFitness, float maxFitness, int latentDim = 10,
                    double learningRate = 0.001, double lambda = 0.0001, int nmfIterations = 2000)
        {
            this.nmfCompetitors = populationSize;
            this.minFitness = minFitness;
            this.maxFitness = maxFitness;
            this.latentDim = latentDim;
            this.learningRate = learningRate;
            this.lambda = lambda;
            this.nmfIterations = nmfIterations;
        }

        public MFISResult ComputeFitness(double[,] G, bool[,] known, bool normalizeFitness)
        {
            if (normalizeFitness)
            {
               // Normalize G to [0, 1] range for better NMF performance
                for (int i = 0; i < nmfCompetitors; i++)
                {
                    for (int j = 0; j < nmfCompetitors; j++)
                    {
                        if (known[i, j])
                        {
                            G[i, j] = (G[i, j] - minFitness) / (maxFitness - minFitness);
                        }
                    }
                }
            }

            // 1. Factorize (NMF)
            double[,] W = RandomMatrix(nmfCompetitors, latentDim);
            double[,] H = RandomMatrix(latentDim, nmfCompetitors);

            RunNmf(G, known, W, H);

            // 2. Reconstruct Ghat = WH
            double[,] Ghat = Multiply(W, H);

            // 3. Replace missing with predictions
            double[,] Gfull = new double[nmfCompetitors, nmfCompetitors];

            for (int i = 0; i < nmfCompetitors; i++)
            {
                for (int j = 0; j < nmfCompetitors; j++)
                {
                    Gfull[i, j] = known[i, j] ? G[i, j] : Ghat[i, j];

                    if (normalizeFitness)
                    {
                        // Denormalize back to original scale
                        Gfull[i, j] = Gfull[i, j] * (maxFitness - minFitness) + minFitness;
                        G[i, j] = known[i, j] ? G[i, j] * (maxFitness - minFitness) + minFitness : 0; // also denormalize the known values for consistency in output
                    }
                }
            }

            return new MFISResult(G, known, W, H, Gfull);
        }

        // ---------------- NMF Core ---------------- //

        private void RunNmf(double[,] G, bool[,] known, double[,] W, double[,] H)
        {
            for (int iter = 0; iter < nmfIterations; iter++)
            {
                for (int i = 0; i < nmfCompetitors; i++)
                {
                    for (int j = 0; j < nmfCompetitors; j++)
                    {
                        if (!known[i, j]) continue;

                        double prediction = Dot(W, H, i, j);
                        double error = G[i, j] - prediction;

                        for (int k = 0; k < latentDim; k++)
                        {
                            double wOld = W[i, k];
                            double hOld = H[k, j];

                            W[i, k] += learningRate * (2 * error * hOld - lambda * wOld);
                            H[k, j] += learningRate * (2 * error * wOld - lambda * hOld);

                            if (W[i, k] < 0) W[i, k] = 0;
                            if (H[k, j] < 0) H[k, j] = 0;
                        }
                    }
                }
            }
        }

        // ---------------- Utility Methods ---------------- //

        private double[,] RandomMatrix(int rows, int cols)
        {
            double[,] m = new double[rows, cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    m[i, j] = Coordinator.Instance.Random.NextDouble() * 0.1;

            return m;
        }

        private double Dot(double[,] W, double[,] H, int row, int col)
        {
            double sum = 0;
            for (int k = 0; k < latentDim; k++)
                sum += W[row, k] * H[k, col];
            return sum;
        }

        private double[,] Multiply(double[,] W, double[,] H)
        {
            int m = W.GetLength(0);
            int n = H.GetLength(1);
            int k = W.GetLength(1);

            double[,] result = new double[m, n];

            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    for (int z = 0; z < k; z++)
                        result[i, j] += W[i, z] * H[z, j];

            return result;
        }
    }

    public class MFISResult
    {
        public double[,] G { get; set; }
        public bool[,] Known { get; set; }
        public double[,] W { get; set; }
        public double[,] H { get; set; }

        public double[,] GFull { get; set; }

        public MFISResult(double[,] G, bool[,] known, double[,] W, double[,] H, double[,] GFull)
        {
            this.G = G;
            this.Known = known;
            this.W = W;
            this.H = H;
            this.GFull = GFull;
        }
    }
}