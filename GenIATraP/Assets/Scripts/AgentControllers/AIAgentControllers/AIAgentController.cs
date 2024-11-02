using System;

namespace AgentControllers.AIAgentControllers
{
    [Serializable]
    public abstract class AIAgentController : AgentController
    {
        public AIAgentController()
        {
            ControllerType = ControllerType.AI;
        }
    }
}