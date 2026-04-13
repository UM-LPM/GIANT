using Base;
using Configuration;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

namespace Problems.TicTacToe
{
    public class TicTacToeEnvironmentController : EnvironmentControllerBase
    {
        [Header("Tic Tac Toe General Configuration")]
        [SerializeField] public int BoardSizeX = 3;
        [SerializeField] public int BoardSizeY = 3;
        [SerializeField] public int BoardSizeZ = 1;
        [SerializeField] public int MarksInARow = 3;

        [Header("Tic Tac Toe Prefabs")]
        [SerializeField] public GameObject GridCellPrefab;
        [SerializeField] public GameObject MarkerAgentAPrefab;
        [SerializeField] public GameObject MarkerAgentBPrefab;

        [HideInInspector] public TicTacToeGrid Grid;

        private TicTacToeMarker MarkerAgentA;
        private TicTacToeMarker MarkerAgentB;

        public bool MarksInRowAchieved = false;
        public int WinIndividualID = -1;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();
            SetBestAndWorstFitnesses(TicTacToeFitness.FitnessValues);

            Grid = GetComponent<TicTacToeGrid>();
            Grid.Init(BoardSizeX, BoardSizeY, BoardSizeZ);
            Grid.Spawn(GridCellPrefab, Environment.transform);

            MarkerAgentA = MarkerAgentAPrefab.GetComponent<TicTacToeMarker>();
            MarkerAgentB = MarkerAgentBPrefab.GetComponent<TicTacToeMarker>();

        }

        protected override void OnPostFixedUpdate()
        {
            CheckIfMarksInRowAchieved();
            ResetAgentMarkerPlacedCurrentRound();
        }

        public void CheckIfMarksInRowAchieved()
        {
            (MarksInRowAchieved, WinIndividualID) = Grid.MarksInRowAchieved();
        }

        public void ResetAgentMarkerPlacedCurrentRound()
        {
            foreach (TicTacToeAgentComponent agent in Agents)
            {
                agent.MarkerPlacedCurrentRound = false;
            }
        }

        public TicTacToeMarker GetAgentMarker(TicTacToeAgentComponent agent)
        {
            if (Agents.Length != 2)
            {
                throw new System.Exception("There should be exactly 2 agents in the environment to determine markers");
            }

            if(agent == Agents[0])
            {
                MarkerAgentA.MarkerId = Agents[0].IndividualID;
                return MarkerAgentA;
            }
            else if (agent == Agents[1])
            {
                MarkerAgentB.MarkerId = Agents[1].IndividualID;
                return MarkerAgentB;
            }
            else
            {
                throw new System.Exception("Agent is not recognized in the environment");
            }
        }

        public int GetAgentOpponentMarkerID(int markerId)
        {
            if(markerId == Agents[0].IndividualID)
                return Agents[1].IndividualID;
            else
                return Agents[0].IndividualID;
        }

        public override bool IsSimulationFinished()
        {
            return base.IsSimulationFinished() || MarksInRowAchieved || Grid.AllMarkersPlaced();
        }

        protected override void OnPreFinishGame()
        {
            SetAgentsFitness();
        }

