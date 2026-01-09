using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public class Shoot : Action
    {
        public int shoot;

        public Shoot(string guid, List<Property>? properties, Position? position)
            : base(guid, "Shoot", properties, position)
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
