using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public enum ShieldLevel
    {
        Low,
        Medium,
        High
    }

    public class ShieldLevelBellow : ConditionNode
    {
        public ShieldLevel shieldLevel;

        public ShieldLevelBellow(Guid guid, string name, List<WebAPI.Models.Property>? properties, WebAPI.Models.Position? position)
            : base(guid, name, properties, position)
        {

        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                shieldLevel = (ShieldLevel)(properties.Find(p => p.Name == "shieldLevel")?.Value ?? 0);
            }
        }
    }
}