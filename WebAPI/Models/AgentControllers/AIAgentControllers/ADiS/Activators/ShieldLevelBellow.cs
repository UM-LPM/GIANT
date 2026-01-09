using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public enum ShieldLevel
    {
        Low,
        Medium,
        High,

    }

    public class ShieldLevelBellow : Activator
    {
        public ShieldLevel shieldLevel;

        public ShieldLevelBellow(string guid, List<WebAPI.Models.Property>? properties, Position? position)
            : base(guid, "ShieldLevelBellow", properties, position)
        {

        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                shieldLevel = (ShieldLevel)(properties.Find(p => p.Name == "shieldLevel")?.Value ?? 0);
            }
        }
    }
}
