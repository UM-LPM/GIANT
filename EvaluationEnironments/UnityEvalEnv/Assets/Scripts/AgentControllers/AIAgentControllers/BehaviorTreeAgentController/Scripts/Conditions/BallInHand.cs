
using Problems.DodgeBall;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public class BallInHand : ConditionNode
    {
        public BallInHand(BallInHand other) : base(other)
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
            context.gameObject.TryGetComponent(out DodgeBallAgentComponent agentComponent);
            if (agentComponent)
            {
                // Check if the agent has the ball in hand
                return agentComponent.BallInHand;
            }

            return false;
        }
        override public BTNode Clone()
        {
            return new BallInHand(this);
        }
    }
}