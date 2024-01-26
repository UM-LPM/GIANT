using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheKiwiCoder;
using UnityEngine;

public class AgentComponent : MonoBehaviour {

    public BehaviourTree BehaviourTree { get; set; }

    public FitnessIndividual AgentFitness { get; set; }

    public bool HasPredefinedBehaviour { get; set; }

    private void Awake() {
        AgentFitness = new FitnessIndividual();

        DefineAdditionalDataOnAwake();
    }

    // Ramming damage ? TODO ???
    /*void OnCollisionEnter(Collision collision) {
        // The relative velocity of the collision
        Vector3 collisionVelocity = collision.relativeVelocity;

        // The strength of the collision
        float collisionStrength = collisionVelocity.magnitude;

        // The GameObject that collided with this one
        GameObject otherGameObject = collision.gameObject;

        // The MoveDirection of the collision
        Vector3 collisionDirection = collision.contacts[0].normal;

        if (otherGameObject.name.Contains("Agent")) {
            // Print the collision information
            Debug.Log("Collision strength: " + collisionStrength);
            Debug.Log("Collided with: " + otherGameObject.name);
            Debug.Log("Collision MoveDirection: " + collisionDirection);
        }

        if (Vector3.Dot(collisionDirection, transform.forward) < 0) {
            Debug.Log(gameObject.name + " hit " + otherGameObject.name);
        }
        else {
            Debug.Log(otherGameObject.name + " hit " + gameObject.name);
        }
    }*/

    protected virtual void DefineAdditionalDataOnAwake() { }
}