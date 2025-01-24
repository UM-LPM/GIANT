using AgentControllers;
using Base;
using UnityEngine;
using Utils;

namespace Problems.Soccer
{
    public class SoccerActionExecutor : ActionExecutor
    {
        private SoccerEnvironmentController SoccerEnvironmentController;

        private void Awake()
        {
            SoccerEnvironmentController = GetComponentInParent<SoccerEnvironmentController>();
        }

        public override void ExecuteActions(AgentComponent agent)
        {
            MoveAgent(agent as SoccerAgentComponent);
        }

        void MoveAgent(SoccerAgentComponent agent)
        {
            var dirToGo = Vector3.zero;
            var rotateDir = Vector3.zero;

            agent.KickPower = 0f;

            var forwardAxis = agent.ActionBuffer.GetDiscreteAction("moveForwardDirection");
            var rightAxis = agent.ActionBuffer.GetDiscreteAction("moveSideDirection");
            var rotateAxis = agent.ActionBuffer.GetDiscreteAction("rotateDirection");

            switch (forwardAxis)
            {
                case 1:
                    dirToGo = agent.transform.forward * SoccerEnvironmentController.ForwardSpeed;
                    agent.KickPower = 1f;
                    break;
                case 2:
                    dirToGo = agent.transform.forward * -SoccerEnvironmentController.ForwardSpeed;
                    break;
            }

            switch (rightAxis)
            {
                case 1:
                    dirToGo = agent.transform.right * -SoccerEnvironmentController.LateralSpeed;
                    break;
                case 2:
                    dirToGo = agent.transform.right * SoccerEnvironmentController.LateralSpeed;
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

            Quaternion turnRotation = Quaternion.Euler(0.0f, rotateDir.y * Time.fixedDeltaTime * SoccerEnvironmentController.AgentRotationSpeed, 0.0f);
            agent.Rigidbody.MoveRotation(agent.Rigidbody.rotation * turnRotation);
            agent.Rigidbody.AddForce(dirToGo * SoccerEnvironmentController.AgentRunSpeed, ForceMode.VelocityChange);
        }

    }
}
