namespace AgentControllers
{
    public abstract class ManualAgentController : AgentController
    {
        public ManualAgentController()
        {
            ControllerType = ControllerType.Manual;
        }

        public ManualAgentController(ManualAgentController other) : base(other)
        {
            ControllerType = other.ControllerType;
        }
    }
}