        private void SetAgentsFitness()
        {
            foreach (TicTacToeAgentComponent agent in Agents)
            {
                // Draw
                if (WinIndividualID == -1)
                {
                    agent.AgentFitness.UpdateFitness(TicTacToeFitness.FitnessValues[TicTacToeFitness.FitnessKeys.Draw.ToString()], TicTacToeFitness.FitnessKeys.Draw.ToString());
                }
                else if (agent.IndividualID == WinIndividualID)
                {
                    agent.AgentFitness.UpdateFitness(TicTacToeFitness.FitnessValues[TicTacToeFitness.FitnessKeys.Win.ToString()], TicTacToeFitness.FitnessKeys.Win.ToString());
                }

                if (agent.MarkersPlaced > 0)
                {
                    agent.AgentFitness.UpdateFitness(TicTacToeFitness.FitnessValues[TicTacToeFitness.FitnessKeys.PlaceMarker.ToString()], TicTacToeFitness.FitnessKeys.PlaceMarker.ToString());
                }

                if(agent.OpportunitiesCreated > 0)
                {
                    agent.AgentFitness.UpdateFitness(TicTacToeFitness.FitnessValues[TicTacToeFitness.FitnessKeys.OpportunitiesCreated.ToString()] * agent.OpportunitiesCreated, TicTacToeFitness.FitnessKeys.OpportunitiesCreated.ToString());
                }

                if(agent.OpponnentBlocked > 0)
                {
                    agent.AgentFitness.UpdateFitness(TicTacToeFitness.FitnessValues[TicTacToeFitness.FitnessKeys.OpponentBlocked.ToString()] * agent.OpponnentBlocked, TicTacToeFitness.FitnessKeys.OpponentBlocked.ToString());
                }

                if(agent.OptimalMoves > 0)
                {
                    agent.AgentFitness.UpdateFitness(TicTacToeFitness.FitnessValues[TicTacToeFitness.FitnessKeys.OptimalMove.ToString()] * agent.OptimalMoves, TicTacToeFitness.FitnessKeys.OptimalMove.ToString());
                }

                string agentFitnessLog = "========================================\n" +
                                  $"[Agent]: TeamID {agent.TeamIdentifier.TeamID}, ID: {agent.IndividualID} \n" +
                                  $"[Win]: {((WinIndividualID == agent.IndividualID) ? '1' : '0')}\n" +
                                  $"[Draw]: {((WinIndividualID == -1) ? '1' : '0')}\n" +
                                  $"[PlaceMarker]: {agent.MarkersPlaced} = {(agent.MarkersPlaced > 0 ? TicTacToeFitness.FitnessValues[TicTacToeFitness.FitnessKeys.PlaceMarker.ToString()] : '0')}\n" +
                                  $"[OpportunitiesCreated]: {agent.OpportunitiesCreated} = {TicTacToeFitness.FitnessValues[TicTacToeFitness.FitnessKeys.OpportunitiesCreated.ToString()] * agent.OpportunitiesCreated} \n" +
                                  $"[OpponentsBlocked]: {agent.OpponnentBlocked} = {TicTacToeFitness.FitnessValues[TicTacToeFitness.FitnessKeys.OpponentBlocked.ToString()] * agent.OpponnentBlocked} \n" +
                                  $"[OptimalMoves]: {agent.OptimalMoves} = {TicTacToeFitness.FitnessValues[TicTacToeFitness.FitnessKeys.OptimalMove.ToString()] * agent.OptimalMoves} \n";

                DebugSystem.LogVerbose(agentFitnessLog);
            }
        }

        public void ReadParamsFromMainConfiguration()
        {
            if (MenuManager.Instance != null && MenuManager.Instance.MainConfiguration != null)
            {
                MainConfiguration conf = MenuManager.Instance.MainConfiguration;

                TicTacToeFitness.FitnessValues = conf.FitnessValues;

                if (conf.ProblemConfiguration.ContainsKey("BoardSizeX"))
                {
                    BoardSizeX = int.Parse(conf.ProblemConfiguration["BoardSizeX"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("BoardSizeY"))
                {
                    BoardSizeY = int.Parse(conf.ProblemConfiguration["BoardSizeY"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("BoardSizeZ"))
                {
                    BoardSizeZ = int.Parse(conf.ProblemConfiguration["BoardSizeZ"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("MarksInARow"))
                {
                    MarksInARow = int.Parse(conf.ProblemConfiguration["MarksInARow"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("MctsIterations"))
                {
                    TicTacToeGrid.MCTS_ITERATIONS = int.Parse(conf.ProblemConfiguration["MctsIterations"]);
                }
                
                if (conf.ProblemConfiguration.ContainsKey("MinimaxMaxDepth"))
                {
                    TicTacToeGrid.MINIMAX_MAX_DEPTH = int.Parse(conf.ProblemConfiguration["MinimaxMaxDepth"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("BestMoveAlg"))
                {
                    var bestMoveAlg = conf.ProblemConfiguration["BestMoveAlg"];

                    if(bestMoveAlg == "MCTS")
                    {
                        TicTacToeGrid.BEST_MOVE_ALG = TicTacToeBestMoveAlgorithm.MCTS;
                    }
                    else if (bestMoveAlg == "Minimax")
                    {
                        TicTacToeGrid.BEST_MOVE_ALG = TicTacToeBestMoveAlgorithm.Minimax;
                    }
                    else if(bestMoveAlg == "Heuristic")
                    {
                        TicTacToeGrid.BEST_MOVE_ALG = TicTacToeBestMoveAlgorithm.Heuristic;
                    }
                }
            }
        }
    }
}
