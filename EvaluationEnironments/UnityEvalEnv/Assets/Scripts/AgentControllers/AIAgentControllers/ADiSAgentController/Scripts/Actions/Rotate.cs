using Utils;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public class Rotate : Action
    {
        public RotateDirection rotateDirection = RotateDirection.Random;
        private Util Util;

        public Rotate(Rotate other) : base(other)
        {
            this.rotateDirection = other.rotateDirection;
        }

        public override void Init()
        {
            Util = context.gameObject.GetComponentInParent<Util>();
        }

        public override void Execute(ActionBuffer actionsOut)
        {
            actionsOut.AddDiscreteAction("rotateDirection", rotateDirection == RotateDirection.Random ? Util.NextIntAC(context.transform.GetInstanceID(), 3) : (int)rotateDirection);
        }

        public override ADiSComponent Clone()
        {
            return new Rotate(this);
        }
    }

    public enum RotateDirection
    {
        Left = 1,
        Right = 2,
        NoAction = 0,
        Random = 3
    }
}