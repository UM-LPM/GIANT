
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public abstract class DecoratorNode : Node
    {
        public Node child;

        protected DecoratorNode(Guid guid, string name, List<WebAPI.Models.Property>? properties, WebAPI.Models.Position? position) 
            : base(guid, name, properties, position)
        {
        }
    }
}