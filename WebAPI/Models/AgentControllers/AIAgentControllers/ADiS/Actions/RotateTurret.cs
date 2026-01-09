using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public enum RotateTurretDirection
    {
        Left = 1,
        Right = 2,
        NoAction = 0,
        Random = 3
    }

    public class RotateTurret : Action
    {
        public RotateTurretDirection rotateDirection = RotateTurretDirection.Left;

        public RotateTurret(string guid, List<Property>? properties, Position? position) : base(guid, "RotateTurret", properties, position)
        {
        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                rotateDirection = (RotateTurretDirection)(properties.Find(p => p.Name == "rotateTurretDirection")?.Value ?? 0);
            }
        }
    }
}