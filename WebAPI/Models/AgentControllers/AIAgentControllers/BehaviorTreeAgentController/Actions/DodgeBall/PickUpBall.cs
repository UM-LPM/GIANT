using AgentControllers.AIAgentControllers.BehaviorTreeAgentController.BombClash;
using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.DodgeBall
{
    public class PickUpBall : ActionNode
    {
        public int pickupBall;

        public PickUpBall(Guid guid, string name, List<Property>? properties, Position position)
            : base(guid, name, properties, position)
        {
        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                pickupBall = properties.Find(p => p.Name == "pickupBall")?.Value ?? 1;
            }
        }
    }
}
