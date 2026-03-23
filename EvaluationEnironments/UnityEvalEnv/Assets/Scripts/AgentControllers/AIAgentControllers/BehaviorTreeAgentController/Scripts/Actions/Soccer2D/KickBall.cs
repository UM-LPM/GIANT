
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.Soccer2D
{
    public class KickBall : ActionNode
    {

        public int shoot = 1;
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
    }
}
