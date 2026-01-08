using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public abstract class Activator : ADiSComponent
    {
        public Activator(string guid, string name, List<Property>? properties, Position? position) : base(guid, name, properties, position)
        {
        }

        public static Activator GetActivator(Guid guid, string name, List<Property>? properties, Position? position) =>
            name switch
            {
                "RayHitObject" => new RayHitObject(guid.ToString(), properties, position),
                _ => throw new ArgumentException($"Activator '{name}' is not recognized."),
            };
    }
}
