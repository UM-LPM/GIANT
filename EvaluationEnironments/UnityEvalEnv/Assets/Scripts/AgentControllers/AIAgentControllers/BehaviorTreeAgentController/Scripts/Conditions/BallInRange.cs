
using Problems.DodgeBall;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public class BallInRange : ConditionNode
    {
        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override bool CheckConditions()
        {
            context.gameObject.TryGetComponent(out DodgeBallAgentComponent agentComponent);
            if (agentComponent)
            {
                return agentComponent.DodgeBallEnvironmentController.BallInRange(agentComponent);
            }

            return false;
        }
    }
}