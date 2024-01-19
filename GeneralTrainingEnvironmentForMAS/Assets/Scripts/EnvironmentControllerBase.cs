using System;
using System.Collections.Generic;
using TheKiwiCoder;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class EnvironmentControllerBase : MonoBehaviour {

    public static event EventHandler<OnGameFinishedEventargs> OnGameFinished;

    [SerializeField] public float SimulationTime = 10f;
    [SerializeField] public int IndividualId;
    [SerializeField] public bool Debug = false;
    [SerializeField] public BTLoadMode BTLoadMode;
    [SerializeField] public float AgentStartFitness;

    // Random agent Initializaion section START
    [SerializeField] public bool RandomAgentInitialization = false;
    [SerializeField] public GameObject AgentPrefab;
    [SerializeField] public Vector3 ArenaSize;
    [SerializeField] public float ArenaOffset = 3f;
    [SerializeField] public float MinPlayerDistance = 3f;
    // Random agent Initializaion section END

    protected BehaviourTree[] AgentBehaviourTrees;
    protected AgentComponent[] Agents;
    protected float CurrentSimulationTime;
    protected GameState GameState;
    protected Util Util;
    protected LayerBTIndex LayerBTIndex;

    public class OnGameFinishedEventargs : EventArgs {
        public GroupFitness[] FitnessGroups;
        public int LayerId;
    }

    protected virtual void Awake() {
        GameState = GameState.IDLE;
        Util = GetComponent<Util>();

        DefineAdditionalDataOnAwake();
    }

    protected virtual void Start() {
        LayerBTIndex = Communicator.Instance.GetReservedLayer();

        GetAgentBehaviourTrees();

        if (RandomAgentInitialization) {
            InitializeRandomAgents();
        }

        SetInitialData();

        DefineAgents();

        AssignBehaviourTrees();

        InitializeFitness();

        DefineAdditionalDataOnStart();

    }

    private void FixedUpdate() {
        CurrentSimulationTime += Time.fixedDeltaTime;

        // Check game termination criteria
        if (CurrentSimulationTime >= SimulationTime) {
            FinishGame();
        }

        if (GameState == GameState.RUNNING) {
            UpdateAgents();
        }

        OnFixedUpdate();
    }

    void GetAgentBehaviourTrees() {
        int startIndex = BTLoadMode == BTLoadMode.Single? LayerBTIndex.BTIndex : - 1; // TODO update this if BTLoadMode == Single | Custom 
        int endIdex = -1; // TODO update this if BTLoadMode == Custom
        AgentBehaviourTrees = Communicator.Instance.GetBehaviourTrees(BTLoadMode, startIndex, endIdex);
        if(AgentBehaviourTrees == null || AgentBehaviourTrees.Length == 0 ) {
            UnityEngine.Debug.LogError("AgentBehaviourTrees (GetAgentBehaviourTrees");
        }
    }

    void InitializeRandomAgents() {
        if (AgentPrefab == null)
            return;

        List<Vector3> usedSpawnPoints = new List<Vector3>();

        Vector3 spawnPos;
        Quaternion rotation;

        int counter = 0;
        while (counter < AgentBehaviourTrees.Length) {
            spawnPos = GetAgentRandomSpawnPoint();

            rotation = GetAgentRandomRotation();

            if (!SpawnPointSuitable(spawnPos, usedSpawnPoints)) {
                continue;
            }

            GameObject obj = Instantiate(AgentPrefab, spawnPos, rotation, gameObject.transform);
            AgentComponent agent = obj.GetComponent<AgentComponent>();
            agent.HasPredefinedBehaviour = false;
            usedSpawnPoints.Add(spawnPos);
            counter++;
        }
    }

    public Vector3 GetAgentRandomSpawnPoint() {
        return new Vector3 {
            x = Util.NextFloat(-(ArenaSize.x / 2) + ArenaOffset, (ArenaSize.x / 2) - ArenaOffset),
            y = ArenaSize.y,
            z = Util.NextFloat(-(ArenaSize.z / 2) + ArenaOffset, (ArenaSize.z / 2) - ArenaOffset),
        };
    }

    public Quaternion GetAgentRandomRotation() {
        return Quaternion.AngleAxis(Util.NextFloat(0, 360), new Vector3(0, 1, 0));
    }

    public bool RespawnPointSuitable(Vector3 newRespawnPos) {
        foreach(var agent in Agents) {
            if (Vector3.Distance(newRespawnPos, agent.transform.position) < MinPlayerDistance) {
                return false;
            }
        }
        return true;
    }

    public bool SpawnPointSuitable(Vector3 newSpawnPos, List<Vector3> usedSpawnPoints) {
        foreach (var usedSpawnPoint in usedSpawnPoints) {
            if (Vector3.Distance(newSpawnPos, usedSpawnPoint) < MinPlayerDistance) {
                return false;
            }
        }
        return true;
    }

    void SetInitialData() {
        GameState = GameState.RUNNING;
        CurrentSimulationTime = 0f;
        if (Communicator.Instance == null) {
            UnityEngine.Debug.LogError("Communicator instance not found");
            return;
        }
        IndividualId = Communicator.Instance.GetCurrentIndividualID();
        SetLayerRecursively(this.gameObject, LayerBTIndex.LayerId);
    }

    void DefineAgents() {
        Agents = FindObjectsOfType<AgentComponent>();
        List<AgentComponent> agentsInLayer = new List<AgentComponent>();

        for (int i = 0; i < Agents.Length; i++) {
            if (Agents[i].gameObject.layer == gameObject.layer) {
                agentsInLayer.Add(Agents[i]);
            }
        }
        Agents = agentsInLayer.ToArray();
    }

    void InitializeFitness() {
        for (int i = 0; i < Agents.Length; i++) {
            switch (BTLoadMode) {
                case BTLoadMode.Single:
                    Agents[i].AgentFitness = new FitnessIndividual(LayerBTIndex.BTIndex, AgentStartFitness);
                    break;
                case BTLoadMode.Full:
                    Agents[i].AgentFitness = new FitnessIndividual(i, AgentStartFitness);
                    break;
                case BTLoadMode.Custom:
                    // TODO impmlement
                    Agents[i].AgentFitness = new FitnessIndividual(AgentStartFitness);
                    break;
            }
        }
    }

    void AssignBehaviourTrees() {
        if(Communicator.Instance == null) {
            UnityEngine.Debug.LogError("Communicator instance not found");
            return;
        }
        BehaviourTree bt = AgentBehaviourTrees[0]; // = Communicator.Instance.GetBehaviourTree(IndividualId);

        for(int i = 0; i < Agents.Length; i++) {
            if (BTLoadMode == BTLoadMode.Full)
                bt = AgentBehaviourTrees[i];
            Agents[i].BehaviourTree = bt.Clone();
            Agents[i].BehaviourTree.Bind(BehaviourTree.CreateBehaviourTreeContext(Agents[i].gameObject));

        }
    }

    protected virtual void DefineAdditionalDataOnStart() { }
    protected virtual void DefineAdditionalDataOnAwake() { }
    protected virtual void OnFixedUpdate() { }

    void SetLayerRecursively(GameObject obj, int newLayer) {
        obj.layer = newLayer;
        //obj.GetComponent<Collider>().includeLayers = newLayer;
        foreach (Transform child in obj.transform) {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public void FinishGame() {
        if (GameState == GameState.RUNNING) {
            // Send event about finished game
            OnGameFinished?.Invoke(this, new OnGameFinishedEventargs() {
                FitnessGroups = GetAgentFitnesses(),
                LayerId = gameObject.layer
            });

            if (Debug) {
                for (int i = 0; i < Agents.Length; i++) { 
                    UnityEngine.Debug.Log(Agents[i].gameObject.transform.position);
                }
                for (int i = 0; i < Agents.Length; i++) { 
                    UnityEngine.Debug.Log(Agents[i].AgentFitness.Fitness.GetFitness());
                }
            }
        }

        GameState = GameState.FINISHED;

        // Unload scene asynchronously when game is finished
        SceneManager.UnloadSceneAsync(this.gameObject.scene);
    }

    GroupFitness[] GetAgentFitnesses() {
        GroupFitness[] FitnessGroups = new GroupFitness[1];
        FitnessGroups[0] = new GroupFitness(Agents.Length);

        for (int i = 0; i < Agents.Length; i++) {
            FitnessGroups[0].FitnessIndividuals[i] = Agents[i].AgentFitness;
        }

        return FitnessGroups;
    }

    public abstract void UpdateAgents();
}

public enum GameState {
    IDLE,
    RUNNING,
    FINISHED
}