using AgentControllers;
using UnityEngine;

namespace Problems.Robostrike
{
    public class RobostrikeActionExecutor : ActionExecutor
    {
        private RobostrikeEnvironmentController RobostrikeEnvironmentController;
        private RobostrikeAgentComponent Agent;

        private void Awake()
        {
            Agent = GetComponent<RobostrikeAgentComponent>();
            RobostrikeEnvironmentController = GetComponentInParent<RobostrikeEnvironmentController>();
        }

        public override void ExecuteActions(ActionBuffer actionBuffer)
        {
            MoveAgent(actionBuffer);
        }

        private void MoveAgent(ActionBuffer actionBuffer)
        {
            // TODO Implement
            throw new System.NotImplementedException();
        }
    }
}
