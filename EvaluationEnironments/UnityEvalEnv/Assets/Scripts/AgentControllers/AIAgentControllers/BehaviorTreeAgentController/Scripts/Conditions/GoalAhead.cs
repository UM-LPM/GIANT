using Problems.Soccer;

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

        private GoalComponent goalComponent;

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override bool CheckConditions()
        {
            context.gameObject.TryGetComponent(out SoccerAgentComponent agentComponent);
            if (agentComponent)
            {
                goalComponent = agentComponent.SoccerEnvironmentController.TeamGoalAhead(agentComponent);

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