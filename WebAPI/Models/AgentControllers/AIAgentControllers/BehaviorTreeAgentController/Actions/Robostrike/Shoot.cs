using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.Robostrike
{
    public class Shoot : ActionNode
    {
        public int shoot;

        public Shoot(Guid guid, string name, List<Property>? properties, Position position)
            : base(guid, name, properties, position)
        {
        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                shoot = properties.Find(p => p.Name == "shoot")?.Value ?? 1;
            }
        }
    }
}