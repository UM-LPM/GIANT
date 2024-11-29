

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public abstract class ConditionNode : Node
    {
        protected ConditionNode(Guid guid, string name, List<WebAPI.Models.Property>? properties, WebAPI.Models.Position? position) 
            : base(guid, name, properties, position)
        {
        }
    }
}
