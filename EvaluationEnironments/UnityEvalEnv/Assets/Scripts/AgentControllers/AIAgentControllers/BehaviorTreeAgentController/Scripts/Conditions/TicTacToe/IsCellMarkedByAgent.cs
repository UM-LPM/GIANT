using Base;
using Problems.TicTacToe;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.TicTacToe
{
    public class IsCellMarkedByAgent: ConditionNode
    {
        public int gridPositionX;
        public int gridPositionY;
        public int gridPositionZ;

        private TicTacToeGrid grid;
        private TicTacToeAgentComponent ticTacToeAgentComponent;

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        public bool Check()
        {
            return CheckConditions();
        }

        protected override bool CheckConditions()
        {
            if(grid == null)
                 grid = context.gameObject.GetComponentInParent<TicTacToeGrid>();

            if(ticTacToeAgentComponent == null)
                ticTacToeAgentComponent = context.gameObject.GetComponent<TicTacToeAgentComponent>();

            return grid.IsCellOccupiedByAgent(gridPositionX, gridPositionY, gridPositionZ, ticTacToeAgentComponent.IndividualID);
        }
    }
}