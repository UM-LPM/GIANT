using Base;
using Problems.TicTacToe;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.TicTacToe
{
    public class IsCellMarkedByOpponent: ConditionNode
    {
        public int gridPositionX;
        public int gridPositionY;
        public int gridPositionZ;

        private TicTacToeGrid grid;
        private TicTacToeAgentComponent ticTacToeAgentComponent;

        public IsCellMarkedByOpponent(IsCellMarkedByOpponent other) : base(other)
        {
            if (other == null)
                return;

            this.gridPositionX = other.gridPositionX;
            this.gridPositionY = other.gridPositionY;
            this.gridPositionZ = other.gridPositionZ;
        }

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

            return grid.IsCellOccupiedByOtherAgent(gridPositionX, gridPositionY, gridPositionZ, ticTacToeAgentComponent.IndividualID);
        }

        public override BTNode Clone()
        {
            return new IsCellMarkedByOpponent(this);
        }
    }
}