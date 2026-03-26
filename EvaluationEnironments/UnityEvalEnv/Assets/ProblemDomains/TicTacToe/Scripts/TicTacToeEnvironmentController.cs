using Base;
using Configuration;
using System.Collections;
using System.Collections.Generic;
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

        private bool marksInRowAchieved = false;
        private int winIndividualID = -1;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();
            SetBestAndWorstFitnesses(TicTacToeFitness.FitnessValues);

            Grid = new TicTacToeGrid(BoardSizeX, BoardSizeY, BoardSizeZ);
            Grid.Spawn(GridCellPrefab, Environment.transform);

            MarkerAgentA = MarkerAgentAPrefab.GetComponent<TicTacToeMarker>();
            MarkerAgentB = MarkerAgentBPrefab.GetComponent<TicTacToeMarker>();

        }

        protected override void OnPostFixedUpdate()
        {
            CheckIfMarksInRowAchieved();
        }

        public void CheckIfMarksInRowAchieved()
        {
            (marksInRowAchieved, winIndividualID) = Grid.XInRowAchieved(MarksInARow);
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

        public override bool IsSimulationFinished()
        {
            return base.IsSimulationFinished() || marksInRowAchieved || Grid.AllMarkersPlaced();
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
                if(winIndividualID == -1)
                {
                    agent.AgentFitness.UpdateFitness(TicTacToeFitness.FitnessValues[TicTacToeFitness.FitnessKeys.Win.ToString()] / Agents.Length, TicTacToeFitness.FitnessKeys.Win.ToString());
                }
                else if (agent.IndividualID == winIndividualID)
                {
                    agent.AgentFitness.UpdateFitness(TicTacToeFitness.FitnessValues[TicTacToeFitness.FitnessKeys.Win.ToString()], TicTacToeFitness.FitnessKeys.Win.ToString());
                }

                if(agent.MarkersPlaced > 0)
                {
                    agent.AgentFitness.UpdateFitness(TicTacToeFitness.FitnessValues[TicTacToeFitness.FitnessKeys.PlaceMarker.ToString()], TicTacToeFitness.FitnessKeys.PlaceMarker.ToString());
                }
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
            }
        }
    }
}
