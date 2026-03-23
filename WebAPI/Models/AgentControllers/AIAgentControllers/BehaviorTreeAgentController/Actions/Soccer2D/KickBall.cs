using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.Soccer2D
{
    public class KickBall : ActionNode
    {
        public int kick;

        public KickBall(Guid guid, string name, List<Property>? properties, Position position)
            : base(guid, name, properties, position)
        {
        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                kick = properties.Find(p => p.Name == "kick")?.Value ?? 1;
            }
        }
    }
}