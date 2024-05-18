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

    protected LayerData LayerBTIndex;
    protected GridCell GridCell;
    protected SceneLoadMode SceneLoadMode;

    public class OnGameFinishedEventargs : EventArgs {
        public FitnessIndividual[] FitnessIndividuals;
        public int LayerId;
        public GridCell GridCell;
        public string ScenarioName; // GameSceneName + AgentSceneName
    }

    protected virtual void Awake() {
        GameState = GameState.IDLE;
        Util = GetComponent<Util>();

        SceneLoadMode = Communicator.Instance.SceneLoadMode;

        // TODO Check if this is OK
        //if (Environment != null)
        //    Environment.SetActive(false);

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
        DefineAdditionalDataOnPreStart();

        GetAgentBehaviourTrees(SceneLoadMode == SceneLoadMode.LayerMode? LayerBTIndex.BTIndex : GridCell.BTIndex);

        if (RandomAgentInitialization) {
            InitializeRandomAgents();
        }

        SetInitialData();

        DefineAgents();

        AssignBehaviourTrees();

        InitializeFitness(SceneLoadMode == SceneLoadMode.LayerMode ? LayerBTIndex.BTIndex : GridCell.BTIndex);

        DefineAdditionalDataOnPostStart();

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
            if(SceneLoadMode == SceneLoadMode.GridMode)
                spawnPos += GridCell.GridCellPosition;

            rotation = GetAgentRandomRotation();

            if (!SpawnPointSuitable(spawnPos, rotation, usedSpawnPoints)) {
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

    public bool RespawnPointSuitable(Vector3 newRespawnPos, Quaternion newRotation) {
        Collider agentCol = AgentPrefab.GetComponent<Collider>();

        Collider[] colliders = Physics.OverlapBox(newRespawnPos, agentCol.bounds.extents * 0.495f, newRotation, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)) + DefaultLayer);
        if (colliders.Length > 0) {
            return false;
        }

        foreach (var agent in Agents) {
            if (Vector3.Distance(newRespawnPos, agent.transform.position) < MinPlayerDistance) {
                return false;
            }
        }
        return true;
    }

    public virtual bool SpawnPointSuitable(Vector3 newSpawnPos, Quaternion newRotation, List<Vector3> usedSpawnPoints) {
        Collider agentCol = AgentPrefab.GetComponent<Collider>();

        Collider[] colliders = Physics.OverlapBox(newSpawnPos, agentCol.bounds.extents * 0.495f, newRotation, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)) + DefaultLayer);
        if(colliders.Length > 0) {
            return false;
        }

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
        Agents = GetComponentsInChildren<AgentComponent>();
        List<AgentComponent> activeAgents = new List<AgentComponent>();
        List<AgentComponent> activeAgentsPredefinedBehaviour = new List<AgentComponent>();

        for (int i = 0; i < Agents.Length; i++) {
            // Get only agents that are active
            if (Agents[i].gameObject.activeSelf) {
                if (Agents[i].HasPredefinedBehaviour)
                    activeAgentsPredefinedBehaviour.Add(Agents[i]);
                else
                    activeAgents.Add(Agents[i]);
            }
        }
        Agents = activeAgents.ToArray();
        AgentsPredefinedBehaviour = activeAgentsPredefinedBehaviour.ToArray();
    }

    void InitializeFitness(int BTIndex) {
        for (int i = 0; i < Agents.Length; i++) {
            switch (BTLoadMode) {
                case BTLoadMode.Single:
                    Agents[i].AgentFitness = new FitnessIndividual(BTIndex);
                    break;
                case BTLoadMode.Full:
                    Agents[i].AgentFitness = new FitnessIndividual(i);
                    break;
                case BTLoadMode.Custom:
                    // TODO impmlement
                    Agents[i].AgentFitness = new FitnessIndividual();
                    break;
            }
        }

        for (int i = 0; i < AgentsPredefinedBehaviour.Length; i++) {
            AgentsPredefinedBehaviour[i].AgentFitness = new FitnessIndividual(-2);
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

    protected virtual void DefineAdditionalDataOnPreStart() { }
    protected virtual void DefineAdditionalDataOnPostStart() { }
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
            string guid = Guid.NewGuid().ToString();
            // Send event about finished game
            OnGameFinished?.Invoke(this, new OnGameFinishedEventargs() {
                FitnessIndividuals = GetAgentFitnesses(),
                LayerId = gameObject.layer,
                GridCell = GridCell,
                ScenarioName = SceneLoadMode == SceneLoadMode.LayerMode ? LayerBTIndex.GameSceneName + "_" + LayerBTIndex.AgentSceneName + "_" + guid : GridCell.GameSceneName + "_" + GridCell.AgentSceneName + "_" + guid
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

    FitnessIndividual[] GetAgentFitnesses() {
        FitnessIndividual[] fitnessIndividuals = new FitnessIndividual[0];

         float minimizer = MinimizeResults ? -1.0f : 1.0f;

        // Based on BtLoadMode set fitness
        // BTLoadMode.Full -> Each agents fitness corresponds to one FitnesIndividual
        // BTLoadMode.Single -> All agents fitnesses corresponds to the same FitnessIndividual
        if (BTLoadMode == BTLoadMode.Full) {
            fitnessIndividuals = new FitnessIndividual[Agents.Length]; // Initialize FitnessIndividuals
            for (int i = 0; i < Agents.Length; i++) {
                Agents[i].AgentFitness.Fitness.SetFitness(Agents[i].AgentFitness.Fitness.GetFitness() * minimizer);

                fitnessIndividuals[i] = Agents[i].AgentFitness;
            }
        }
        else if (BTLoadMode == BTLoadMode.Single) {
            fitnessIndividuals = new FitnessIndividual[] { new FitnessIndividual() }; // Initialize one FitnessIndividual
            fitnessIndividuals[0].IndividualId = Agents[0].AgentFitness.IndividualId; // All agents have the same individual Id

            for (int i = 0; i < Agents.Length; i++) {
                Agents[i].AgentFitness.Fitness.SetFitness(Agents[i].AgentFitness.Fitness.GetFitness() * minimizer);

                foreach (KeyValuePair<string, float> fitness in Agents[i].AgentFitness.Fitness.IndividualValues) {
                    fitnessIndividuals[0].Fitness.UpdateFitness(fitness.Value, fitness.Key);
                }
            }

            fitnessIndividuals[0].Fitness.SetFitness(fitnessIndividuals[0].Fitness.GetFitness() * minimizer);
        }

        if(fitnessIndividuals.Length == 0) {
            UnityEngine.Debug.LogError("FitnessIndividuals not initialized");
        }

        return fitnessIndividuals;
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