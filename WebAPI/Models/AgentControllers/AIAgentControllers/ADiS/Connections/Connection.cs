namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public class Connection : ADiSComponent
    {
        public List<ActivatorConnection> ActivatorConnections = new List<ActivatorConnection>();
        public List<Action> Actions = new List<Action>();
        public double Weight;

        public Connection(Guid guid, double weight, Position? position) : base(guid.ToString(), "Connection", null, position)
        {
            Weight = weight;
        }

        protected override void MapProperties(List<WebAPI.Models.Property>? properties)
        {
            return; // No properties to map
        }
    }
}
