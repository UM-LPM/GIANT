
using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public class Encapsulator : DecoratorNode
    {
        public Encapsulator(Guid guid, string name, List<WebAPI.Models.Property>? properties, Position? position)
            : base(guid, name, properties, position)
        {

        }

        protected override void MapProperties(List<Property>? properties)
        {
        }
    }
}