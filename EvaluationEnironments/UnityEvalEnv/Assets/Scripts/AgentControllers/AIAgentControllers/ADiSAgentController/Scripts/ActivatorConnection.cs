using System;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    [Serializable]
    public class ActivatorConnection : Node
    {
        public Activator Activator;
        public bool IsNegated;
    }
}
