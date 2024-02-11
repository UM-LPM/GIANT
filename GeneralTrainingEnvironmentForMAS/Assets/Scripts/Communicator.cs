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

public class Communicator : MonoBehaviour {

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

    [Header("Response Configuration")]
    [SerializeField] FitnessStatisticType FitnessStatisticType = FitnessStatisticType.Mean;

    [HideInInspector] public static Communicator Instance;

    private int CurrentIndividualID = 0;

    Layer Layer;
    Grid Grid;

    private int BTsLoaded;

    private HttpListener Listener;
    private Thread ListenerThread;

    private PopFitness PopFitness;

    private BehaviourTree[] PopBTs;


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
        Listener.Prefixes.Add("http://localhost:4444/");
        Listener.Start();

        ListenerThread = new Thread(StartListener);
        ListenerThread.Start();

        EnvironmentControllerBase.OnGameFinished += EnvironmentController_OnGameFinished;

        Time.timeScale = TimeScale;
    }

    private void EnvironmentController_OnGameFinished(object sender, EnvironmentControllerBase.OnGameFinishedEventargs e) {
        foreach (GroupFitness fitnessGroup in e.FitnessGroups) {
            foreach (FitnessIndividual fitnessIndividual in fitnessGroup.FitnessIndividuals) {
                if (fitnessIndividual.IndividualId < 0)
                    Debug.LogError("IndividualID is -1");
                PopFitness.Fitnesses[fitnessIndividual.IndividualId].Add(fitnessIndividual.Fitness.GetFitness());
            }
        }

        if (SceneLoadMode == SceneLoadMode.LayerMode)
            Layer.ReleaseLayer(e.LayerId);
        else if (SceneLoadMode == SceneLoadMode.GridMode)
            Grid.ReleaseGridCell(e.GridCell);
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

        // Load coresponding behaviour trees and order them by name 
        PopBTs = Resources.LoadAll<BehaviourTree>(BtSource);
        PopBTs = PopBTs.OrderBy(bt => bt.id).ToArray();

        // Reset variables
        PopFitness = new PopFitness(PopBTs.Length);

        CurrentIndividualID = 0;

        /////////////////////////////////////////////////////////////////
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
                                LoadAgentScenarioSingleLayerMode(scenario.AgentSceneName);
                                break;
                            case BTLoadMode.Full:
                                LoadAgentScenarioFullLayerMode(scenario.AgentSceneName);
                                break;
                            case BTLoadMode.Custom:
                                LoadAgentScenarioCustomLayerMode(scenario.AgentSceneName);
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
        else if(SceneLoadMode == SceneLoadMode.GridMode) {
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
                            LoadAgentScenarioSingleGridMode(scenario.AgentSceneName);
                            break;
                        case BTLoadMode.Full:
                            LoadAgentScenarioFullGridMode(scenario.AgentSceneName);
                            break;
                        case BTLoadMode.Custom:
                            LoadAgentScenarioCustomGridMode(scenario.AgentSceneName);
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

        /////////////////////////////////////////////////////////////////

        Debug.Log("PerformEvaluation function finished");

        // Based on FitnessStatisticType calculate fitness statistics
        CalculateFitnessStatistics();

        HttpServerResponse response = new HttpServerResponse() { PopFitness = PopFitness.FinalFitnesses };
        string responseJson = JsonUtility.ToJson(response);

        byte[] buffer = Encoding.UTF8.GetBytes(responseJson);
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.OutputStream.Close();
    }

    void CalculateFitnessStatistics() {
        switch (FitnessStatisticType) {
            case FitnessStatisticType.Mean:
                for (int i = 0; i < PopFitness.Fitnesses.Length; i++) {
                    PopFitness.FinalFitnesses[i] = PopFitness.Fitnesses[i].Average();
                }
                break;
            case FitnessStatisticType.StdDeviation:
                for (int i = 0; i < PopFitness.Fitnesses.Length; i++) {
                    double mean = PopFitness.Fitnesses[i].Average();
                    double variance = PopFitness.Fitnesses[i].Select(n => Math.Pow(n - mean, 2)).Average();
                    PopFitness.FinalFitnesses[i] = (float) Math.Sqrt(variance);
                }
                break;
            case FitnessStatisticType.Min:
                for (int i = 0; i < PopFitness.Fitnesses.Length; i++) {
                    PopFitness.FinalFitnesses[i] = PopFitness.Fitnesses[i].Min();
                }
                break;
            case FitnessStatisticType.Max:
                for (int i = 0; i < PopFitness.Fitnesses.Length; i++) {
                    PopFitness.FinalFitnesses[i] = PopFitness.Fitnesses[i].Max();
                }
                break;
        }
    }

    void LoadAgentScenarioSingleLayerMode(string agentSceneName) {
        while(BTsLoaded < PopBTs.Length && Layer.GetAndReserveAvailableLayer(BTsLoaded) >= 0) {
            SceneManager.LoadScene(agentSceneName, LoadSceneMode.Additive);
            BTsLoaded++;
        }
    }

    void LoadAgentScenarioFullLayerMode(string agentSceneName) {
        if (Layer.GetAndReserveAvailableLayer(-1) >= 0) {
            SceneManager.LoadScene(agentSceneName, LoadSceneMode.Additive);
            BTsLoaded = PopBTs.Length;
        }
    }

    void LoadAgentScenarioCustomLayerMode(string agentSceneName) {
        // TODO Custom loading scenario
    }

    void LoadAgentScenarioSingleGridMode(string agentSceneName) {
        while (BTsLoaded < PopBTs.Length && Grid.GetAndReserveAvailableGridlayer(BTsLoaded) != null) {
            SceneManager.LoadScene(agentSceneName, LoadSceneMode.Additive);
            BTsLoaded++;
        }
    }

    void LoadAgentScenarioFullGridMode(string agentSceneName) {
        if (Grid.GetAndReserveAvailableGridlayer(BTsLoaded) != null) {
            SceneManager.LoadScene(agentSceneName, LoadSceneMode.Additive);
            BTsLoaded = PopBTs.Length;
        }
    }

    void LoadAgentScenarioCustomGridMode(string agentSceneName) {
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

    public LayerBTIndex GetReservedLayer() {
        return Layer.GetReservedLayer();
    }

    public GridCell GetReservedGridCell() {
        return Grid.GetReservedGridCell();
    }
}

public class HttpServerResponse {
    public float[] PopFitness;
}

public class LayerBTIndex {
    public int LayerId;
    public int BTIndex;

    public LayerBTIndex(int layerId, int btIndex) {
        LayerId = layerId;
        BTIndex = btIndex;
    }
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