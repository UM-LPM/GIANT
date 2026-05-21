
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.Soccer2D
{
    public class KickBall : ActionNode
    {

        public int shoot = 1;

        public KickBall(KickBall other) : base(other)
        {
            if (other == null)
                return;

            this.shoot = other.shoot;
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
            blackboard.actionsOut.AddDiscreteAction("kick", shoot);

            return State.Success;
        }

        public override BTNode Clone()
        {
            return new KickBall(this);
        }
    }
}
