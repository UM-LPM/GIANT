
namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public class Shoot : Action
    {
        public int shoot = 1;

        public Shoot(Shoot other) : base(other)
        {
            this.shoot = other.shoot;
        }

        public override void Init()
        {
            return;
        }

        public override void Execute(ActionBuffer actionsOut)
        {
            actionsOut.AddDiscreteAction("shoot", shoot);
        }

        public override ADiSComponent Clone()
        {
            return new Shoot(this);
        }
    }
}
