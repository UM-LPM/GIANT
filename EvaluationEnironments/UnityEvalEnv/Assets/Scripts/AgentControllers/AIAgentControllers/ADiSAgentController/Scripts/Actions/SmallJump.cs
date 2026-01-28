
namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public class SmallJump : Action
    {
        public int smallJump = 1;

        public override void Init()
        {
            return;
        }

        public override void Execute(ActionBuffer actionsOut)
        {
            actionsOut.AddDiscreteAction("smallJump", smallJump);
        }
    }
}
