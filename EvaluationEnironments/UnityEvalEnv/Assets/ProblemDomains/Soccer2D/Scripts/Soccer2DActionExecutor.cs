using AgentControllers;
using Base;
using UnityEngine;
using Utils;

namespace Problems.Soccer2D
{
    public class Soccer2DActionExecutor : ActionExecutor
    {
        private Soccer2DEnvironmentController SoccerEnvironmentController;

        private void Awake()
        {
            SoccerEnvironmentController = GetComponentInParent<Soccer2DEnvironmentController>();
        }

        public override void ExecuteActions(AgentComponent agent)
        {
            MoveAgent(agent as Soccer2DAgentComponent);
        }

        void MoveAgent(Soccer2DAgentComponent agent)
        {
            var moveInput = Vector3.zero;
            var rotateDir = 0f;

            agent.KickPower = 0f;

            var forwardAxis = agent.ActionBuffer.GetDiscreteAction("moveForwardDirection");
            var rightAxis = agent.ActionBuffer.GetDiscreteAction("moveSideDirection");
            var rotateAxis = agent.ActionBuffer.GetDiscreteAction("rotateDirection");

            // Movement direction
            switch (forwardAxis)
            {
                case 1: moveInput += agent.transform.up; break;      // forward
                case 2: moveInput -= agent.transform.up; break;      // backward
            }

            switch (rightAxis)
            {
                case 1: moveInput -= agent.transform.right; break;   // left
                case 2: moveInput += agent.transform.right; break;   // right
            }

            // Rotation
            switch (rotateAxis)
            {
                case 1: rotateDir = 1f; break;
                case 2: rotateDir = -1f; break;
            }

            // accelerate in movement direction
            if (moveInput != Vector3.zero)
            {
                agent.Velocity += moveInput.normalized * SoccerEnvironmentController.AgentAcceleration * Time.fixedDeltaTime;
                agent.Velocity = Vector3.ClampMagnitude(agent.Velocity, SoccerEnvironmentController.AgentMaxAcceleration);
            }
            else
            {
                // apply damping when no input
                agent.Velocity *= SoccerEnvironmentController.AgentMoveDamping;
            }

            // compute new position and rotation
            Vector3 newAgentPos = agent.transform.position + agent.Velocity * Time.fixedDeltaTime;
            Quaternion newAgentRotation = Quaternion.Euler(
                0, 0, agent.transform.rotation.eulerAngles.z + rotateDir * SoccerEnvironmentController.AgentRotationSpeed * Time.fixedDeltaTime
            );

            // Collision check
            if (!PhysicsUtil.PhysicsOverlapObject(
                SoccerEnvironmentController.PhysicsScene,
                SoccerEnvironmentController.PhysicsScene2D,
                SoccerEnvironmentController.GameType,
                agent.gameObject,
                newAgentPos,
                SoccerEnvironmentController.AgentColliderExtendsMultiplier.x,
                Vector3.zero,
                newAgentRotation,
                PhysicsOverlapType.OverlapSphere,
                true,
                gameObject.layer))
            {
                agent.transform.position = newAgentPos;
                agent.transform.rotation = newAgentRotation;
            }
        }
    }
}
