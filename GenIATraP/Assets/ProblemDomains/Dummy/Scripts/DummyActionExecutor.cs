using AgentControllers;
using UnityEngine;

namespace Problems.Dummy
{
    public class DummyActionExecutor: ActionExecutor
    {
        private DummyEnvironmentController DummyEnvironmentController;
        private DummyAgentComponent Agent;

        private void Awake()
        {
            Agent = GetComponent<DummyAgentComponent>();
            DummyEnvironmentController = GetComponentInParent<DummyEnvironmentController>();
        }

        public override void ExecuteActions(ActionBuffer actionBuffer)
        {
            MoveAgent(actionBuffer);
        }

        private void MoveAgent(ActionBuffer actionBuffer)
        {
            var dirToGo = Vector3.zero;
            var rotateDir = Vector3.zero;

            var forwardAxis = actionBuffer.GetDiscreteAction("moveForwardDirection");
            var rotateAxis = actionBuffer.GetDiscreteAction("rotateDirection");

            switch (forwardAxis)
            {
                case 1:
                    dirToGo = Agent.transform.forward * DummyEnvironmentController.ForwardSpeed;
                    break;
                case 2:
                    dirToGo = Agent.transform.forward * -DummyEnvironmentController.ForwardSpeed;
                    break;
            }

            switch (rotateAxis)
            {
                case 1:
                    rotateDir = Agent.transform.up * -1f;
                    break;
                case 2:
                    rotateDir = Agent.transform.up * 1f;
                    break;
            }

            // Movement Version 1 
            /*Agent.transform.Translate(dirToGo * Time.fixedDeltaTime * AgentMoveSpeed);
            Agent.transform.Rotate(rotateDir, Time.fixedDeltaTime * AgentRotationSpeed);*/

            // Movement Version 2
            Agent.Rigidbody.MovePosition(Agent.Rigidbody.position + (dirToGo * DummyEnvironmentController.AgentMoveSpeed * Time.fixedDeltaTime));
            Quaternion turnRotation = Quaternion.Euler(0.0f, rotateDir.y * Time.fixedDeltaTime * DummyEnvironmentController.AgentRotationSpeed, 0.0f);
            Agent.Rigidbody.MoveRotation(Agent.Rigidbody.rotation * turnRotation);
        }
    }
}
