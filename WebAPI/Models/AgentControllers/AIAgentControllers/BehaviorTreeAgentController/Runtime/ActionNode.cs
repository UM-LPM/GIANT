
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public abstract class ActionNode : Node
    {
        protected ActionNode(Guid guid, string name, List<WebAPI.Models.Property>? properties, WebAPI.Models.Position? position) 
            : base(guid, name, properties, position)
        {
        }
    }
}