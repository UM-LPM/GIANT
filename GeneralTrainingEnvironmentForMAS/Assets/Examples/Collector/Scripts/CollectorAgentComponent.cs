using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectorAgentComponent : AgentComponent {

    public Rigidbody Rigidbody { get; set; }
 
    public int TargetsAquired { get; set; }

    protected override void DefineAdditionalDataOnAwake() {
        Rigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Obstacle")) {
            if (CollectorFitness.FitnessValues[CollectorFitness.Keys[(int)CollectorFitness.FitnessKeys.AgentTouchedStaticObject]] != 0) {
                AgentFitness.Fitness.UpdateFitness((CollectorFitness.FitnessValues[CollectorFitness.Keys[(int)CollectorFitness.FitnessKeys.AgentTouchedStaticObject]]), CollectorFitness.FitnessKeys.AgentTouchedStaticObject.ToString());
                // Finish game ???
            }
        }
    }
}