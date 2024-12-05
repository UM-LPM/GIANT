using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public enum HealthLevel
    {
        Low,
        Medium,
        High,

    }

    public class HealthLevelBellow : ConditionNode
    {
        public HealthLevel healthLevel;

        public HealthLevelBellow(Guid guid, string name, List<WebAPI.Models.Property>? properties, Position? position)
            : base(guid, name, properties, position)
        {

        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                healthLevel = (HealthLevel)(properties.Find(p => p.Name == "healthLevel")?.Value ?? 0);
            }
        }
    }
}