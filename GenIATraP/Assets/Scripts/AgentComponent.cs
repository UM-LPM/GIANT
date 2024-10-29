using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentControllers;
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using UnityEngine;

public abstract class AgentComponent : MonoBehaviour {

    [field: SerializeField, Header("Base Agent Configuration")]
    public bool HasPredefinedBehaviour { get; set; }
    [field: SerializeField]
    public BehaviorTreeAgentController BehaviourTree { get; set; } // TODO Replace this with a AgentController
    public FitnessIndividual AgentFitness { get; set; }
    public Vector3 StartPosition { get; set; } // TODO remove this (this must be defined in AgentScene or GameScene or EnvController)
    public Quaternion StartRotation{ get; set; } // TODO remove this (this must be defined in AgentScene or GameScene or EnvController)
    public List<Vector3> LastKnownPositions { get; set; }
    public ActionBuffer ActionBuffer { get; set; }

    // New properties
    [field: SerializeField]
    public AgentController AgentController { get; set; }
    public ActionExecutor ActionExecutor { get; set; }

    private void Awake() {
        AgentFitness = new FitnessIndividual();
        StartPosition = transform.position;
        StartRotation = transform.rotation;
        LastKnownPositions = new List<Vector3>();
        LastKnownPositions.Add(transform.position);

        ActionExecutor = GetComponent<ActionExecutor>();

        DefineAdditionalDataOnAwake();
    }

    private void Start()
    {
        DefineAdditionalDataOnStart();
    }

    protected virtual void DefineAdditionalDataOnAwake() { }
    protected virtual void DefineAdditionalDataOnStart() { }
}