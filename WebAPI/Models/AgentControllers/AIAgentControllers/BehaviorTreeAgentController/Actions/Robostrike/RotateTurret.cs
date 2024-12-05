using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.Robostrike
{
    public class RotateTurret : ActionNode
    {
        public RotateDirection rotateDirection = RotateDirection.Random;

        public RotateTurret(Guid guid, string name, List<WebAPI.Models.Property>? properties, Position? position)
            : base(guid, name, properties, position)
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