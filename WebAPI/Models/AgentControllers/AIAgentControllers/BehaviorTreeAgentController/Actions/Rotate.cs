using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public enum RotateDirection
    {
        Left = 1,
        Right = 2,
        NoAction = 0,
        Random = 3
    }

    public class Rotate : ActionNode
    {
        public RotateDirection rotateDirection = RotateDirection.Random;

        public Rotate(Guid guid, string name, List<WebAPI.Models.Property>? properties, WebAPI.Models.Position? position)
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