using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheKiwiCoder;
using UnityEngine;

public abstract class AgentComponent : MonoBehaviour {

    [field: SerializeField, Header("Base Agent Configuration")]
    public bool HasPredefinedBehaviour { get; set; }
    [field: SerializeField]
    public BehaviourTree BehaviourTree { get; set; }
    public FitnessIndividual AgentFitness { get; set; }
    public Vector3 StartOffset { get; set; }
    public List<Vector3> LastKnownPositions { get; set; }
    public ActionBuffer ActionBuffer { get; set; }

    private void Awake() {
        AgentFitness = new FitnessIndividual();
        StartOffset = transform.parent.position;
        LastKnownPositions = new List<Vector3>();
        LastKnownPositions.Add(transform.position);

        DefineAdditionalDataOnAwake();
    }

    protected virtual void DefineAdditionalDataOnAwake() { }
}