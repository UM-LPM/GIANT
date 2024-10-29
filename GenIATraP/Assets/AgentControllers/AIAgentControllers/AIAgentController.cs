namespace AgentControllers.AIAgentControllers
{
    public abstract class AIAgentController : AgentController
    {
        public AIAgentController()
        {
            ControllerType = ControllerType.AI;
        }
    }
}