using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheKiwiCoder;
using UnityEngine;

public class AgentComponent : MonoBehaviour {

    [field: SerializeField, Header("Base Agent Configuration")]
    public bool HasPredefinedBehaviour { get; set; }
    [field: SerializeField]
    public BehaviourTree BehaviourTree { get; set; }
    public FitnessIndividual AgentFitness { get; set; }

    private void Awake() {
        AgentFitness = new FitnessIndividual();

        DefineAdditionalDataOnAwake();
    }

    protected virtual void DefineAdditionalDataOnAwake() { }
}