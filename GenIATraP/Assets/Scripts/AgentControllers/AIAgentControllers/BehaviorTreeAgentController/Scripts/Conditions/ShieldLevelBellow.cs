
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public enum ShieldLevel
    {
        Low,
        Medium,
        High
    }

    public class ShieldLevelBellow : ConditionNode
    {
        public ShieldLevel shieldLevel;
        // TODO: Support for raw value instead of enum ??? (public int healthLevel;)

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }


        protected override bool CheckConditions()
        {
            // TODO Implement
            /*ShieldComponent shieldComponent = context.gameObject.GetComponent<ShieldComponent>();

            if (shieldComponent != null)
            {
                if (shieldComponent.Shield <= ShieldLevelToValue(shieldLevel))
                {
                    return true;
                }
            }*/

            return false;
        }
        public static int ShieldLevelToValue(ShieldLevel ShieldLevel)
        {
            switch (ShieldLevel)
            {
                case ShieldLevel.Low:
                    return 4;
                case ShieldLevel.Medium:
                    return 6;
                case ShieldLevel.High:
                    return 8;
                default:
                    return 0;
            }
        }

    }
}