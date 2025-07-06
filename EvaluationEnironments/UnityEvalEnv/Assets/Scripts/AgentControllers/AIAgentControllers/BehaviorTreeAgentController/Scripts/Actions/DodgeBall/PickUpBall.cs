
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.DodgeBall
{
    public class PickUpBall : ActionNode
    {

        public int pickupBall = 1;
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
    }
}
