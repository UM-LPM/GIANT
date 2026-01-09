using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public abstract class Action: ADiSComponent
    {
        public Action(string guid, string name, List<Property>? properties, Position? position) : base(guid, name, properties, position)
        {
        }

        public static Action GetAction(Guid guid, string name, List<Property>? properties, Position? position) =>
            name switch
            {
                "MoveForward" => new MoveForward(guid.ToString(), properties, position),
                "MoveSide" => new MoveSide(guid.ToString(), properties, position),
                "Rotate" => new Rotate(guid.ToString(), properties, position),
                "Shoot" => new Shoot(guid.ToString(), properties, position),
                null => throw new ArgumentException("Action name cannot be null."),
                _ => throw new ArgumentException($"Action '{name}' is not recognized."),
            };
    }
}
