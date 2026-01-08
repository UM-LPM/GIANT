using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public enum RotateDirection
    {
        Left = 1,
        Right = 2,
        NoAction = 0,
        Random = 3
    }

    public class Rotate : Action
    {
        public RotateDirection rotateDirection = RotateDirection.Left;

        public Rotate(string guid, List<Property>? properties, Position? position) : base(guid, "Rotate", properties, position)
        {
        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                rotateDirection = (RotateDirection)(properties.Find(p => p.Name == "rotateDirection")?.Value ?? 0);
            }
        }
    }
}