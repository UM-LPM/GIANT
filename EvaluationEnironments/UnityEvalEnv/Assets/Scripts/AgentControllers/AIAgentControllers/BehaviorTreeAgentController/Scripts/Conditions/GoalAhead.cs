using Problems.Soccer2D;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public class GoalAhead : ConditionNode
    {
        public enum GoalType
        {
            Team,
            Oponent
        }

        public GoalType goalType;

        private Soccer2DGoalComponent goalComponent;

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
                goalComponent = agentComponent.Soccer2DEnvironmentController.TeamGoalAhead(agentComponent);

                if (goalComponent)
                {
                    if (goalComponent.Team == agentComponent.Team && goalType == GoalType.Team
                        || goalComponent.Team != agentComponent.Team && goalType == GoalType.Oponent)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
    }
}