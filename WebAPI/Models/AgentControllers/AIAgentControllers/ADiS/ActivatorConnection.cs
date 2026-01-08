namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public class ActivatorConnection : Node
    {
        public Activator Activator;
        public bool IsNegated;

        public ActivatorConnection(Guid guid, Activator activator, bool isNegated) : base(guid.ToString(), "ActivatorConnection", null)
        {
            Activator = activator;
            IsNegated = isNegated;
        }
    }
}
