using AgentControllers;
using Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.BoxTact
{
    public class BoxTactActionExecutor : ActionExecutor
    {
        private BoxTactEnvironmentController BoxTactEnvironmentController;

        // Move Agent variables
        int upAxis = 0;
        int sideAxis = 0;

        private void Awake()
        {
            BoxTactEnvironmentController = GetComponentInParent<BoxTactEnvironmentController>();
        }

        public override void ExecuteActions(AgentComponent agent)
        {
            MoveAgent(agent as BoxTactAgentComponent);
        }

        private void MoveAgent(BoxTactAgentComponent agent)
        {
            upAxis = agent.ActionBuffer.GetDiscreteAction("moveUpDirection");
            sideAxis = agent.ActionBuffer.GetDiscreteAction("moveSideDirection");

            if (upAxis == 1)
            {
                agent.SetDirection(Vector2.up);
            }
            else if (upAxis == 2)
            {
                agent.SetDirection(Vector2.down);
            }
            else if (sideAxis == 1)
            {
                agent.SetDirection(Vector2.left);
            }
            else if (sideAxis == 2)
            {
                agent.SetDirection(Vector2.right);
            }
            else
            {
                agent.SetDirection(Vector2.zero);
            }

            if (agent.NextAgentUpdateTime <= BoxTactEnvironmentController.CurrentSimulationTime && agent.MoveDirection != Vector2.zero)
            {
                if (BoxTactEnvironmentController.AgentCanMove(agent))
                {
                    agent.transform.Translate(new Vector3(agent.MoveDirection.x, agent.MoveDirection.y, 0));
                    agent.NextAgentUpdateTime = BoxTactEnvironmentController.CurrentSimulationTime + BoxTactEnvironmentController.AgentUpdateinterval;
                    agent.CheckIfNewSectorExplored();
                }
                else
                {
                    agent.SetDirection(Vector2.zero);
                }
            }
        }
    }
}
