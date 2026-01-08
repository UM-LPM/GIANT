using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public enum MoveSideDirection
    {
        Left = 1,
        Right = 2,
        NoAction = 0,
        Random = 3
    }

    public class MoveSide: Action
    {
        public MoveSideDirection moveSideDirection = MoveSideDirection.Left;

        public MoveSide(string guid, List<Property>? properties, Position? position) : base(guid, "MoveSide", properties, position)
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
