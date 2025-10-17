using AgentControllers;
using Base;
using Problems.Robostrike;
using UnityEngine;
using Utils;

namespace Problems.Collector
{
    public class DummyActionExecutor: ActionExecutor
    {
        private CollectorEnvironmentController CollectorEnvironmentController;

        // Move Agent variables
        Vector3 dirToGo;
        Vector3 rotateDir;
        int forwardAxis;
        int rotateAxis;

        Vector3 newAgentPos;
        Quaternion newAgentRotation;

        private void Awake()
        {
            CollectorEnvironmentController = GetComponentInParent<CollectorEnvironmentController>();
        }

        public override void ExecuteActions(AgentComponent agent)
        {
            MoveAgent(agent as CollectorAgentComponent);
        }

        private void MoveAgent(CollectorAgentComponent agent)
        {
            dirToGo = Vector3.zero;
            rotateDir = Vector3.zero;

            forwardAxis = agent.ActionBuffer.GetDiscreteAction("moveForwardDirection");
            rotateAxis = agent.ActionBuffer.GetDiscreteAction("rotateDirection");

            switch (forwardAxis)
            {
                case 1:
                    dirToGo = agent.transform.forward * CollectorEnvironmentController.ForwardSpeed;
                    break;
                case 2:
                    dirToGo = agent.transform.forward * -CollectorEnvironmentController.ForwardSpeed;
                    break;
            }

            switch (rotateAxis)
            {
                case 1:
                    rotateDir = agent.transform.up * -1f;
                    break;
                case 2:
                    rotateDir = agent.transform.up * 1f;
                    break;
            }

            // 1. Calculate the new position and rotation
            newAgentPos = agent.transform.position + (dirToGo * Time.fixedDeltaTime * CollectorEnvironmentController.AgentMoveSpeed);
            newAgentRotation = Quaternion.Euler(0, agent.transform.rotation.eulerAngles.y + rotateDir.y * Time.fixedDeltaTime * CollectorEnvironmentController.AgentRotationSpeed, 0);

            // 2. Check if agent can be moved and rotated without colliding to other objects
            if (!PhysicsUtil.PhysicsOverlapObject(CollectorEnvironmentController.PhysicsScene, CollectorEnvironmentController.PhysicsScene2D, CollectorEnvironmentController.GameType, agent.gameObject, newAgentPos, CollectorEnvironmentController.AgentColliderExtendsMultiplier.x, Vector3.zero, newAgentRotation, PhysicsOverlapType.OverlapSphere, true, gameObject.layer))
            {
                // 3. Move and rotate the agent
                agent.transform.position = newAgentPos;
                agent.transform.rotation = newAgentRotation;
            }

        }
    }
}
