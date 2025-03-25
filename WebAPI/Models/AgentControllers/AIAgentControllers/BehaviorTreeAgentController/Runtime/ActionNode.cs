
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public abstract class ActionNode : Node
    {
        protected ActionNode(Guid guid, string name, List<WebAPI.Models.Property>? properties, Position? position) 
            : base(guid, name, properties, position)
        {
        }

        public override void AddChild(Node child)
        {
            return;
        }
    }
}