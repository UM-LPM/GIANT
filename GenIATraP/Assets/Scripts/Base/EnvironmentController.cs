using System;
using System.Collections.Generic;
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using UnityEngine;
using UnityEngine.SceneManagement;
using AgentOrganizations;
using AgentControllers.AIAgentControllers.NeuralNetworkAgentController.ActionObservationCollectors;
using AgentControllers.AIAgentControllers.NeuralNetworkAgentController;
using Fitnesses;
using Spawners;
using Utils;
using AgentControllers;
using Configuration;
using Problems.Moba_game;

namespace Base
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Util))]
    public abstract class EnvironmentControllerBase : MonoBehaviour
    {

        public static event EventHandler<OnGameFinishedEventargs> OnGameFinished;

        [Header("Base Configuration")]
        [SerializeField] public ComponentSetupType EnvironmentControllerSetup = ComponentSetupType.MOCK;
        [SerializeField] public GameType GameType = GameType._3D;
        [SerializeField] public float SimulationSteps = 10000;
        [SerializeField] public float SimulationTime = 0f;
        [SerializeField] public LayerMask DefaultLayer = 0;
        [SerializeField] GameObject Environment;
        [SerializeField] public SceneLoadMode SceneLoadMode;

        [Header("Agent Initializaion Configuration")]
        [SerializeField] public GameObject AgentPrefab;
        [SerializeField] public Vector3 ArenaSize; // For square arena
        [SerializeField] public float ArenaRadius; // For square arena
        [SerializeField] public Vector3 ArenaCenterPoint; // For circular arena
        [SerializeField] public float ArenaOffset = 3f;
        [SerializeField] public float MinAgentDistance = 10f;
        [SerializeField] public Vector3 AgentColliderExtendsMultiplier = new Vector3(0.50f, 0.495f, 0.505f);
        [SerializeField] public bool RandomTeamColor = true;

        [Header("Decision Requests")]
        [Range(1, 100)]
        [Tooltip("Update agents BT every X steps (fixedUpdate() method call)")]
        [SerializeField] public int DecisionRequestInterval = 1;

        [Header("Match Configuration")]
        [SerializeField] public Match Match;

        public AgentComponent[] Agents { get; set; }
        public int CurrentSimulationSteps { get; set; }
        public float CurrentSimulationTime { get; set; }
        public GameState GameState { get; set; }
        public Util Util { get; set; }
        public ActionObservationProcessor ActionObservationProcessor { get; set; }
        public ActionExecutor ActionExecutor { get; set; }

        public LayerData LayerBTIndex { get; set; }
        public GridCell GridCell { get; set; }

        protected MatchSpawner MatchSpawner { get; set; }

        protected ActionBuffer ActionBuffer;


        private void Awake()
        {
            DefineAdditionalDataOnPreAwake();

            if (EnvironmentControllerSetup == ComponentSetupType.MOCK && Match == null)
            {
                throw new Exception("Match is not defined");
                // TODO Add error reporting here
            }

            GameState = GameState.IDLE;
            Util = gameObject.GetComponent<Util>();

            MatchSpawner = gameObject.GetComponent<MatchSpawner>();
            ActionObservationProcessor = gameObject.GetComponent<ActionObservationProcessor>();
            if (ActionObservationProcessor == null)
            {
                Debug.Log("ActionObservationProcessor is not set!");
                // TODO Add error reporting here
            }

            ActionExecutor = gameObject.GetComponent<ActionExecutor>();
            if (ActionExecutor == null)
            {
                throw new Exception("ActionExecutor is not set!");
                // TODO Add error reporting here
            }

            SetLayerGridData();

            if (EnvironmentControllerSetup == ComponentSetupType.REAL)
            {
                ReadParamsFromMainConfiguration();
            }

            DefineAdditionalDataOnPostAwake();
        }

        private void Start()
        {
            DefineAdditionalDataOnPreStart();

            if (EnvironmentControllerSetup == ComponentSetupType.REAL)
            {
                GetMatch(SceneLoadMode == SceneLoadMode.LayerMode ? LayerBTIndex.MatchIndex : GridCell.MatchIndex);
            }

            Agents = MatchSpawner.Spawn<AgentComponent>(this);

            InitializeAgents();

            EnvironmentInitialization();

            DefineAdditionalDataOnPostStart();
        }

        private void Update()
        {
            OnUpdate();
        }

        private void FixedUpdate()
        {
            OnPreFixedUpdate();

            // Check game termination criteria
            if (SimulationTime > 0)
            {
                if (CurrentSimulationTime >= SimulationTime)
                {
                    FinishGame();
                }
            }
            else if (SimulationSteps > 0)
            {
                if (CurrentSimulationSteps >= SimulationSteps)
                {
                    FinishGame();
                }
            }

            if (GameState == GameState.RUNNING)
            {
                if (CurrentSimulationSteps % DecisionRequestInterval == 0)
                {
                    UpdateAgents(true);
                }
                else
                {
                    //UpdateAgents(false);
                    UpdateAgents(true);
                }
            }

            CurrentSimulationTime += Time.fixedDeltaTime;
            CurrentSimulationSteps += 1;

            OnPostFixedUpdate();
        }

        void SetLayerGridData()
        {
            if (EnvironmentControllerSetup == ComponentSetupType.REAL)
            {
                SceneLoadMode = Communicator.Instance.SceneLoadMode;

                if (SceneLoadMode == SceneLoadMode.LayerMode)
                {
                    LayerBTIndex = Communicator.Instance.GetReservedLayer();
                    if (Environment != null)
                        Environment.SetActive(false);
                }
                else
                {
                    GridCell = Communicator.Instance.GetReservedGridCell();
                    transform.position = GridCell.GridCellPosition;
                    Environment.SetActive(true);
                }
            }
            else
            {
                if (SceneLoadMode == SceneLoadMode.LayerMode)
                {
                    LayerBTIndex = new LayerData(6, -1);
                }
                else
                {
                    GridCell = new GridCell(new Vector3(0, 0, 0)); ;
                }
            }
        }

        void ReadParamsFromMainConfiguration()
        {
            if (MenuManager.Instance != null && MenuManager.Instance.MainConfiguration != null)
            {
                MainConfiguration MainConfiguration = MenuManager.Instance.MainConfiguration;
                if (MainConfiguration.SimulationSteps > 0)
                {
                    SimulationSteps = MenuManager.Instance.MainConfiguration.SimulationSteps;
                    SimulationTime = 0;
                }
                else if (MainConfiguration.SimulationTime > 0)
                {
                    SimulationTime = MenuManager.Instance.MainConfiguration.SimulationTime;
                    SimulationSteps = 0;
                }

                if (MainConfiguration.ProblemConfiguration.ContainsKey("DecisionRequestInterval"))
                {
                    DecisionRequestInterval = int.Parse(MainConfiguration.ProblemConfiguration["DecisionRequestInterval"]);
                }

                if (MainConfiguration.ProblemConfiguration.ContainsKey("RayHitObjectDetectionType"))
                {
                    RayHitObject.RAY_HIT_OBJECT_DETECTION_TYPE = (RayHitObjectDetectionType)Enum.Parse(typeof(RayHitObjectDetectionType), MainConfiguration.ProblemConfiguration["RayHitObjectDetectionType"]);
                }

                if (MainConfiguration.ProblemConfiguration.ContainsKey("ArenaSizeX"))
                {
                    ArenaSize.x = float.Parse(MainConfiguration.ProblemConfiguration["ArenaSizeX"]);
                }

                if (MainConfiguration.ProblemConfiguration.ContainsKey("ArenaSizeZ"))
                {
                    ArenaSize.z = float.Parse(MainConfiguration.ProblemConfiguration["ArenaSizeZ"]);
                }

                if (MainConfiguration.ProblemConfiguration.ContainsKey("ArenaOffset"))
                {
                    ArenaOffset = float.Parse(MainConfiguration.ProblemConfiguration["ArenaOffset"]);
                }

                // TODO Add support in the future
                //IncludeEncapsulatedNodesToFreqCount = conf.IncludeEncapsulatedNodesToFreqCount;
            }
        }

        void GetMatch(int BTIndex)
        {
            Match = Communicator.Instance.GetMatch(BTIndex);
            if (Match == null)
            {
                throw new Exception("Match is not defined");
                // TODO Add error reporting here
            }
        }

        public void InitializeAgents()
        {
            for (int i = 0; i < Agents.Length; i++)
            {
                if (Agents[i].AgentController == null)
                {
                    throw new Exception("AgentController is not set!");
                    // TODO Add error reporting here
                }

                // Different initialization for different types of AgentControllers
                switch (Agents[i].AgentController)
                {
                    case NeuralNetworkAgentController:
                        // Problem specific initialization parameters
                        Agents[i].AgentController.Initialize(new Dictionary<string, object>
                    {
                        { "actionObservationProcessor", ActionObservationProcessor }
                    });
                        break;
                    case BehaviorTreeAgentController:
                        //Agents[i].AgentController = Agents[i].AgentController.Clone();
                        ((BehaviorTreeAgentController)Agents[i].AgentController).Bind(BehaviorTreeAgentController.CreateBehaviourTreeContext(Agents[i].gameObject));
                        ((BehaviorTreeAgentController)Agents[i].AgentController).InitNodeCallFrequencyCounter();
                        break;
                    case ManualAgentController:
                        break;
                    // TODO Implement other AgentController types
                    default:
                        throw new Exception("Unknown AgentController type!");
                        // TODO Add error reporting here
                }
            }
        }

        void EnvironmentInitialization()
        {
            GameState = GameState.RUNNING;
            CurrentSimulationTime = 0f;
            CurrentSimulationSteps = 0;

            SetLayerRecursively(gameObject, SceneLoadMode == SceneLoadMode.LayerMode ? LayerBTIndex.LayerId : GridCell.Layer);
        }

        public static void SetLayerRecursively(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        public void FinishGame()
        {
            if (GameState == GameState.RUNNING)
            {
                OnPreFinishGame();

                string guid = Guid.NewGuid().ToString();
                OnGameFinished?.Invoke(this, new OnGameFinishedEventargs()
                {
                    MatchFitness = MapAgentFitnessesToMatchFitness(),
                    SimulationSteps = CurrentSimulationSteps,
                    LayerId = gameObject.layer,
                    GridCell = GridCell,
                });
            }

            GameState = GameState.FINISHED;

            // Unload scene asynchronously when game is finished
            SceneManager.UnloadSceneAsync(gameObject.scene);
            UnityEditor.EditorApplication.isPlaying = false;
        }

        private MatchFitness MapAgentFitnessesToMatchFitness()
        {
            MatchFitness matchFitness = new MatchFitness();
            matchFitness.MatchId = Match.MatchId;

            Guid guid = Guid.NewGuid();
            string scenarioName = SceneLoadMode == SceneLoadMode.LayerMode ? LayerBTIndex.GameSceneName + "_" + LayerBTIndex.AgentSceneName + "_" + guid : GridCell.GameSceneName + "_" + GridCell.AgentSceneName + "_" + guid;
            matchFitness.MatchName = Match.MatchId + "_" + scenarioName;

            for (int i = 0; i < Agents.Length; i++)
            {
                if (Agents[i].TeamID >= 0)
                {
                    matchFitness.AddAgentFitness(Agents[i]);
                }
            }

            return matchFitness;
        }

        protected virtual void DefineAdditionalDataOnPreStart() { }
        protected virtual void DefineAdditionalDataOnPostStart() { }
        protected virtual void DefineAdditionalDataOnPreAwake() { }
        protected virtual void DefineAdditionalDataOnPostAwake() { }

        protected virtual void OnPreFixedUpdate() { }
        protected virtual void OnPostFixedUpdate() { }
        protected virtual void OnPreFinishGame() { }

        public virtual void CheckEndingState() { }

        protected virtual void OnUpdate() { }

        public virtual void UpdateAgents(bool getNewDecisions)
        {
            for (int i = 0; i < Agents.Length; i++)
            {
                if (Agents[i].gameObject.activeSelf)
                {
                    if (getNewDecisions)
                    {
                        ActionBuffer = new ActionBuffer();
                        Agents[i].AgentController.GetActions(in ActionBuffer);
                        Agents[i].ActionBuffer = ActionBuffer;
                    }

                    if (Agents[i].ActionBuffer == null)
                    {
                        throw new Exception("ActionBuffer is not set!");
                        // TODO Add error reporting here
                    }

                    ActionExecutor.ExecuteActions(Agents[i]);
                }
            }
        }

        public int GetNumOfActiveAgents()
        {
            int counter = 0;

            foreach (var agent in Agents)
            {
                if (agent.gameObject.activeSelf)
                    counter++;
            }

            return counter;
        }
    }

    public enum GameState
    {
        IDLE,
        RUNNING,
        FINISHED
    }
    public enum GameType
    {
        _2D,
        _3D
    }

    public enum ComponentSetupType
    {
        MOCK,
        REAL
    }
}