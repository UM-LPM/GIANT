using Utils;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public enum RotateDirection
    {
        Left = 1,
        Right = 2,
        NoAction = 0,
        Random = 3
    }

    public class Rotate : ActionNode
    {

        public RotateDirection rotateDirection = RotateDirection.Random;

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
            blackboard.actionsOut.AddDiscreteAction("rotateDirection", rotateDirection == RotateDirection.Random ? Util.NextIntAC(3) : (int)rotateDirection);

            return State.Success;
        }
    }
}