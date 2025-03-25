using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public class RootNode : Node
    {
        public Node child;

        public RootNode(Guid guid, string name, List<WebAPI.Models.Property>? properties, Position? position)
            : base(guid, name, properties, position)
        {
            child = null;
        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
            }
        }

        public override void AddChild(Node child)
        {
            this.child = child;
        }
    }
}