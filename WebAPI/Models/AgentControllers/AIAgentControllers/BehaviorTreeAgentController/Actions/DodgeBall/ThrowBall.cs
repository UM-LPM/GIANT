using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.DodgeBall
{
    public class ThrowBall : ActionNode
    {

        public int throwBall;

        public ThrowBall(Guid guid, string name, List<Property>? properties, Position position)
            : base(guid, name, properties, position)
        {
        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                throwBall = properties.Find(p => p.Name == "throwBall")?.Value ?? 1;
            }
        }
    }
}
