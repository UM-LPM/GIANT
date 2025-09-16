namespace AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController
{
    public class Inverter : DecoratorNode
    {
        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            return State.Success;
        }
    }
}