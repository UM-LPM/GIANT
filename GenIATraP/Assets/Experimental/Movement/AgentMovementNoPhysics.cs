using AgentControllers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class AgentMovementNoPhysics : MonoBehaviour
{
    ActionBuffer ActionBufferManual;

    [SerializeField] float ForwardSpeed = 1f;
    [SerializeField] private float AgentRotationSpeed = 100f;
    [SerializeField] private float AgentRunSpeed = 2f;

    Quaternion predictedRotation;
    Vector3 predictedPosition;

    Vector3 agentExtends;
    float offset = 0.95f;

    private void Awake() {
        Collider collider = GetComponent<Collider>();
        agentExtends = new Vector3(collider.bounds.extents.x, collider.bounds.extents.y, collider.bounds.extents.z);
    }

    private void Update() {
        OnInput();
    }

    private void FixedUpdate() {

        MoveAgent();
    }

    void OnInput() {
        ActionBufferManual = new ActionBuffer(); // Forward,  Rotate
        //forward
        if (Input.GetKey(KeyCode.W)) {
            ActionBufferManual.AddDiscreteAction("moveForwardDirection", 1);
        }
        if (Input.GetKey(KeyCode.S)) {
            ActionBufferManual.AddDiscreteAction("moveForwardDirection", 2);
        }
        //rotate
        if (Input.GetKey(KeyCode.A)) {
            ActionBufferManual.AddDiscreteAction("rotateDirection", 1);
        }
        if (Input.GetKey(KeyCode.D)) {
            ActionBufferManual.AddDiscreteAction("rotateDirection", 2);
        }
    }

    void MoveAgent() {
        if (ActionBufferManual == null || ActionBufferManual.DiscreteActions == null || ActionBufferManual.DiscreteActions.Count == 0)
            return;

        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;


        var forwardAxis = ActionBufferManual.GetDiscreteAction("moveForwardDirection");
        var rotateAxis = ActionBufferManual.GetDiscreteAction("rotateDirection");

        switch (forwardAxis) {
            case 1:
                dirToGo = Vector3.forward * ForwardSpeed;
                break;
            case 2:
                dirToGo = Vector3.forward * -ForwardSpeed;
                break;
        }

        switch (rotateAxis) {
            case 1:
                rotateDir = Vector3.up * -1f;
                break;
            case 2:
                rotateDir = Vector3.up * 1f;
                break;
        }

        predictedRotation = Quaternion.Euler(rotateDir * Time.fixedDeltaTime * AgentRotationSpeed) * transform.rotation;

        // Calculate new position
        predictedPosition = transform.position + (predictedRotation * dirToGo * Time.fixedDeltaTime * AgentRunSpeed);

        if (AgentCanMove(predictedPosition, predictedRotation)) {
            transform.Rotate(rotateDir, Time.fixedDeltaTime * AgentRotationSpeed);
            transform.Translate(dirToGo * Time.fixedDeltaTime * AgentRunSpeed);
        }
    }

    bool AgentCanMove(Vector3 newPos, Quaternion newRotation) {
        // Adjust the size of the box to match the player
        Vector3 boxSize = agentExtends * offset; // Half the size of the player
        Visualizer.DisplayBox(newPos, boxSize, newRotation);

        Collider[] colliders = Physics.OverlapBox(newPos, boxSize, newRotation);
        foreach (Collider col in colliders) {
            if (col.gameObject != gameObject) {
                return false;
            }
        }

        return true;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        //Gizmos.DrawWireCube(predictedPosition, new Vector3(1,1,1));

        //Gizmos.matrix = Matrix4x4.TRS(transform.position, predictedRotation, transform.localScale);
        //Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
