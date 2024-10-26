namespace AgentControllers
{
    public abstract class ManualAgentController : AgentController
    {
        public ManualAgentController()
        {
            ControllerType = ControllerType.Manual;
        }
    }
}