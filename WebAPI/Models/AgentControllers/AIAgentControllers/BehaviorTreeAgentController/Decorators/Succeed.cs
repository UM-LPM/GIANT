
using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public class Succeed : DecoratorNode
    {
        public Succeed(Guid guid, string name, List<Property>? properties, Position? position)
            : base(guid, name, properties, position)
        {
        }

        protected override void MapProperties(List<Property>? properties)
        {
        }
    }
}