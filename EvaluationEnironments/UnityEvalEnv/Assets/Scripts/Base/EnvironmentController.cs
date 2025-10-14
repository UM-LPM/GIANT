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
using AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController;

namespace Base
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Util))]
    public abstract class EnvironmentControllerBase : MonoBehaviour
    {

        [Header("Base Configuration")]
        [SerializeField] public ComponentSetupType EnvironmentControllerSetup = ComponentSetupType.MOCK;
        [SerializeField] public GameType GameType = GameType._3D;
        [SerializeField] public int SimulationSteps = 10000;
        [SerializeField] public float SimulationTime = 0f;
        [SerializeField] public LayerMask DefaultLayer = 0;
        [SerializeField] GameObject Environment;
        [SerializeField] public bool IncludeNodeCallFrequencyCounts = false;
        [HideInInspector] public bool ForceNewDecisions = false;

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
        private int DecisionRequestCount = 0;

        [Header("Match Configuration")]
        [SerializeField] public Match Match;

        [HideInInspector] public PhysicsScene PhysicsScene { get; set; }
        [HideInInspector] public PhysicsScene2D PhysicsScene2D { get; set; }

        public AgentComponent[] Agents { get; set; }
        public int CurrentSimulationSteps { get; set; }
        public float CurrentSimulationTime { get; set; }
        public GameState GameState { get; set; }
        public Util Util { get; set; }
        public ActionObservationProcessor ActionObservationProcessor { get; set; }
        public ActionExecutor ActionExecutor { get; set; }

        protected MatchSpawner MatchSpawner { get; set; }

        protected ActionBuffer ActionBuffer;


        private void Awake()
        {
            DefineAdditionalDataOnPreAwake();

            if (EnvironmentControllerSetup == ComponentSetupType.MOCK && Match == null)
            {
                throw new Exception("Match is not defined");
            }

            GameState = GameState.IDLE;
            Util = gameObject.GetComponent<Util>();

            MatchSpawner = gameObject.GetComponent<MatchSpawner>();
            ActionObservationProcessor = gameObject.GetComponent<ActionObservationProcessor>();
            if (ActionObservationProcessor == null)
            {
                DebugSystem.LogWarning("ActionObservationProcessor is not set!");
            }

            ActionExecutor = gameObject.GetComponent<ActionExecutor>();
            if (ActionExecutor == null)
            {
                throw new Exception("ActionExecutor is not set!");
            }

            if (EnvironmentControllerSetup == ComponentSetupType.REAL)
            {
                ReadParamsFromMainConfiguration();
            }

            DefineAdditionalDataOnPostAwake();
        }

        private void Start()
        {
            DefineAdditionalDataOnPreStart();

            Agents = MatchSpawner.Spawn<AgentComponent>(this);

            InitializeAgents();

            EnvironmentInitialization();

            DefineAdditionalDataOnPostStart();
        }

        private void Update()
        {
            OnUpdate();
        }

        public void OnStep()
        {
            OnPreFixedUpdate();

            if (ForceNewDecisions)
            {
                DecisionRequestCount = 0;
                ForceNewDecisions = false;
            }

            if (GameState == GameState.RUNNING)
            {
                if (DecisionRequestCount % DecisionRequestInterval == 0)
                {
                    UpdateAgents(true);
                }
                else
                {
                    UpdateAgents(false);
                }
            }

            CurrentSimulationTime += Time.fixedDeltaTime;
            CurrentSimulationSteps += 1;

            DecisionRequestCount += 1;

            OnPostFixedUpdate();
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
                    AgentControllers.AIAgentControllers.BehaviorTreeAgentController.RayHitObject.RAY_HIT_OBJECT_DETECTION_TYPE = (AgentControllers.AIAgentControllers.BehaviorTreeAgentController.RayHitObjectDetectionType)Enum.Parse(typeof(AgentControllers.AIAgentControllers.BehaviorTreeAgentController.RayHitObjectDetectionType), MainConfiguration.ProblemConfiguration["RayHitObjectDetectionType"]);
                    AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController.RayHitObject.RAY_HIT_OBJECT_DETECTION_TYPE = (AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController.RayHitObjectDetectionType)Enum.Parse(typeof(AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController.RayHitObjectDetectionType), MainConfiguration.ProblemConfiguration["RayHitObjectDetectionType"]);
                }

                if (MainConfiguration.ProblemConfiguration.ContainsKey("TargetGameObjects"))
                {
                    ConditionNode.TargetGameObjects = MainConfiguration.ProblemConfiguration["TargetGameObjects"].Split(',');
                    ActivatorNode.TargetGameObjects = MainConfiguration.ProblemConfiguration["TargetGameObjects"].Split(',');
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

                if (MainConfiguration.IncludeNodeCallFrequencyCounts)
                {
                    IncludeNodeCallFrequencyCounts = MainConfiguration.IncludeNodeCallFrequencyCounts;
                }
                else
                {
                    IncludeNodeCallFrequencyCounts = false;
                }

                // TODO Add support in the future
                //IncludeEncapsulatedNodesToFreqCount = conf.IncludeEncapsulatedNodesToFreqCount;
            }
        }

        void InitializeAgents()
        {
            for (int i = 0; i < Agents.Length; i++)
            {
                if (Agents[i].AgentController == null)
                {
                    throw new Exception("AgentController is not set!");
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
                    case ActivatorBasedBehaviorSystemAgentController:
                        ((ActivatorBasedBehaviorSystemAgentController)Agents[i].AgentController).Bind(ActivatorBasedBehaviorSystemAgentController.CreateActivatorBasedBehaviorSystemContext(Agents[i].gameObject));
                        break;
                    default:
                        throw new Exception("Unknown AgentController type!");
                }

                var sensor = Agents[i].GetComponentInChildren<Sensor>();
                if(sensor != null)
                {
                    sensor.PhysicsScene = PhysicsScene;
                    sensor.PhysicsScene2D = PhysicsScene2D;
                }

            }
        }

        void EnvironmentInitialization()
        {
            GameState = GameState.RUNNING;
            CurrentSimulationTime = 0f;
            CurrentSimulationSteps = 0;
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
            OnPreFinishGame();
            GameState = GameState.FINISHED;
        }

        public bool IsSimulationFinished()
        {
            if (SimulationTime > 0)
            {
                if (CurrentSimulationTime >= SimulationTime)
                {
                    return true;
                }
            }
            else if (SimulationSteps > 0)
            {
                if (CurrentSimulationSteps >= SimulationSteps)
                {
                    return true;
                }
            }
            return false;
        }

        public MatchFitness MapAgentFitnessesToMatchFitness()
        {
            MatchFitness matchFitness = new MatchFitness();
            matchFitness.MatchId = Match.MatchId;

            Guid guid = Guid.NewGuid();
            string scenarioName = guid.ToString();
            matchFitness.MatchName = Match.MatchId + "_" + scenarioName;

            for (int i = 0; i < Agents.Length; i++)
            {
                if (Agents[i].TeamIdentifier.TeamID >= 0)
                {
                    matchFitness.AddAgentFitness(Agents[i], IncludeNodeCallFrequencyCounts);
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
            if (getNewDecisions)
            {
                for (int i = 0; i < Agents.Length; i++)
                {
                    if (Agents[i].gameObject.activeSelf)
                    {
                        ActionBuffer = new ActionBuffer();
                        Agents[i].AgentController.GetActions(in ActionBuffer);
                        Agents[i].ActionBuffer = ActionBuffer;
                    }
                }
            }

            for (int i = 0; i < Agents.Length; i++)
            {
                if (Agents[i].ActionBuffer == null)
                {
                    throw new Exception("ActionBuffer is not set!");
                }

                if (Agents[i].gameObject.activeSelf)
                {
                    ActionExecutor.ExecuteActions(Agents[i]);
                }
            }
        }

        public virtual int GetNumOfActiveAgents()
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