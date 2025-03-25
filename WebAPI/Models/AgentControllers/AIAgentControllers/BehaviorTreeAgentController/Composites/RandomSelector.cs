using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public class RandomSelector : CompositeNode
    {
        public RandomSelector(Guid guid, string name, List<WebAPI.Models.Property>? properties, Position? position)
            : base(guid, name, properties, position)
        {

        }
    }
}