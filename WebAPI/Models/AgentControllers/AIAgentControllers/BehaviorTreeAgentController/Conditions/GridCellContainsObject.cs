using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public class GridCellContainsObject : ConditionNode
    {
        public TargetGameObject targetGameObject;
        public int gridPositionX;
        public int gridPositionY;

        public GridCellContainsObject(Guid guid, string name, List<WebAPI.Models.Property>? properties, WebAPI.Models.Position? position)
            : base(guid, name, properties, position)
        {

        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                targetGameObject = (TargetGameObject)(properties.Find(p => p.Name == "targetGameObject")?.Value ?? 0);
                gridPositionX = properties.Find(p => p.Name == "gridPositionX")?.Value ?? 0;
                gridPositionY = properties.Find(p => p.Name == "gridPositionY")?.Value ?? 0;
            }
        }
    }
}