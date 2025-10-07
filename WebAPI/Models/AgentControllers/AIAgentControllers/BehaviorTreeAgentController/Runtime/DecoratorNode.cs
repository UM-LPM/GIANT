
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public abstract class DecoratorNode : BTNode
    {
        public BTNode child;

        protected DecoratorNode(Guid guid, string name, List<WebAPI.Models.Property>? properties, Position? position) 
            : base(guid, name, properties, position)
        {
        }

        public override void AddChild(BTNode child)
        {
            this.child = child;
        }
    }
}