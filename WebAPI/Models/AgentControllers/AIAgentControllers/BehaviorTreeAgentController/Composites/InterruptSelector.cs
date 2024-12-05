namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public class InterruptSelector : Selector
    {
        public InterruptSelector(Guid guid, string name, List<WebAPI.Models.Property>? properties, Position? position)
            : base(guid, name, properties, position)
        {

        }
    }
}