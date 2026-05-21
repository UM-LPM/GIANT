
using Problems.Shooter;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public class HasWeapon : ConditionNode
    {
        public HasWeapon(HasWeapon other) : base(other)
        {
        }
        
        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override bool CheckConditions()
        {
            context.gameObject.TryGetComponent(out ShooterAgentComponent agentComponent);
            if (agentComponent)
            {
                return agentComponent.HasWeapon();
            }

            return false;
        }

        public override BTNode Clone()
        {
            return new HasWeapon(this);
        }
    }
}