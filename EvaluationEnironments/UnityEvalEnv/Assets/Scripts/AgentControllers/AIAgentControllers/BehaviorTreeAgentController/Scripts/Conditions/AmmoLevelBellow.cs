
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public enum AmmoLevel
    {
        Low,
        Medium,
        High
    }

    public class AmmoLevelBellow : ConditionNode
    {
        public AmmoLevel ammoLevel;
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
            /*AmmoComponent ammoComponent = context.gameObject.GetComponent<AmmoComponent>();

            if (ammoComponent != null)
            {
                if (ammoComponent.Ammo <= AmmoLevelToValue(ammoLevel))
                {
                    return true;
                }
            }*/

            return false;
        }

        public static int AmmoLevelToValue(AmmoLevel AmmoLevel)
        {
            switch (AmmoLevel)
            {
                case AmmoLevel.Low:
                    return 4;
                case AmmoLevel.Medium:
                    return 10;
                case AmmoLevel.High:
                    return 16;
            }

            return 0;
        }

    }
}