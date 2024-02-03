using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SoccerEnvironmentController : EnvironmentControllerBase {

    [Header("Soccer configuration")]
    [SerializeField] float ForwardSpeed = 1f;
    [SerializeField] float LateralSpeed = 1f;
    [SerializeField] float AgentRunSpeed = 2f;
    [SerializeField] float AgentRotationSpeed = 100f;
    [SerializeField] public float KickPower = 2000f;
    [SerializeField] SoccerGameScenarioType GameScenarioType = SoccerGameScenarioType.GoldenGoal;
    [SerializeField] public static float VelocityPassTreshold = 0.2f;
    [SerializeField] float PassTolerance = 0.1f; // Tolerance in degrees.

    SoccerBallComponent SoccerBall;

    private ActionBuffer ActionBufferManual;
    GoalComponent GoalPurple;
    GoalComponent GoalBlue;

    protected override void DefineAdditionalDataOnAwake() {
        SoccerBall = GetComponentInChildren<SoccerBallComponent>();
        GoalBlue = FindObjectsByType<GoalComponent>(FindObjectsSortMode.InstanceID).Where(a => a.Team == Team.Blue).First();
        GoalPurple = FindObjectsByType<GoalComponent>(FindObjectsSortMode.InstanceID).Where(a => a.Team == Team.Purple).First();
    }

    protected override void OnUpdate() {
        OnGameInput();
    }

    public override void UpdateAgents() {
        if (ManualAgentControl) {
            MoveAgentsManualInput(Agents);
        }
        else {
            UpdateAgentsWithBTs(Agents);
        }

        if(ManualAgentPredefinedBehaviourControl) {
            MoveAgentsManualInput(AgentsPredefinedBehaviour);
        }
        else {
            UpdateAgentsWithBTs(AgentsPredefinedBehaviour);
        }
    }

    void OnGameInput() {
        ActionBufferManual = new ActionBuffer(null, new int[] { 0, 0, 0}); // Forward, Side, Rotate
        var discreteActionsOut = ActionBufferManual.DiscreteActions;
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
        if(ActionBufferManual.DiscreteActions.Length == 0)
            return;

        foreach (SoccerAgentComponent agent in agents) {
            if(agent.gameObject.activeSelf) {
                MoveAgent(agent, ActionBufferManual);
            }
        }
    }

    void UpdateAgentsWithBTs(AgentComponent[] agents) {
        ActionBuffer actionBuffer;
        foreach (SoccerAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf) {
                actionBuffer = new ActionBuffer(null, new int[] { 0, 0, 0}); // Forward, Side, Rotate

                agent.BehaviourTree.UpdateTree(actionBuffer);
                MoveAgent(agent, actionBuffer);
            }
        }
    }


    void MoveAgent(SoccerAgentComponent agent, ActionBuffer actionBuffer) {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        agent.KickPower = 0f;

        var forwardAxis = actionBuffer.DiscreteActions[0];
        var rightAxis = actionBuffer.DiscreteActions[1];
        var rotateAxis = actionBuffer.DiscreteActions[2];

        switch (forwardAxis) {
            case 1:
                dirToGo = agent.transform.forward * ForwardSpeed;
                agent.KickPower = 1f;
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

        agent.transform.Rotate(rotateDir, Time.fixedDeltaTime * AgentRotationSpeed);
        agent.Rigidbody.AddForce(dirToGo * AgentRunSpeed, ForceMode.VelocityChange);
    }

    public void GoalScored(SoccerAgentComponent striker, Team goalReceivingTeam) {
        if(striker.Team == goalReceivingTeam) {
            // Autogol was scored
            // Add penalty to all the agents that received the autogoal
            UpdateTeamFitness(Agents, goalReceivingTeam, SoccerFitness.AUTO_GOAL);
            UpdateTeamFitness(AgentsPredefinedBehaviour, goalReceivingTeam, SoccerFitness.AUTO_GOAL);
        }
        else {
            // Legit goal was scored
            // Add fitness only to the scorer but add penalty to all opponents
            striker.AgentFitness.Fitness.UpdateFitness(SoccerFitness.GOAL_SCORED);

            UpdateTeamFitness(Agents, goalReceivingTeam, SoccerFitness.GOAL_RECEIVED);
            UpdateTeamFitness(AgentsPredefinedBehaviour, goalReceivingTeam, SoccerFitness.GOAL_RECEIVED);
        }

        // Check engind state
        CheckEndingState();
    }

    public override void CheckEndingState() {
        if (GameScenarioType == SoccerGameScenarioType.GoldenGoal) {
            FinishGame();
        }
        else if (GameScenarioType == SoccerGameScenarioType.Match) {
            ResetAgentsPositions(Agents);
            ResetAgentsPositions(AgentsPredefinedBehaviour);
            ResetSoccerBall();
        }
    }

    void ResetAgentsPositions(AgentComponent[] agents) {
        foreach(SoccerAgentComponent agent in agents) {
            agent.transform.position = agent.StartPosition;
            agent.transform.rotation = agent.StartRotation;
            agent.Rigidbody.velocity = Vector3.zero;
        }
    }

    void ResetSoccerBall() {
        SoccerBall.transform.position = SoccerBall.StartPosition;
        SoccerBall.transform.rotation = SoccerBall.StartRotation;
        SoccerBall.Rigidbody.velocity = Vector3.zero;
        SoccerBall.Rigidbody.angularVelocity = Vector3.zero;
    }

    void UpdateTeamFitness(AgentComponent[] agents, Team team, float value) {
        foreach (SoccerAgentComponent agent in agents.Where(a => (a as SoccerAgentComponent).Team == team)) {
            agent.AgentFitness.Fitness.UpdateFitness(value);
        }
    }

    public void AgentTouchedSoccerBall(SoccerAgentComponent agent, SoccerBallComponent soccerBall) {
        GoalComponent goal = agent.Team == Team.Blue ? GoalPurple : GoalBlue;
        // Only update if agent intentionaly hit the ball
        if(Mathf.Abs(soccerBall.Rigidbody.velocity.x) > VelocityPassTreshold) {
            Vector3 directionToTarget = (goal.transform.position - soccerBall.transform.position).normalized;
            float dotProduct = Vector3.Dot(soccerBall.Rigidbody.velocity.normalized, directionToTarget);

            UnityEngine.Debug.Log(dotProduct);

            if (dotProduct > PassTolerance) {
                // The object is moving towards the target
                agent.AgentFitness.Fitness.UpdateFitness(SoccerFitness.PASS_TO_OPONENT_GOAL);
            }
            else if(dotProduct < -PassTolerance) {
                // The object is not moving towards the target
                agent.AgentFitness.Fitness.UpdateFitness(SoccerFitness.PASS_TO_OWN_GOAL);
            }
            else {
                // THe object is not moving in any dirrection
                agent.AgentFitness.Fitness.UpdateFitness(SoccerFitness.PASS);
            }
        }
    }
}

public enum Team {
    Blue,
    Purple
}

public enum SoccerGameScenarioType {
    GoldenGoal,
    Match
}
