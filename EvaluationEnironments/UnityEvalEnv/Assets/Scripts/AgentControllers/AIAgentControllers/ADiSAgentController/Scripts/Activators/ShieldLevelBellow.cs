using Problems.Robostrike;

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

        public ShieldLevelBellow(ShieldLevelBellow other) : base(other)
        {
            this.shieldLevel = other.shieldLevel;
        }

        public override void Init()
        {
            return;
        }

        public override bool IsActivated()
        {
            ShieldComponent healthComponent = context.gameObject.GetComponent<ShieldComponent>();

            if (healthComponent != null)
            {
                if (healthComponent.Shield <= ShieldLevelToValue(shieldLevel))
                {
                    return true;
                }
            }

            return false;
        }

        public static int ShieldLevelToValue(ShieldLevel healthLevel)
        {
            switch (healthLevel)
            {
                case ShieldLevel.Low:
                    return 2;
                case ShieldLevel.Medium:
                    return 5;
                case ShieldLevel.High:
                    return 8;
                default:
                    return 0;
            }
        }
        public override ADiSComponent Clone()
        {
            return new ShieldLevelBellow(this);
        }
    }
}
