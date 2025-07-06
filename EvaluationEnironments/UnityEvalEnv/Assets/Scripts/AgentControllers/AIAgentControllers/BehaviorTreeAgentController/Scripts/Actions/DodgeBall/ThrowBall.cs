
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.DodgeBall
{
    public class ThrowBall : ActionNode
    {

        public int throwBall = 1;
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
    }
}
