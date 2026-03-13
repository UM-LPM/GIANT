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
        int currentMatchID;
        List<int> freeOpponentTeamIDs;

        public MatrixFactorizationInteractionScheme(CompetitionTeamOrganizator teamOrganizator, Individual[] individuals, bool regenerateTeamsEachRound, int rounds)
            : base(teamOrganizator, individuals, regenerateTeamsEachRound)
        {
            Rounds = rounds == -1 ? Teams.Count -1 : rounds >  Teams.Count -1 ? Teams.Count -1 : rounds; // If rounds is not set round robin will be performed
            ExecutedRounds = 0;
            PlayedMatches = new List<MatchFitness>();
        }

        public override void ResetCompetition()
        {
            Teams.Clear();
            ExecutedRounds = 0;
            PlayedMatches.Clear();
        }

        public override Match[] GenerateCompetitionMatches()
        {
            if (TeamsPerMatch != 2)
            {
                Debug.LogError("MatrixFactorizationInteractionScheme currently only supports 1v1 matches (TeamsPerMatch = 2). Please set TeamsPerMatch to 2 and try again.");
                return new Match[] { };
            }

            if (IsCompetitionFinished())
                return new Match[] { };

            TournamentMatches.Clear();
            currentMatchID = 0;

            for (int i = 0; i < Teams.Count; i++)
            {
                // Get all Team IDs except the current team and teams that are in matchedOpponentTeamIDs
                freeOpponentTeamIDs = Teams.Where(team => team.TeamId != Teams[i].TeamId)
                    .Select(team => team.TeamId).ToList();

                // If there are no free opponents left, the team has played against all other teams
                if (freeOpponentTeamIDs.Count == 0)
                    continue;

                for (int j = 0; j < Rounds; j++)
                {
                    int randomIndex = Coordinator.Instance.Random.Next(0, freeOpponentTeamIDs.Count);
                    int opponentTeamID = freeOpponentTeamIDs[randomIndex];
                    freeOpponentTeamIDs.RemoveAt(randomIndex);

                    /*if (Coordinator.Instance.Random.NextDouble() > 0.5)
                        TournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(currentMatchID++, new Team[] { Teams[i], Teams.Find(team => team.TeamId == opponentTeamID) }));
                    else
                        TournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(currentMatchID++, new Team[] { Teams.Find(team => team.TeamId == opponentTeamID), Teams[i] }));
                    */
                    TournamentMatches.Add(ScriptableObject.CreateInstance<Match>().Initialize(currentMatchID++, new Team[] { Teams[i], Teams.Find(team => team.TeamId == opponentTeamID) }));
                }
            }

            // No need to shuffle TournamentMatches randomly or add swapped matches, since the matrix handles both 

         
            return TournamentMatches.ToArray();
        }

        public override void UpdateTeamsScore(List<MatchFitness> competitionMatchFitnesses, List<CompetitionPlayer> players = null)
        {
            // 1. Define matrix G with dimensions [number of teams x number of teams] and initialize with zeros
            double[, ] G = new double[Teams.Count, Teams.Count];
            bool[,] known = new bool[Teams.Count, Teams.Count];

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

                // 2. Record individual match results
                var team = Teams.Find(t => t.TeamId == team1Id);
                var opponents = Teams.Find(t => t.TeamId == team2Id).Individuals.Select(ind => ind.IndividualId).ToArray();

                team.IndividualMatchResults.Add(new IndividualMatchResult()
                {
                    MatchName = matchFitness.MatchName,
                    OpponentsIDs = opponents,
                    Value = matchFitness.TeamFitnesses[0].GetTeamFitness(),
                    IndividualValues = matchFitness.TeamFitnesses[0].GetTeamIndividualValues()
                });
            }

            // 3. Execute non-negative matrix factorization (NMF) to obtain W and H
            var mfis = new MFIS(
                Teams.Count,
                EnvironmentControllerBase.BEST_FITNESS,
                EnvironmentControllerBase.WORST_FITNESS,
                latentDim: 10,
                learningRate: 0.001,
                lambda: 0.0001,
                nmfIterations: 2000
            );

            MFISResult mfisResult = mfis.ComputeFitness(G, known, true);

            // 4. For each team calculate and set fitness as the average fitness (e.g. team1IdFitness = (G''[team1Id, team2Id] + G''[team1Id, team3Id] + ...) / teamSize)
            foreach(CompetitionTeam team in Teams)
            {
                int teamId = team.TeamId;
                double fitnessSum = 0;
                int count = 0;
                for (int opponentId = 0; opponentId < Teams.Count; opponentId++)
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