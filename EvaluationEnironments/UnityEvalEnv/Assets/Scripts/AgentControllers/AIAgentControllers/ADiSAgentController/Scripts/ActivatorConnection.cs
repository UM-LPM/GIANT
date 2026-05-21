using System;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    [Serializable]
    public class ActivatorConnection : Node
    {
        public Activator Activator;
        public bool IsNegated;

        public ActivatorConnection(Activator activator, bool isNegated)
        {
            this.Activator = activator;
            this.IsNegated = isNegated;
        }

        public ActivatorConnection(ActivatorConnection other) : base(other)
        {
            this.Activator = other.Activator;
            this.IsNegated = other.IsNegated;
        }
    }
}
