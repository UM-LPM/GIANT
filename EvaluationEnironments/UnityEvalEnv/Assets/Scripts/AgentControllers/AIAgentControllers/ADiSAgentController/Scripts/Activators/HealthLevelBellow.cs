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

        public override void Init()
        {
            return;
        }

        public override bool IsActivated()
        {
            HealthComponent healthComponent = context.gameObject.GetComponent<HealthComponent>();

            if (healthComponent != null)
            {
                if (healthComponent.Health <= HealthLevelToValue(healthLevel))
                {
                    return true;
                }
            }

            return false;
        }

        public static int HealthLevelToValue(HealthLevel healthLevel)
        {
            switch (healthLevel)
            {
                case HealthLevel.Low:
                    return 2;
                case HealthLevel.Medium:
                    return 5;
                case HealthLevel.High:
                    return 8;
                default:
                    return 0;
            }
        }

    }
}
