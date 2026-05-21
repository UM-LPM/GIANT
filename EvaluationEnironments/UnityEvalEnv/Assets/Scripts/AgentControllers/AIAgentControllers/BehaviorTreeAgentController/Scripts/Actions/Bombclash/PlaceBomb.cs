
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.BombClash
{
    public class PlaceBomb : ActionNode
    {

        public int placeBomb = 1;

        public PlaceBomb(PlaceBomb other) : base(other)
        {
            if (other == null)
                return;

            this.placeBomb = other.placeBomb;
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
            blackboard.actionsOut.AddDiscreteAction("placeBomb", placeBomb);

            return State.Success;
        }

        public override BTNode Clone()
        {
            return new PlaceBomb(this);
        }
    }
}