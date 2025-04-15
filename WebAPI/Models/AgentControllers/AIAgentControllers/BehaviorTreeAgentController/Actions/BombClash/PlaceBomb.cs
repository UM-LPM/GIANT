using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.Bombclash
{
    public class PlaceBomb : ActionNode
    {
        public int placeBomb;

        public PlaceBomb(Guid guid, string name, List<Property>? properties, Position position)
            : base(guid, name, properties, position)
        {
        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                placeBomb = properties.Find(p => p.Name == "PlaceBomb")?.Value ?? 1;
            }
        }
    }
}