using Utils;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public class MoveSide : Action
    {
        public MoveSideDirection moveSideDirection = MoveSideDirection.Random;
        private Util Util;

        public override void Init()
        {
            Util = context.gameObject.GetComponentInParent<Util>();
        }

        public override void Execute(ActionBuffer actionsOut)
        {
            actionsOut.AddDiscreteAction("moveSideDirection", moveSideDirection == MoveSideDirection.Random ? Util.NextIntAC(context.transform.GetInstanceID(), 3) : (int)moveSideDirection);
        }
    }

    public enum MoveSideDirection
    {
        Left = 1,
        Right = 2,
        NoAction = 0,
        Random = 3
    }
}
