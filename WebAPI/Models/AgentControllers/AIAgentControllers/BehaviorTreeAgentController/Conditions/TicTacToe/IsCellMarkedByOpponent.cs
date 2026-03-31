using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.TicTacToe
{
    public class IsCellMarkedByOpponent : ConditionNode
    {
        public int gridPositionX;
        public int gridPositionY;
        public int gridPositionZ;

        public IsCellMarkedByOpponent(Guid guid, string name, List<WebAPI.Models.Property>? properties, Position? position)
            : base(guid, name, properties, position)
        {

        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                gridPositionX = properties.Find(p => p.Name == "gridPositionX")?.Value ?? 0;
                gridPositionY = properties.Find(p => p.Name == "gridPositionY")?.Value ?? 0;
                gridPositionZ = properties.Find(p => p.Name == "gridPositionZ")?.Value ?? 0;
            }
        }
    }
}
