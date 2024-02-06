using System;
using System.Collections.Generic;
using TheKiwiCoder;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class EnvironmentControllerBase : MonoBehaviour {

    public static event EventHandler<OnGameFinishedEventargs> OnGameFinished;

    [Header("Base Configuration")]
    [SerializeField] public float SimulationTime = 10f;
    [SerializeField] public int IndividualId;
    [SerializeField] public bool Debug = false;
    [SerializeField] public BTLoadMode BTLoadMode;
    [SerializeField] public float AgentStartFitness;
    [SerializeField] public bool MinimizeResults = true; // If true lower fitness is better
    [SerializeField] public LayerMask DefaultLayer = 0;
    [SerializeField] GameObject Environment;

    [Header("Random Agent Initializaion Configuration")]
    [SerializeField] public bool RandomAgentInitialization = false;
    [SerializeField] public GameObject AgentPrefab;
    [SerializeField] public Vector3 ArenaSize;
    [SerializeField] public float ArenaOffset = 3f;
    [SerializeField] public float MinPlayerDistance = 3f;

    [Header("Agent Control Configuration")]
    [SerializeField] public bool ManualAgentControl = false;
    [SerializeField] public bool ManualAgentPredefinedBehaviourControl = false;

    protected BehaviourTree[] AgentBehaviourTrees;
    protected AgentComponent[] Agents;
    protected AgentComponent[] AgentsPredefinedBehaviour;
    protected float CurrentSimulationTime;
    protected GameState GameState;
    protected Util Util;

    protected LayerBTIndex LayerBTIndex;
    protected GridCell GridCell;
    protected SceneLoadMode SceneLoadMode;

    public class OnGameFinishedEventargs : EventArgs {
        public GroupFitness[] FitnessGroups;
        public int LayerId;
        public GridCell GridCell;
    }

    protected virtual void Awake() {
        GameState = GameState.IDLE;
        Util = GetComponent<Util>();

        SceneLoadMode = Communicator.Instance.SceneLoadMode;

        if (SceneLoadMode == SceneLoadMode.LayerMode) {
            LayerBTIndex = Communicator.Instance.GetReservedLayer();
            if(Environment != null)
                Environment.SetActive(false);
        }
        else {
            GridCell = Communicator.Instance.GetReservedGridCell();
            transform.position = GridCell.GridCellPosition; // Check if this must go to Awake method
            Environment.SetActive(true);
        }

        DefineAdditionalDataOnAwake();
    }

    protected virtual void Start() {

        GetAgentBehaviourTrees(SceneLoadMode == SceneLoadMode.LayerMode? LayerBTIndex.BTIndex : GridCell.BTIndex);

        if (RandomAgentInitialization) {
            InitializeRandomAgents();
        }

        SetInitialData();

        DefineAgents();

        AssignBehaviourTrees();

        InitializeFitness(SceneLoadMode == SceneLoadMode.LayerMode ? LayerBTIndex.BTIndex : GridCell.BTIndex);

        DefineAdditionalDataOnStart();

    }

    private void Update() {
        OnUpdate();
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

    void GetAgentBehaviourTrees(int BTIndex) {
        int startIndex = BTLoadMode == BTLoadMode.Single? BTIndex : - 1; // TODO update this if BTLoadMode == Single | Custom 
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
        
        SetLayerRecursively(this.gameObject, SceneLoadMode == SceneLoadMode.LayerMode? LayerBTIndex.LayerId: GridCell.Layer);
    }

    void DefineAgents() {
        Agents = GetComponentsInChildren<AgentComponent>(); // FindObjectsOfType<AgentComponent>();
        List<AgentComponent> agentsInLayer = new List<AgentComponent>();
        List<AgentComponent> agentsPredefinedBehaviourInLayer = new List<AgentComponent>();

        for (int i = 0; i < Agents.Length; i++) {
            // Find agents that are on the same layer and don't have predefined behaviour
            //if (Agents[i].gameObject.layer == gameObject.layer) {
                if(Agents[i].HasPredefinedBehaviour)
                    agentsPredefinedBehaviourInLayer.Add(Agents[i]);
                else
                    agentsInLayer.Add(Agents[i]);
            //}
        }
        Agents = agentsInLayer.ToArray();
        AgentsPredefinedBehaviour = agentsPredefinedBehaviourInLayer.ToArray();
    }

    void InitializeFitness(int BTIndex) {
        for (int i = 0; i < Agents.Length; i++) {
            switch (BTLoadMode) {
                case BTLoadMode.Single:
                    Agents[i].AgentFitness = new FitnessIndividual(BTIndex, AgentStartFitness);
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

        for (int i = 0; i < AgentsPredefinedBehaviour.Length; i++) {
            AgentsPredefinedBehaviour[i].AgentFitness = new FitnessIndividual(-1, AgentStartFitness);
        }
    }

    void AssignBehaviourTrees() {
        if(Communicator.Instance == null) {
            UnityEngine.Debug.LogError("Communicator instance not found");
            return;
        }
        BehaviourTree bt = AgentBehaviourTrees[0];

        for(int i = 0; i < Agents.Length; i++) {
            if (BTLoadMode == BTLoadMode.Full)
                bt = AgentBehaviourTrees[i];
            Agents[i].BehaviourTree = bt.Clone();
            Agents[i].BehaviourTree.Bind(BehaviourTree.CreateBehaviourTreeContext(Agents[i].gameObject));

        }

        // Clone BTs of agents with predefined behaviour and bind them
        for (int i = 0; i < AgentsPredefinedBehaviour.Length; i++) {
            if(!ManualAgentPredefinedBehaviourControl) {
                if(AgentsPredefinedBehaviour[i].BehaviourTree == null) {
                    UnityEngine.Debug.LogError("Behaviour tree for agent with predefined behaviour not set");
                }
                else {
                    AgentsPredefinedBehaviour[i].BehaviourTree = AgentsPredefinedBehaviour[i].BehaviourTree.Clone();
                    AgentsPredefinedBehaviour[i].BehaviourTree.Bind(BehaviourTree.CreateBehaviourTreeContext(AgentsPredefinedBehaviour[i].gameObject));
                }
            }
        }
    }

    protected virtual void DefineAdditionalDataOnStart() { }
    protected virtual void DefineAdditionalDataOnAwake() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnFixedUpdate() { }

    public static void SetLayerRecursively(GameObject obj, int newLayer) {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform) {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public void FinishGame() {
        if (GameState == GameState.RUNNING) {
            // Send event about finished game
            OnGameFinished?.Invoke(this, new OnGameFinishedEventargs() {
                FitnessGroups = GetAgentFitnesses(),
                LayerId = gameObject.layer,
                GridCell = GridCell
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

         float minimizer = MinimizeResults ? -1.0f : 1.0f;

        // Based on BtLoadMode set fitness
        // BTLoadMode.Full -> Each agents fitness corresponds to one FitnesIndividual
        // BTLoadMode.Single -> All agents fitnesses corresponds to the same FitnessIndividual
        if (BTLoadMode == BTLoadMode.Full) {
            FitnessGroups[0] = new GroupFitness(Agents.Length);
            for (int i = 0; i < Agents.Length; i++) {
                Agents[i].AgentFitness.Fitness.SetFitness(Agents[i].AgentFitness.Fitness.GetFitness() * minimizer);

                FitnessGroups[0].FitnessIndividuals[i] = Agents[i].AgentFitness;
            }
        }
        else if (BTLoadMode == BTLoadMode.Single) {
            FitnessGroups[0] = new GroupFitness(1);
            FitnessIndividual fitnessIndividual = new FitnessIndividual();
            for (int i = 0; i < Agents.Length; i++) {
                Agents[i].AgentFitness.Fitness.SetFitness(Agents[i].AgentFitness.Fitness.GetFitness() * minimizer);

                fitnessIndividual.Fitness.UpdateFitness(Agents[i].AgentFitness.Fitness.GetFitness());
            }

            fitnessIndividual.IndividualId = Agents[0].AgentFitness.IndividualId; // All agents have the same individual Id
            FitnessGroups[0].FitnessIndividuals[0] = fitnessIndividual;
        }

        return FitnessGroups;
    }

    public int GetNumOfActiveAgents() {
        int counter = 0;

        foreach (var agent in Agents) {
            if (agent.gameObject.activeSelf)
                counter++;
        }

        foreach (var agent in AgentsPredefinedBehaviour) {
            if (agent.gameObject.activeSelf)
                counter++;
        }

        return counter;
    }

    public virtual void CheckEndingState() {
        if (GetNumOfActiveAgents() == 1) {
            FinishGame();
        }
    }

    public abstract void UpdateAgents();
}

public enum GameState {
    IDLE,
    RUNNING,
    FINISHED
}