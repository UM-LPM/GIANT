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

        public ShieldLevelBellow(ShieldLevelBellow other) : base(other)
        {
            if (other == null)
                return;

            this.shieldLevel = other.shieldLevel;
        }

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }


        protected override bool CheckConditions()
        {
            ShieldComponent shieldComponent = context.gameObject.GetComponent<ShieldComponent>();

            if (shieldComponent != null)
            {
                if (shieldComponent.Shield <= ShieldLevelToValue(shieldLevel))
                {
                    return true;
                }
            }

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

        public override BTNode Clone()
        {
            return new ShieldLevelBellow(this);
        }
    }
}