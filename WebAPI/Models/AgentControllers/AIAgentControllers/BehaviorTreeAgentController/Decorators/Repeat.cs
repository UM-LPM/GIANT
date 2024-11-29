
using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public class Repeat : DecoratorNode
    {
        public bool restartOnSuccess = true;
        public bool restartOnFailure = false;

        public Repeat(Guid guid, string name, List<WebAPI.Models.Property>? properties, WebAPI.Models.Position? position)
            : base(guid, name, properties, position)
        {

        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                restartOnSuccess = (properties.Find(p => p.Name == "restartOnSuccess")?.Value ?? 1) == 1;
                restartOnFailure = (properties.Find(p => p.Name == "restartOnFailure")?.Value ?? 1) == 1;
            }
        }
    }
}