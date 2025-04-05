using AgentControllers;
using Base;
using UnityEngine;

namespace Problems.Collector
{
    public class CollectorActionExecutor: ActionExecutor
    {
        private CollectorEnvironmentController CollectorEnvironmentController;

        // Move Agent variables
        Vector3 dirToGo;
        Vector3 rotateDir;
        int forwardAxis;
        int rotateAxis;

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

            // Movement Version 1 
            /*Agent.transform.Translate(dirToGo * Time.fixedDeltaTime * AgentMoveSpeed);
            Agent.transform.Rotate(rotateDir, Time.fixedDeltaTime * AgentRotationSpeed);*/

            // Movement Version 2
            agent.Rigidbody.MovePosition(agent.Rigidbody.position + (dirToGo * CollectorEnvironmentController.AgentMoveSpeed * Time.fixedDeltaTime));
            Quaternion turnRotation = Quaternion.Euler(0.0f, rotateDir.y * Time.fixedDeltaTime * CollectorEnvironmentController.AgentRotationSpeed, 0.0f);
            agent.Rigidbody.MoveRotation(agent.Rigidbody.rotation * turnRotation);
        }
    }
}
