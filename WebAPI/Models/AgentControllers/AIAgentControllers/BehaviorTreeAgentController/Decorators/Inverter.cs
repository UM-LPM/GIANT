using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public class Inverter : DecoratorNode
    {
        public Inverter(Guid guid, string name, List<Property>? properties, Position? position)
            : base(guid, name, properties, position)
        {
        }

        protected override void MapProperties(List<Property>? properties)
        {
        }
    }
}