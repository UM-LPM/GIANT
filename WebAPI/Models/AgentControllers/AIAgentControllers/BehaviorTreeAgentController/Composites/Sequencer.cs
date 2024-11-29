using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public class Sequencer : CompositeNode
    {
        public Sequencer(Guid guid, string name, List<WebAPI.Models.Property>? properties, WebAPI.Models.Position? position)
            : base(guid, name, properties, position)
        {

        }
    }
}