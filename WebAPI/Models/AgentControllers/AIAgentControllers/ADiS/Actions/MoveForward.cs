using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public enum MoveForwardDirection
    {
        Forward = 1,
        Backward = 2,
        NoAction = 0,
        Random = 3
    }

    public class MoveForward : Action
    {
        public MoveForwardDirection moveForwardDirection = MoveForwardDirection.Random;

        public MoveForward(string guid, List<Property>? properties, Position? position)
            : base(guid, "MoveForward", properties, position)
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
