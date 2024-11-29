using Utils;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public enum MoveSideDirection
    {
        Left = 1,
        Right = 2,
        NoAction = 0,
        Random = 3
    }

    public class MoveSide : ActionNode
    {

        public MoveSideDirection moveSideDirection = MoveSideDirection.Random;

        private Util Util;
        protected override void OnStart()
        {
            Util = context.gameObject.GetComponentInParent<Util>();
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            var discreteActionsOut = blackboard.actionsOut.DiscreteActions;
            blackboard.actionsOut.AddDiscreteAction("moveSideDirection", moveSideDirection == MoveSideDirection.Random ? Util.NextIntAC(3) : (int)moveSideDirection);

            return State.Success;
        }
    }
}
