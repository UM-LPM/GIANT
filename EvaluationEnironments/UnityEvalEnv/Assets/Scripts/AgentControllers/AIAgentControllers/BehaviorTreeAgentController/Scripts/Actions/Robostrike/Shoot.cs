
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.Robostrike
{
    public class Shoot : ActionNode
    {

        public int shoot = 1;

        public Shoot(Shoot other) : base(other)
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
            blackboard.actionsOut.AddDiscreteAction("shoot", shoot);

            return State.Success;
        }

        public override BTNode Clone()
        {
            return new Shoot(this);
        }
    }
}
