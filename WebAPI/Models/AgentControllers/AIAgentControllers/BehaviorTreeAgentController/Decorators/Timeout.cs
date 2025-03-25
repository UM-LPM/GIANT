using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public class Timeout : DecoratorNode
    {
        public float duration = 1.0f;

        public Timeout(Guid guid, string name, List<WebAPI.Models.Property>? properties, Position? position)
            : base(guid, name, properties, position)
        {

        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                duration = properties.Find(p => p.Name == "duration")?.Value ?? 1.0f;
            }
        }
    }
}