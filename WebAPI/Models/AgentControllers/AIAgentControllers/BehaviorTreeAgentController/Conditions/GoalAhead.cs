using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public enum GoalType
    {
        Team,
        Oponent
    }

    public class GoalAhead : ConditionNode
    {
        public GoalType goalType;

        public GoalAhead(Guid guid, string name, List<WebAPI.Models.Property>? properties, Position? position)
            : base(guid, name, properties, position)
        {

        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                goalType = (GoalType)(properties.Find(p => p.Name == "goalType")?.Value ?? 0);
            }
        }
    }
}