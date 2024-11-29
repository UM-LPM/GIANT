using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public enum MoveForwardDirection
    {
        Forward = 1,
        Backward = 2,
        NoAction = 0,
        Random = 3
    }

    public class MoveForward : ActionNode
    {
        public MoveForwardDirection moveForwardDirection = MoveForwardDirection.Random;

        public MoveForward(Guid guid, string name, List<WebAPI.Models.Property>? properties, WebAPI.Models.Position? position)
            : base(guid, name, properties, position)
        {

        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                moveForwardDirection = (MoveForwardDirection)(properties.Find(p => p.Name == "moveForwardDirection")?.Value ?? 0);
            }
        }
    }
}