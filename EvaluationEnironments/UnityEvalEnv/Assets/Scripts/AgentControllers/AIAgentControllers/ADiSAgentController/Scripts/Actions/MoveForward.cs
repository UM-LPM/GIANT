using Utils;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public class MoveForward : Action
    {
        public MoveForwardDirection moveForwardDirection = MoveForwardDirection.Random;
        private Util Util;

        public MoveForward(MoveForward other) : base(other)
        {
            this.moveForwardDirection = other.moveForwardDirection;
        }

        public override void Init()
        {
            Util = context.gameObject.GetComponentInParent<Util>();
        }

        public override void Execute(ActionBuffer actionsOut)
        {
            actionsOut.AddDiscreteAction("moveForwardDirection", moveForwardDirection == MoveForwardDirection.Random ? Util.NextIntAC(context.transform.GetInstanceID(), 3) : (int)moveForwardDirection);
        }

        public override ADiSComponent Clone()
        {
            return new MoveForward(this);
        }
    }

    public enum MoveForwardDirection
    {
        Forward = 1,
        Backward = 2,
        NoAction = 0,
        Random = 3
    }
}
