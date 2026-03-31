using Base;
using Problems.TicTacToe;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.TicTacToe
{
    public class IsCellEmpty: ConditionNode
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

            return grid.IsCellOccupied(gridPositionX, gridPositionY, gridPositionZ);
        }
    }
}