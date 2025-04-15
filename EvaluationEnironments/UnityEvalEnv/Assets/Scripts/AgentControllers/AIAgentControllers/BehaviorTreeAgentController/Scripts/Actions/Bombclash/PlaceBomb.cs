
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.Bombclash
{
    public class PlaceBomb : ActionNode
    {

        public int placeBomb = 1;

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
    }
}