using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.TicTacToe
{
    public class PlaceRandomMarker: ActionNode
    {
        public int placeRandomMarker;

        public PlaceRandomMarker(Guid guid, string name, List<Property>? properties, Position position)
            : base(guid, name, properties, position)
        {
        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                placeRandomMarker = properties.Find(p => p.Name == "placeRandomMarker")?.Value ?? 1;
            }
        }
    }
}
