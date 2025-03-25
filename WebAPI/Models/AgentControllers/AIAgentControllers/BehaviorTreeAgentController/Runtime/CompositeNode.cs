
using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public abstract class CompositeNode : Node
    {
        public List<Node> children = new List<Node>();

        protected CompositeNode(Guid guid, string name, List<Property>? properties, Position position) 
            : base(guid, name, properties, position)
        {
        }

        protected override void MapProperties(List<Property>? properties)
        {
        }

        public override void AddChild(Node child)
        {
            children.Add(child);
        }
    }
}