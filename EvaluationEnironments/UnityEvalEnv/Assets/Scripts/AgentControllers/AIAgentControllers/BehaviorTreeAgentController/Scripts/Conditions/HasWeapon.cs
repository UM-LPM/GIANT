
using Problems.Shooter;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public class HassWeapon : ConditionNode
    {
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
    }
}