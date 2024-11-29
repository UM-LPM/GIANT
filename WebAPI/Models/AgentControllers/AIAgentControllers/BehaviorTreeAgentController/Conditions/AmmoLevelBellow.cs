using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public enum AmmoLevel
    {
        Low,
        Medium,
        High
    }

    public class AmmoLevelBellow : ConditionNode
    {
        public AmmoLevel ammoLevel;

        public AmmoLevelBellow(Guid guid, string name, List<WebAPI.Models.Property>? properties, WebAPI.Models.Position? position)
            : base(guid, name, properties, position)
        {

        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                ammoLevel = (AmmoLevel)(properties.Find(p => p.Name == "ammoLevel")?.Value ?? 0);
            }
        }
    }
}