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
    [SerializeField] private string BtSource;
    [SerializeField] private string GameSceneName;

    [SerializeField] GameScenario[] GameScenarios;
    [SerializeField] AgentScenario[] AgentScenarios;

    [SerializeField] private float TimeScale = 1f;
    [SerializeField] private int BatchSize = 1;

    [SerializeField] int MinLayerId = 6;
    [SerializeField] int MaxLayerId = 26;

    [SerializeField] FitnessStatisticType FitnessStatisticType = FitnessStatisticType.Mean;

    [HideInInspector] public static Communicator Instance;

    private int CurrentIndividualID = 0;

    private LayerBTIndex[] LayerIds;
    // 0 - Available; 1 - Reserverd; 2 - In Use
    private int[] LayerAvailability;

    private int BTsLoaded;

    private HttpListener Listener;
    private Thread ListenerThread;

    private PopFitness PopFitness;

    private BehaviourTree[] PopBTs;

    private bool BatchExecuting;

    private void Awake() {
        // Initialize layer data
        LayerIds = new LayerBTIndex[MaxLayerId - MinLayerId + 1];
        LayerAvailability = new int[LayerIds.Length];

        for (int i = 0; i < LayerIds.Length; i++) {
            LayerIds[i] = new LayerBTIndex(MinLayerId + i, -1);
            LayerAvailability[i] = 0; // All are default available
        }
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
        BatchExecuting = false;
    }

    private void EnvironmentController_OnGameFinished(object sender, EnvironmentControllerBase.OnGameFinishedEventargs e) {
        foreach (GroupFitness fitnessGroup in e.FitnessGroups) {
            foreach (FitnessIndividual fitnessIndividual in fitnessGroup.FitnessIndividuals) {
                if (fitnessIndividual.IndividualId < 0)
                    Debug.LogError("IndividualID is -1");
                PopFitness.Fitnesses[fitnessIndividual.IndividualId].Add(fitnessIndividual.Fitness.GetFitness());
            }
        }

        ReleaseLayer(e.LayerId);
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
        if (NumberOfUsedLayeres() > 0) {
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
        foreach (GameScenario gameScenario in GameScenarios) {
            SceneManager.LoadScene(gameScenario.GameSceneName);

            foreach (AgentScenario scenario in AgentScenarios) {
                if (!scenario.ContainsGameScenario(gameScenario.GameSceneName))
                    continue;

                BTsLoaded = 0;
                // For each scenario all population must be evaluated
                while (BTsLoaded < PopBTs.Length) {
                    while (!IsBatchExecuted(gameScenario.GameSceneName)) {
                        yield return null;
                    }

                    switch (scenario.BTLoadMode) {
                        case BTLoadMode.Single:
                            LoadAgentScenarioSingle(scenario.AgentSceneName);
                            break;
                        case BTLoadMode.Full:
                            LoadAgentScenarioFull(scenario.AgentSceneName);
                            break;
                        case BTLoadMode.Custom:
                            LoadAgentScenarioCustom(scenario.AgentSceneName);
                            break;
                    }

                    // Check if there is available layer
                    while (!CanUseAnotherLayer()) {
                        BatchExecuting = true;
                        yield return null;
                    }
                }
            }

            // Wait for all agent scenarios for current game scenario be finished before continuing to the other one (To prevent collision detection problems)
            while (NumberOfUsedLayeres() > 0) {
                yield return null;
            }

            SceneManager.UnloadSceneAsync(gameScenario.GameSceneName);
        }

        // Wait for all scene to finish when all different game and agent scenarios have been loaded
        while (NumberOfUsedLayeres() > 0) {
            yield return null;
        }

        /////////////////////////////////////////////////////////////////

        Debug.Log("PerformEvaluation function finished");

        // Only unload game scene if There were any game scenarios
        //if(AgentScenarios.Length > 0 && PopBTs.Length > 0)
        //    SceneManager.UnloadSceneAsync(GameSceneName);

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

    bool IsBatchExecuted(string gameSceneName) {
        if(NumberOfUsedLayeres() == 0) {
            if(SceneManager.sceneCount > 1)
                SceneManager.UnloadSceneAsync(gameSceneName);
            SceneManager.LoadScene(gameSceneName);
        }
        return true;
    }

    void LoadAgentScenarioSingle(string agentSceneName) {
        while(BTsLoaded < PopBTs.Length && GetAndReserveAvailableLayer(BTsLoaded) >= 0) {
            SceneManager.LoadScene(agentSceneName, LoadSceneMode.Additive);
            BTsLoaded++;
        }
    }

    void LoadAgentScenarioFull(string agentSceneName) {
        if (GetAndReserveAvailableLayer(-1) >= 0) {
            SceneManager.LoadScene(agentSceneName, LoadSceneMode.Additive);
            BTsLoaded = PopBTs.Length;
        }
    }

    void LoadAgentScenarioCustom(string agentSceneName) {
        // TODO Implement in future
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


    int NumberOfUsedLayeres() {
        int counter = 0;
        foreach(int layerAvailability in LayerAvailability) {
            if(layerAvailability > 0)
                counter++;
        }
        return counter;
    }

    int GetAndReserveAvailableLayer(int BtIndex) {
        if (CanUseAnotherLayer()) {
            for (int i = 0; i < LayerAvailability.Length; i++) {
                if (LayerAvailability[i] == 0) {
                    LayerAvailability[i] = 1; // Set Layer to reserved
                    LayerIds[i].BTIndex = BtIndex;
                    return i;
                }
            }
        }
        return -1;
    }

    bool CanUseAnotherLayer() {
        if(NumberOfUsedLayeres() >= BatchSize)
            return false;
        return true;
    }

    void ReleaseLayer(int layerId) {
        for (int i = 0; i < LayerIds.Length; i++) {
            if (LayerIds[i].LayerId == layerId) {
                LayerAvailability[i] = 0;
            }
        }
    }

    public LayerBTIndex GetReservedLayer() {
        for (int i = 0; i < LayerAvailability.Length; i++) {
            if (LayerAvailability[i] == 1) {
                LayerAvailability[i] = 2;
                return LayerIds[i];
            }
        }
        Debug.LogError("GetReservedLayer() method returned null");
        return null;
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