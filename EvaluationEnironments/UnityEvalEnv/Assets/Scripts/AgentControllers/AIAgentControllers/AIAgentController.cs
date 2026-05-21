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

        public AIAgentController(AIAgentController other) : base(other)
        {
            ControllerType = other.ControllerType;
        }
    }
}