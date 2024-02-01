using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoccerEnvironmentController : EnvironmentControllerBase {

    [Header("Soccer configuration")]
    [SerializeField] float ForwardSpeed = 1f;
    [SerializeField] float LateralSpeed = 1f;
    [SerializeField] float AgentRunSpeed = 2f;
    [SerializeField] float AgentRotationSpeed = 100f;

    private float KickPower;
    private ActionBuffers ActionBuffer;

    protected override void OnUpdate() {
        OnGameInput();
    }

    public override void UpdateAgents() {
        if (ManualAgentControl) {
            MoveAgentsManualInput(Agents);
        }

        if(ManualAgentPredefinedBehaviourControl) {
            MoveAgentsManualInput(AgentsPredefinedBehaviour);
        }
    }

    void OnGameInput() {
        ActionBuffer = new ActionBuffers(null, new int[] { 0, 0, 0}); // Forward, Side, Rotate
        var discreteActionsOut = ActionBuffer.DiscreteActions;
        //forward
        if (Input.GetKey(KeyCode.W)) {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S)) {
            discreteActionsOut[0] = 2;
        }
        //rotate
        if (Input.GetKey(KeyCode.A)) {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.D)) {
            discreteActionsOut[1] = 2;
        }
        //right
        if (Input.GetKey(KeyCode.E)) {
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.Q)) {
            discreteActionsOut[2] = 2;
        }
    }

    void MoveAgentsManualInput(AgentComponent[] agents) {
        foreach (SoccerAgentComponent agent in agents) {
            if(agent.gameObject.activeSelf) {
                MoveAgent(agent, ActionBuffer);
            }
        }
    }


    public void MoveAgent(SoccerAgentComponent agent, ActionBuffers actionBuffer) {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        KickPower = 0f;

        var forwardAxis = actionBuffer.DiscreteActions[0];
        var rightAxis = actionBuffer.DiscreteActions[1];
        var rotateAxis = actionBuffer.DiscreteActions[2];

        switch (forwardAxis) {
            case 1:
                dirToGo = agent.transform.forward * ForwardSpeed;
                KickPower = 1f;
                break;
            case 2:
                dirToGo = agent.transform.forward * -ForwardSpeed;
                break;
        }

        switch (rightAxis) {
            case 1:
                dirToGo = agent.transform.right * -LateralSpeed;
                break;
            case 2:
                dirToGo = agent.transform.right * LateralSpeed;
                break;
        }

        switch (rotateAxis) {
            case 1:
                rotateDir = agent.transform.up * -1f;
                break;
            case 2:
                rotateDir = agent.transform.up * 1f;
                break;
        }

        agent.transform.Rotate(rotateDir, Time.deltaTime * AgentRotationSpeed);
        agent.Rigidbody.AddForce(dirToGo * AgentRunSpeed, ForceMode.VelocityChange);
    }
}
