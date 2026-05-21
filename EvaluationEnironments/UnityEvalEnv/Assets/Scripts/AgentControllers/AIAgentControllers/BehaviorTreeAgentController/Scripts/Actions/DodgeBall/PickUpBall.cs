
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.DodgeBall
{
    public class PickUpBall : ActionNode
    {

        public int pickupBall = 1;

        public PickUpBall(PickUpBall other) : base(other)
        {
            if (other == null)
                return;

            this.pickupBall = other.pickupBall;
        }
        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            var discreteActionsOut = blackboard.actionsOut.DiscreteActions;
            blackboard.actionsOut.AddDiscreteAction("pickupBall", pickupBall);

            return State.Success;
        }

        public override BTNode Clone()
        {
            return new PickUpBall(this);
        }
    }
}
