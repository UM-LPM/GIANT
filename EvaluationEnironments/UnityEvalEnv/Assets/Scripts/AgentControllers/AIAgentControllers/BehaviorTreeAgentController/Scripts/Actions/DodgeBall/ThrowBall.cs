
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.DodgeBall
{
    public class ThrowBall : ActionNode
    {

        public int throwBall = 1;

        public ThrowBall(ThrowBall other) : base(other)
        {
            if (other == null)
                return;

            this.throwBall = other.throwBall;
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
            blackboard.actionsOut.AddDiscreteAction("throwBall", throwBall);

            return State.Success;
        }

        public override BTNode Clone()
        {
            return new ThrowBall(this);
        }
    }
}
