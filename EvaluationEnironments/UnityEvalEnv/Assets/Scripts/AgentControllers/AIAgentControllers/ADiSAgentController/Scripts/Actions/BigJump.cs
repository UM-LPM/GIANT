
namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public class BigJump : Action
    {
        public int bigJump = 1;

        public override void Init()
        {
            return;
        }

        public override void Execute(ActionBuffer actionsOut)
        {
            actionsOut.AddDiscreteAction("bigJump", bigJump);
        }
    }
}
