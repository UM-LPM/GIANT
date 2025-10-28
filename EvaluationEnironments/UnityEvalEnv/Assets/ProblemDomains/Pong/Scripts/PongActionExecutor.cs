using AgentControllers;
using Base;
using UnityEngine;
using Utils;

namespace Problems.Pong
{
    public class PongActionExecutor : ActionExecutor
    {
        private PongEnvironmentController PongEnvironmentController;

        private void Awake()
        {
            PongEnvironmentController = GetComponentInParent<PongEnvironmentController>();
        }

        public override void ExecuteActions(AgentComponent agent)
        {
            MoveAgent(agent as PongAgentComponent);
        }

        void MoveAgent(PongAgentComponent agent)
        {
            var moveInput = Vector3.zero;

            var forwardAxis = agent.ActionBuffer.GetDiscreteAction("moveForwardDirection");

            // Movement direction
            switch (forwardAxis)
            {
                case 1: moveInput -= agent.Forward; break;      // forward
                case 2: moveInput += agent.Forward; break;      // backward
            }


            // compute new position and rotation
            Vector3 newAgentPos = agent.transform.position + moveInput.normalized * PongEnvironmentController.AgentSpeed * Time.fixedDeltaTime;

            // Collision check
            if (!PhysicsUtil.PhysicsOverlapBox(
                PongEnvironmentController.PhysicsScene,
                PongEnvironmentController.PhysicsScene2D,
                PongEnvironmentController.GameType,
                agent.gameObject,
                newAgentPos,
                agent.transform.rotation,
                new Vector3(
                    agent.BoxCollider2D.size.x,
                    0f,
                    0f
                    ),
                true,
                gameObject.layer))
            {
                agent.transform.position = newAgentPos;
            }
        }
    }
}
