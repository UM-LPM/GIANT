
namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public class Shoot : Action
    {
        public int shoot = 1;

        public override void Init()
        {
            return;
        }

        public override void Execute(ActionBuffer actionsOut)
        {
            actionsOut.AddDiscreteAction("shootMissile", shoot);
        }
    }
}
