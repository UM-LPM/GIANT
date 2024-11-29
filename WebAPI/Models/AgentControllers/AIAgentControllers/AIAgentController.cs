namespace AgentControllers.AIAgentControllers
{
    [Serializable]
    public abstract class AIAgentController : AgentController
    {
        public AIAgentController(string name, ControllerType controllerType = ControllerType.AI) : base(name, controllerType)
        {
        }
    }
}