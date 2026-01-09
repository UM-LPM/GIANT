using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public enum HealthLevel
    {
        Low,
        Medium,
        High,

    }

    public class HealthLevelBellow : Activator
    {
        public HealthLevel healthLevel;

        public HealthLevelBellow(string guid, List<WebAPI.Models.Property>? properties, Position? position)
            : base(guid, "HealthLevelBellow", properties, position)
        {

        }

        protected override void MapProperties(List<Property>? properties)
        {
            if (properties != null)
            {
                healthLevel = (HealthLevel)(properties.Find(p => p.Name == "healthLevel")?.Value ?? 0);
            }
        }
    }
}
