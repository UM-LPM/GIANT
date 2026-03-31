using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using Problems.TicTacToe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.TicTacToe
{
    public class PlaceMarker : ActionNode
    {
        public int placeMarker = 1;
        public int gridPositionX = -1;
        public int gridPositionY = -1;
        public int gridPositionZ = -1;

        private TicTacToeGrid grid;
        private TicTacToeEnvironmentController ticTacToeEnvironmentController;
        private TicTacToeAgentComponent ticTacToeAgentComponent;

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            if (grid == null)
                grid = context.gameObject.GetComponentInParent<TicTacToeGrid>();

            if (ticTacToeAgentComponent == null)
                ticTacToeAgentComponent = context.gameObject.GetComponent<TicTacToeAgentComponent>();

            if (ticTacToeEnvironmentController == null)
                ticTacToeEnvironmentController = context.gameObject.GetComponentInParent<TicTacToeEnvironmentController>();

            if (grid.PlaceMarker(
                gridPositionX,
                gridPositionY,
                gridPositionZ,
                ticTacToeEnvironmentController.GetAgentMarker(ticTacToeAgentComponent),
                ticTacToeAgentComponent
                ))
                return State.Success;

            return State.Failure;
            /*var discreteActionsOut = blackboard.actionsOut.DiscreteActions;

            blackboard.actionsOut.AddDiscreteAction("placeMarker", placeMarker);
            blackboard.actionsOut.AddDiscreteAction("gridPositionX", gridPositionX);
            blackboard.actionsOut.AddDiscreteAction("gridPositionY", gridPositionY);
            blackboard.actionsOut.AddDiscreteAction("gridPositionZ", gridPositionZ);

            return State.Success;*/
        }
    }
}
