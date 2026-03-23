using AgentControllers;
using Base;
using System.Linq;
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
            KickSoccerBall(agent as Soccer2DAgentComponent);
        }

        void MoveAgent(Soccer2DAgentComponent agent)
        {
            var moveInput = Vector3.zero;
            var rotateDir = 0f;

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

            float dt = Time.fixedDeltaTime;

            Vector3 movement = Vector3.zero;
            if (moveInput != Vector3.zero)
            {
                movement = moveInput.normalized * SoccerEnvironmentController.AgentMoveSpeed * dt;
            }

            // Compute new position & rotation
            Vector3 newAgentPos = agent.transform.position + movement;

            Quaternion newAgentRotation = Quaternion.Euler(
                0, 0,
                agent.transform.rotation.eulerAngles.z +
                rotateDir * SoccerEnvironmentController.AgentRotationSpeed * dt
            );

            // --- Collision handling (unchanged logic) ---
            var hits = PhysicsUtil.PhysicsCircleCast2D(
                SoccerEnvironmentController.PhysicsScene2D,
                agent.gameObject,
                agent.transform.position,
                SoccerEnvironmentController.AgentColliderExtendsMultiplier.x,
                movement.normalized,
                movement.magnitude,
                true,
                gameObject.layer
            );

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider.gameObject != gameObject)
                {
                    newAgentPos = hit.point + (hit.normal * SoccerEnvironmentController.AgentColliderExtendsMultiplier.x);
                    break;
                }
            }

            agent.transform.position = newAgentPos;
            agent.transform.rotation = newAgentRotation;
        }

        void KickSoccerBall(Soccer2DAgentComponent agent)
        {
            if (agent.ActionBuffer.GetDiscreteAction("kick") != 1)
                return;

            // Add kick cooldown logic
            if(agent.NextKickTime > SoccerEnvironmentController.CurrentSimulationSteps)
                return;

            var ball = SoccerEnvironmentController.SoccerBall;
            Vector3 toBall = (ball.transform.position - agent.transform.position);
            float distance = toBall.magnitude;

            // 1. Distance check
            if (distance > SoccerEnvironmentController.KickRange)
                return;

            // 2. Facing check (dot product)
            Vector3 forward = agent.transform.up;
            float alignment = Vector3.Dot(forward, toBall.normalized);

            if (alignment < SoccerEnvironmentController.KickAlignmentThreshold)
                return;

            // 3. Apply impulse
            Vector3 dir = Vector3.Lerp(forward, toBall.normalized, 0.3f).normalized;

            ball.AddForce(dir * SoccerEnvironmentController.KickForce);

            // Set last touched agent
            ball.LastTouchedAgent = agent;

            SoccerEnvironmentController.AgentTouchedSoccerBall(agent);

            agent.NextKickTime = SoccerEnvironmentController.CurrentSimulationSteps + SoccerEnvironmentController.KickCooldown;
        }
    }
}
