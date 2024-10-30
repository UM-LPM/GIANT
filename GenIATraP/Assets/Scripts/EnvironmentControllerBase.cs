using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using AgentOrganizations;
using static Unity.VisualScripting.LudiqRootObjectEditor;
using AgentControllers.AIAgentControllers.NeuralNetworkAgentController.ObservationCollectors;
using AgentControllers.AIAgentControllers.NeuralNetworkAgentController;
using Fitnesses;

public abstract class EnvironmentControllerBase : MonoBehaviour {

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

    List<AgentComponent> Agents { get; set;}
    public int CurrentSimulationSteps { get; set; }
    public float CurrentSimulationTime { get; set; }
    public GameState GameState { get; set; }
    public Util Util { get; set; }
    public ActionObservationProcessor ActionObservationProcessor { get; set; }

    public LayerData LayerBTIndex { get; set; }
    public GridCell GridCell { get; set; }

    public IndividualSpawner IndividualSpawner { get; set; }

    private void Awake()
    {
        DefineAdditionalDataOnPreAwake();

        if(EnvironmentControllerSetup == ComponentSetupType.MOCK && Match == null)
        {
            throw new System.Exception("Match is not defined");
            // TODO Add error reporting here
        }

        GameState = GameState.IDLE;
        Util = gameObject.GetComponent<Util>();
        IndividualSpawner = gameObject.GetComponent<IndividualSpawner>();
        ActionObservationProcessor = gameObject.GetComponent<ActionObservationProcessor>();

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
            GetMatch(SceneLoadMode == SceneLoadMode.LayerMode ? LayerBTIndex.BTIndex : GridCell.BTIndex);
        }

        Agents = IndividualSpawner.SpawnIndividuals(this);

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
                UpdateAgents(false);
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
            MainConfiguration conf = MenuManager.Instance.MainConfiguration;
            if (conf.SimulationSteps > 0)
            {
                SimulationSteps = MenuManager.Instance.MainConfiguration.SimulationSteps;
                SimulationTime = 0;
            }
            else if (conf.SimulationTime > 0)
            {
                SimulationTime = MenuManager.Instance.MainConfiguration.SimulationTime;
                SimulationSteps = 0;
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
            throw new System.Exception("Match is not defined");
            // TODO Add error reporting here
        }
    }

    void InitializeAgents()
    {
        for (int i = 0; i < Agents.Count; i++)
        {
            if (Agents[i].AgentController == null)
            {
                throw new System.Exception("AgentController is not set!");
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
                // TODO Implement other AgentController types
                default:
                    throw new System.Exception("Unknown AgentController type!");
                    // TODO Add error reporting here
            }
        }
    }

    void EnvironmentInitialization()
    {
        GameState = GameState.RUNNING;
        CurrentSimulationTime = 0f;
        CurrentSimulationSteps = 0;

        SetLayerRecursively(this.gameObject, SceneLoadMode == SceneLoadMode.LayerMode ? LayerBTIndex.LayerId : GridCell.Layer);
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
            MatchFitness matchFitness = MapAgentFitnessesToMatchFitness(); // TODO Remove in the future
            OnGameFinished?.Invoke(this, new OnGameFinishedEventargs()
            {
                MatchFitness = MapAgentFitnessesToMatchFitness(),
                ScenarioName = SceneLoadMode == SceneLoadMode.LayerMode ? LayerBTIndex.GameSceneName + "_" + LayerBTIndex.AgentSceneName + "_" + guid : GridCell.GameSceneName + "_" + GridCell.AgentSceneName + "_" + guid,                                                                                                                                                                     //ScenarioName = SceneLoadMode == SceneLoadMode.LayerMode ? LayerBTIndex.GameSceneName + "_" + LayerBTIndex.AgentSceneName + "_" : GridCell.GameSceneName + "_" + GridCell.AgentSceneName + "_",
                SimulationSteps = CurrentSimulationSteps,
            });
        }

        GameState = GameState.FINISHED;

        // Unload scene asynchronously when game is finished
        SceneManager.UnloadSceneAsync(gameObject.scene);
    }

    private MatchFitness MapAgentFitnessesToMatchFitness()
    {
        MatchFitness matchFitness = new MatchFitness();
        matchFitness.MatchId = Match.MatchId;

        for (int i = 0; i < Agents.Count; i++)
        {
            matchFitness.AddAgentFitness(Agents[i]);
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

    protected virtual void OnUpdate() {
        // TODO Remove in the future
        if(Input.GetKeyDown(KeyCode.Space))
        {
            // Add random fitness to random agent
            int randomAgentIndex = Util.NextInt(0, Agents.Count);
            string[] fitnessKeys = new string[] { "RandomFitness1", "RandomFitness2", "RandomFitness3", "RandomFitness4", "RandomFitness5", "RandomFitness6" };
            int randomFitnessKeyIndex = Util.NextInt(0, fitnessKeys.Length);

            Dictionary<string, float> fitnessValues = new Dictionary<string, float>()
            {
                { "RandomFitness1", 1 },
                { "RandomFitness2", 2 },
                { "RandomFitness3", 3 },
                { "RandomFitness4", 4 },
                { "RandomFitness5", 5 },
                { "RandomFitness6", 6 },
            };

            Agents[randomAgentIndex].AgentFitness.UpdateFitness(fitnessValues[fitnessKeys[randomFitnessKeyIndex]], fitnessKeys[randomFitnessKeyIndex]);
            Debug.Log("Random fitness added to agent " + randomAgentIndex + " with key " + fitnessKeys[randomFitnessKeyIndex] + " and value " + fitnessValues[fitnessKeys[randomFitnessKeyIndex]]);
        }
    }

    public virtual void UpdateAgents(bool getNewDecisions){
        ActionBuffer actionBuffer;
        for (int i = 0; i < Agents.Count; i++)
        {
            if (getNewDecisions)
            {
                actionBuffer = new ActionBuffer();
                Agents[i].AgentController.GetActions(in actionBuffer);
                Agents[i].ActionBuffer = actionBuffer;
            }

            if(Agents[i].ActionBuffer == null)
            {
                throw new System.Exception("ActionBuffer is not set!");
                // TODO Add error reporting here
            }

            Agents[i].ActionExecutor.ExecuteActions(Agents[i].ActionBuffer);
        }
    }
}

public class OnGameFinishedEventargs : EventArgs
{
    public MatchFitness MatchFitness;
    public string ScenarioName;
    public int SimulationSteps;
}

public enum GameState {
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