using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public enum AgentSideBasic
    {
        Center = 0,
        Left = 1,
        Right = 2
    }
    public enum AgentSideAdvanced
    {
        Center = 0,
        Left = 1,
        Right = 2,
        BackCenter = 3,
        BackLeft = 4,
        BackRight = 5
    }

    public enum TargetGameObject
    {
        Agent,
        Wall,
        Obstacle,
        Object1,
        Object2,
        Object3,
        Object4,
        Object5,
        Object6,
        Object7,
        Object8,
        Object9,
        Object10,
        Object11,
        Object12,
        Object13,
        Object14,
        Object15,
        Object16,
        Object17,
        Object18,
        Object19,
        Object20,
        Object21,
        Object22,
        Object23,
        Object24,
        Object25,
        Object26,
        Object27,
        Object28,
        Object29,
        Object30,
        Object31,
        Object32,
        Object33,
        Object34,
        Object35,
        Object36,
        Object37,
        Object38,
        Object39,
        Object40,
        Empty
    }

    public class RayHitObject : ConditionNode
    {
        public TargetGameObject targetGameObject;
        public AgentSideAdvanced side;
        public int rayIndex;

        public RayHitObject(Guid guid, string name, List<WebAPI.Models.Property>? properties, Position? position)
            : base(guid, name, properties, position)
        {

        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                targetGameObject = (TargetGameObject)(properties.Find(p => p.Name == "targetGameObject")?.Value ?? 0);
                side = (AgentSideAdvanced)(properties.Find(p => p.Name == "side")?.Value ?? 0);
                rayIndex = properties.Find(p => p.Name == "rayIndex")?.Value ?? 0;
            }
        }
    }
}