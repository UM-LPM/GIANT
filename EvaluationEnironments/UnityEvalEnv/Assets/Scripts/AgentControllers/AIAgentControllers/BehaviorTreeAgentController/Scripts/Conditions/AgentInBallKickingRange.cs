
using Problems.Soccer2D;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public class AgentInBallKickingRange : ConditionNode
    {
        public AgentInBallKickingRange(AgentInBallKickingRange other) : base(other)
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
            context.gameObject.TryGetComponent(out Soccer2DAgentComponent agentComponent);
            if (agentComponent)
            {
                return agentComponent.Soccer2DEnvironmentController.IsAgentInBallKickingRange(agentComponent);
            }

            return false;
        }

        public override BTNode Clone()
        {
            return new AgentInBallKickingRange(this);
        }
    }
}