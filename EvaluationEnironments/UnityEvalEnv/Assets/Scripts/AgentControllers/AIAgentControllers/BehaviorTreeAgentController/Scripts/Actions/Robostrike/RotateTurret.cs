using Utils;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.Robostrike
{
    public class RotateTurret : ActionNode
    {

        public RotateDirection rotateDirection = RotateDirection.Random;

        private Util Util;

        public RotateTurret(RotateTurret other) : base(other)
        {
            if (other == null)
                return;

            this.rotateDirection = other.rotateDirection;
        }

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
            blackboard.actionsOut.AddDiscreteAction("rotateTurretDirection", rotateDirection == RotateDirection.Random ? Util.NextIntAC(this.context.transform.GetInstanceID(), 3) : (int)rotateDirection);

            return State.Success;
        }

        public override BTNode Clone()
        {
            return new RotateTurret(this);
        }
    }
}