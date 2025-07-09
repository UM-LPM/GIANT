
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public enum HealthLevel
    {
        Low,
        Medium,
        High,

    }

    public class HealthLevelBellow : ConditionNode
    {
        public HealthLevel healthLevel;

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }


        protected override bool CheckConditions()
        {
            HealthComponent healthComponent = context.gameObject.GetComponent<HealthComponent>();

            if (healthComponent != null)
            {
                if(healthComponent.Health <= HealthLevelToValue(healthLevel))
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