using AgentControllers;
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController.TicTacToe;
using Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Problems.TicTacToe
{
    public class TicTacToeActionExecutor : ActionExecutor
    {
        private TicTacToeEnvironmentController TicTacToeEnvironmentController;

        private void Awake()
        {
            TicTacToeEnvironmentController = GetComponentInParent<TicTacToeEnvironmentController>();
        }

        public override void ExecuteActions(AgentComponent agent)
        {
            PlaceMarker(agent as TicTacToeAgentComponent);
        }

        private void PlaceMarker(TicTacToeAgentComponent agent)
        {
            if (agent.ActionBuffer.GetDiscreteAction("placeMarker") == 1)
            {
                int gridPositionX = agent.ActionBuffer.GetDiscreteAction("gridPositionX");
                int gridPositionY = agent.ActionBuffer.GetDiscreteAction("gridPositionY");
                int gridPositionZ = agent.ActionBuffer.GetDiscreteAction("gridPositionZ");

                TicTacToeEnvironmentController.Grid.PlaceMarker(
                    gridPositionX,
                    gridPositionY,
                    gridPositionZ,
                    TicTacToeEnvironmentController.GetAgentMarker(agent),
                    agent
                    );
            }
            else if (agent.ActionBuffer.GetDiscreteAction("placeRandomMarker") == 1)
            {
                TicTacToeEnvironmentController.Grid.PlaceRandomMarker(TicTacToeEnvironmentController.GetAgentMarker(agent), agent);
            }
        }
    }
}
