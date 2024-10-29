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

public abstract class EnvironmentControllerBase : MonoBehaviour {

    public static event EventHandler<OnGameFinishedEventargs> OnGameFinished;

    [Header("Base Configuration")]
    [SerializeField] public EnvironmentControllerSetupType EnvironmentControllerSetup = EnvironmentControllerSetupType.MOCK;
    [SerializeField] public GameType GameType = GameType._3D;
    [SerializeField] public LayerMask DefaultLayer = 0;
    [SerializeField] GameObject Environment;

    [Header("Agent Initializaion Configuration")]
    [SerializeField] public GameObject AgentPrefab;
    [SerializeField] public Vector3 ArenaSize; // For cube arena
    [SerializeField] public float ArenaRadius; // For circular arena
    [SerializeField] public Vector3 ArenaCenterPoint; // For circular arena
    [SerializeField] public float ArenaOffset = 3f;
    [SerializeField] public float MinAgentDistance = 10f;
    [SerializeField] public Vector3 AgentColliderExtendsMultiplier = new Vector3(0.50f, 0.495f, 0.505f);

    [Header("TODO Delete Configuration")]
    [SerializeField] public Match Match;

    [HideInInspector] public GameState GameState;
    [HideInInspector] public Util Util;

    public LayerData LayerBTIndex;
    public GridCell GridCell;
    public SceneLoadMode SceneLoadMode;

    public IndividualSpawner IndividualSpawner { get; set; }

    private void Awake()
    {
        DefineAdditionalDataOnPreAwake();

        // TODO Implement
        GameState = GameState.IDLE;
        Util = gameObject.GetComponent<Util>();
        IndividualSpawner = gameObject.GetComponent<IndividualSpawner>();

        DefineAdditionalDataOnPostAwake();
    }

    private void Start()
    {
        DefineAdditionalDataOnPreStart();

        // TODO Implement

        IndividualSpawner.SpawnIndividuals(this);


        DefineAdditionalDataOnPostStart();
    }

    private void Update()
    {
        OnUpdate();
    }

    private void FixedUpdate()
    {
        OnPreFixedUpdate();

        // TODO Implement

        OnPostFixedUpdate();
    }

    protected virtual void DefineAdditionalDataOnPreStart() { }
    protected virtual void DefineAdditionalDataOnPostStart() { }
    protected virtual void DefineAdditionalDataOnPreAwake() { }
    protected virtual void DefineAdditionalDataOnPostAwake() { }

    protected virtual void OnUpdate() { }

    protected virtual void OnPreFixedUpdate() { }
    protected virtual void OnPostFixedUpdate() { }
    protected virtual void OnPreFinishGame() { }
    public abstract void UpdateAgents(bool updateBTs); // Move updateBTs to somwhere else
}

public class OnGameFinishedEventargs : EventArgs
{
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

public enum EnvironmentControllerSetupType
{
    MOCK,
    REAL
}