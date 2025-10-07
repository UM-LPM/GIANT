
using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public abstract class CompositeNode : BTNode
    {
        public List<BTNode> children = new List<BTNode>();

        protected CompositeNode(Guid guid, string name, List<Property>? properties, Position position) 
            : base(guid, name, properties, position)
        {
        }

        protected override void MapProperties(List<Property>? properties)
        {
        }

        public override void AddChild(BTNode child)
        {
            children.Add(child);
        }
    }
}