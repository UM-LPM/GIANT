using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using Problems.TicTacToe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.TicTacToe
{
    public class PlaceRandomMarker : ActionNode
    {
        public int placeRandomMarker = 1;

        private TicTacToeGrid grid;
        private TicTacToeEnvironmentController ticTacToeEnvironmentController;
        private TicTacToeAgentComponent ticTacToeAgentComponent;

        public PlaceRandomMarker(PlaceRandomMarker other) : base(other)
        {
            if (other == null)
                return;

            this.placeRandomMarker = other.placeRandomMarker;
        }

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

            if (grid.PlaceRandomMarker(
                ticTacToeEnvironmentController.GetAgentMarker(ticTacToeAgentComponent),
                ticTacToeAgentComponent
                ))
                return State.Success;

            return State.Failure;

            //var discreteActionsOut = blackboard.actionsOut.DiscreteActions;

            //blackboard.actionsOut.AddDiscreteAction("placeRandomMarker", placeRandomMarker);

            //return State.Success;
        }

        public override BTNode Clone()
        {
            return new PlaceRandomMarker(this);
        }
    }
}
