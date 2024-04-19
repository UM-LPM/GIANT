using System;
using System.Collections;
using System.Net;
using System.Text;
using System.Threading;
using TheKiwiCoder;
using UnityEngine;
using UnityEngine.SceneManagement;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class Communicator : MonoBehaviour {

    [Header("HTTP Server Configuration")]
    [SerializeField] private string uri = "http://localhost:4444/";

    [Header("Scene Loading Configuration")]
    [SerializeField] public SceneLoadMode SceneLoadMode = SceneLoadMode.LayerMode;
    
    [Header("LayerMode Configuration")]
    [SerializeField] int MinLayerId = 6;
    [SerializeField] int MaxLayerId = 26;
    [SerializeField] private int BatchSize = 1;

    [Header("GridMode Configuration")]
    [SerializeField] Vector3Int GridSize = new Vector3Int(10, 0, 10);
    [SerializeField] Vector3Int GridSpacing = new Vector3Int(50, 0, 50);

    [Header("Scenes Configuration")]
    [SerializeField] private string BtSource;
    [SerializeField] GameScenario[] GameScenarios;
    [SerializeField] AgentScenario[] AgentScenarios;

    [Header("Execution Configuration")]
    [SerializeField] private float TimeScale = 1f;
    [SerializeField] private int RerunTimes = 1;

    [Header("Response Configuration")]
    [SerializeField] FitnessStatisticType FitnessStatisticType = FitnessStatisticType.Mean;

    [Header("Initial Seed Configuration")]
    [SerializeField] public int InitialSeed = 316227711;
    [SerializeField] bool RandomSeed = false;

    [HideInInspector] public static Communicator Instance;

    private int CurrentIndividualID = 0;

    Layer Layer;
    Grid Grid;

    private int BTsLoaded;

    private HttpListener Listener;
    private Thread ListenerThread;

    private PopFitness PopFitness;

    private BehaviourTree[] PopBTs;

    private int SimulationStepsCombined;


    private void Awake() {
        Layer = new Layer(MinLayerId, MaxLayerId, BatchSize);
        Grid = new Grid(GridSize, GridSpacing);
    }

    void Start() {
        if (Instance != null) {
            Destroy(this);
        }
        else {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        Listener = new HttpListener();
        Listener.Prefixes.Add(uri);
        Listener.Start();

        ListenerThread = new Thread(StartListener);
        ListenerThread.Start();

        EnvironmentControllerBase.OnGameFinished += EnvironmentController_OnGameFinished;

        Time.timeScale = TimeScale;
    }

    private void EnvironmentController_OnGameFinished(object sender, EnvironmentControllerBase.OnGameFinishedEventargs e) {
        foreach (FitnessIndividual fitnessIndividual in e.FitnessIndividuals) {
            if (fitnessIndividual.IndividualId < 0)
                Debug.LogError("IndividualID is -1");
            PopFitness.FitnessIndividuals[fitnessIndividual.IndividualId].Fitnesses.Add(e.ScenarioName, fitnessIndividual.Fitness);
        }

        if (SceneLoadMode == SceneLoadMode.LayerMode)
            Layer.ReleaseLayer(e.LayerId);
        else if (SceneLoadMode == SceneLoadMode.GridMode)
            Grid.ReleaseGridCell(e.GridCell);

        SimulationStepsCombined += e.SimulationSteps;
    }

    private void StartListener() {
        while (Listener.IsListening) {
            try {
                var context = Listener.GetContext(); // Block until a client connects.
                ThreadPool.QueueUserWorkItem(o => HandleRequest(context));
            }
            catch (HttpListenerException) {
                // The Listener was stopped, exit the loop
                break;
            }
        }
    }

    private void HandleRequest(HttpListenerContext context) {
        if (Layer.NumberOfUsedLayeres() > 0) {
            context.Response.StatusCode = 888;
            context.Response.OutputStream.Close();
            return;
        }

        UnityMainThreadDispatcher.Instance().Enqueue((PerformEvaluation(context)));
    }

    IEnumerator PerformEvaluation(HttpListenerContext context) {

        // Refresh the Asset Database
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
        // Configure the random seed
        if (RandomSeed)
            InitialSeed = new System.Random().Next();

        // Reset SimulationStepsCombined
        SimulationStepsCombined = 0;

        // Load coresponding behaviour trees and order them by name 
        PopBTs = Resources.LoadAll<BehaviourTree>(BtSource);
        PopBTs = PopBTs.OrderBy(bt => bt.id).ToArray();

        // Reset variables
        PopFitness = new PopFitness(PopBTs.Length);

        CurrentIndividualID = 0;

        /////////////////////////////////////////////////////////////////
        for (int i = 0; i < RerunTimes; i++) {
            if (SceneLoadMode == SceneLoadMode.LayerMode) {
                foreach (GameScenario gameScenario in GameScenarios) {
                    SceneManager.LoadScene(gameScenario.GameSceneName);

                    foreach (AgentScenario scenario in AgentScenarios) {
                        if (!scenario.ContainsGameScenario(gameScenario.GameSceneName))
                            continue;

                        BTsLoaded = 0;
                        // For each scenario all population must be evaluated
                        while (BTsLoaded < PopBTs.Length) {
                            while (!Layer.IsBatchExecuted(gameScenario.GameSceneName)) {
                                yield return null;
                            }

                            switch (scenario.BTLoadMode) {
                                case BTLoadMode.Single:
                                    LoadAgentScenarioSingleLayerMode(scenario.AgentSceneName, gameScenario.GameSceneName);
                                    break;
                                case BTLoadMode.Full:
                                    LoadAgentScenarioFullLayerMode(scenario.AgentSceneName, gameScenario.GameSceneName);
                                    break;
                                case BTLoadMode.Custom:
                                    LoadAgentScenarioCustomLayerMode(scenario.AgentSceneName, gameScenario.GameSceneName);
                                    break;
                            }

                            // Check if there is available layer
                            while (!Layer.CanUseAnotherLayer()) {
                                yield return null;
                            }
                        }
                    }

                    // Wait for all agent scenarios for current game scenario be finished before continuing to the other one (To prevent collision detection problems)
                    while (Layer.NumberOfUsedLayeres() > 0) {
                        yield return null;
                    }

                    SceneManager.UnloadSceneAsync(gameScenario.GameSceneName);
                }

                // Wait for all scene to finish when all different game and agent scenarios have been loaded
                while (Layer.NumberOfUsedLayeres() > 0) {
                    yield return null;
                }
            }
            else if (SceneLoadMode == SceneLoadMode.GridMode) {
                SceneManager.LoadScene(GameScenarios[0].GameSceneName); // Dummy scene for deterministic execution
                                                                        // In GridMode we don't load gameScenes as they are supposed to be inside of every scene
                foreach (AgentScenario scenario in AgentScenarios) {
                    BTsLoaded = 0;
                    // For each scenario all population must be evaluated
                    while (BTsLoaded < PopBTs.Length) {
                        while (!Grid.IsBatchExecuted(GameScenarios[0].GameSceneName)) {
                            yield return null;
                        }

                        switch (scenario.BTLoadMode) {
                            case BTLoadMode.Single:
                                LoadAgentScenarioSingleGridMode(scenario.AgentSceneName, GameScenarios[0].GameSceneName);
                                break;
                            case BTLoadMode.Full:
                                LoadAgentScenarioFullGridMode(scenario.AgentSceneName, GameScenarios[0].GameSceneName);
                                break;
                            case BTLoadMode.Custom:
                                LoadAgentScenarioCustomGridMode(scenario.AgentSceneName, GameScenarios[0].GameSceneName);
                                break;
                        }

                        // Check if there is available gridCell
                        while (!Grid.CanUseAnotherGridCell()) {
                            yield return null;
                        }
                    }
                }

                // Wait for all scene to finish when all different game and agent scenarios have been loaded
                while (Grid.NumberOfUsedGridCells() > 0) {
                    yield return null;
                }
                SceneManager.UnloadSceneAsync(GameScenarios[0].GameSceneName);
            }
        }

        /////////////////////////////////////////////////////////////////

        Debug.Log("PerformEvaluation function finished (Simulation steps: " + SimulationStepsCombined + ")");

        // Based on FitnessStatisticType calculate fitness statistics
        CalculateFitnessStatistics();

        HttpServerResponse response = new HttpServerResponse() { PopFitness = PopFitness.FitnessIndividuals };
        //string responseJson = JsonUtility.ToJson(response);
        string responseJson = JsonConvert.SerializeObject(response);

        byte[] buffer = Encoding.UTF8.GetBytes(responseJson);
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.OutputStream.Close();
    }

    void CalculateFitnessStatistics() {
        for (int i = 0; i < PopFitness.FitnessIndividuals.Length; i++) {
            PopFitness.FitnessIndividuals[i].FinalFitness = PopFitness.FitnessIndividuals[i].SumFitness();

            switch (FitnessStatisticType) {
                case FitnessStatisticType.Mean:
                    PopFitness.FitnessIndividuals[i].FinalFitnessStats = PopFitness.FitnessIndividuals[i].MeanFitness();
                    break;
                case FitnessStatisticType.StdDeviation:
                    PopFitness.FitnessIndividuals[i].FinalFitnessStats = PopFitness.FitnessIndividuals[i].StdDeviationFitness();
                    break;
                case FitnessStatisticType.Min:
                    PopFitness.FitnessIndividuals[i].FinalFitnessStats = PopFitness.FitnessIndividuals[i].MinFitness();
                    break;
                case FitnessStatisticType.Max:
                    PopFitness.FitnessIndividuals[i].FinalFitnessStats = PopFitness.FitnessIndividuals[i].MaxFitness();
                    break;
            }
        }
    }

    void LoadAgentScenarioSingleLayerMode(string agentSceneName, string gameSceneName) {
        while(BTsLoaded < PopBTs.Length && Layer.GetAndReserveAvailableLayer(BTsLoaded, gameSceneName, agentSceneName) >= 0) {
            SceneManager.LoadScene(agentSceneName, LoadSceneMode.Additive);
            BTsLoaded++;
        }
    }

    void LoadAgentScenarioFullLayerMode(string agentSceneName, string gameSceneName) {
        if (Layer.GetAndReserveAvailableLayer(-1, gameSceneName, agentSceneName) >= 0) {
            SceneManager.LoadScene(agentSceneName, LoadSceneMode.Additive);
            BTsLoaded = PopBTs.Length;
        }
    }

    void LoadAgentScenarioCustomLayerMode(string agentSceneName, string gameSceneName) {
        // TODO Custom loading scenario
    }

    void LoadAgentScenarioSingleGridMode(string agentSceneName, string gameSceneName) {
        while (BTsLoaded < PopBTs.Length && Grid.GetAndReserveAvailableGridlayer(BTsLoaded, gameSceneName, agentSceneName) != null) {
            SceneManager.LoadScene(agentSceneName, LoadSceneMode.Additive);
            BTsLoaded++;
        }
    }

    void LoadAgentScenarioFullGridMode(string agentSceneName, string gameSceneName) {
        if (Grid.GetAndReserveAvailableGridlayer(BTsLoaded, gameSceneName, agentSceneName) != null) {
            SceneManager.LoadScene(agentSceneName, LoadSceneMode.Additive);
            BTsLoaded = PopBTs.Length;
        }
    }

    void LoadAgentScenarioCustomGridMode(string agentSceneName, string gameSceneName) {
        // TODO Custom loading scenario
    }

    public bool ScenesLoaded(List<AsyncOperation> operations) {
        foreach (AsyncOperation op in operations) {
            if (!op.isDone) {
                return false;
            }
        }
        return true;
    }

    public int GetCurrentIndividualID() {
        return CurrentIndividualID++;
    }

    public BehaviourTree[] GetBehaviourTrees(BTLoadMode bTLoadMode, int startIndex = -1, int endIndex = -1) {
        switch (bTLoadMode) {
            case BTLoadMode.Single:
                return new BehaviourTree[]{ PopBTs[startIndex]};
            case BTLoadMode.Full:
                return PopBTs;
            case BTLoadMode.Custom:
                return new ArraySegment<BehaviourTree>(PopBTs, startIndex, endIndex - startIndex).ToArray();
        }

        Debug.LogError("GetBehaviourTrees returned null");
        return null;
    }

    public LayerData GetReservedLayer() {
        return Layer.GetReservedLayer();
    }

    public GridCell GetReservedGridCell() {
        return Grid.GetReservedGridCell();
    }
}

public class HttpServerResponse {
    public FitnessIndividual[] PopFitness { get; set; }
}

public enum FitnessStatisticType {
    Mean,
    StdDeviation,
    Min,
    Max
}

public enum SceneLoadMode {
    LayerMode,
    GridMode
}