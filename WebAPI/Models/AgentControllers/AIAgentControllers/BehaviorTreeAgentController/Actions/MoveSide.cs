using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public enum MoveSideDirection
    {
        Left = 1,
        Right = 2,
        NoAction = 0,
        Random = 3
    }

    public class MoveSide : ActionNode
    {
        public MoveSideDirection moveSideDirection = MoveSideDirection.Random;

        public MoveSide(Guid guid, string name, List<WebAPI.Models.Property>? properties, Position? position)
            : base(guid, name, properties, position)
        {

        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                moveSideDirection = (MoveSideDirection)(properties.Find(p => p.Name == "moveSideDirection")?.Value ?? 0);
            }
        }
    }
}