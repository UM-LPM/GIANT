using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public enum AmmoLevel
    {
        Low,
        Medium,
        High,

    }

    public class AmmoLevelBellow : Activator
    {
        public AmmoLevel ammoLevel;

        public AmmoLevelBellow(string guid, List<WebAPI.Models.Property>? properties, Position? position)
            : base(guid, "AmmoLevelBellow", properties, position)
        {

        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                ammoLevel = (AmmoLevel)(properties.Find(p => p.Name == "ammoLevel")?.Value ?? 0);
            }
        }
    }
}
