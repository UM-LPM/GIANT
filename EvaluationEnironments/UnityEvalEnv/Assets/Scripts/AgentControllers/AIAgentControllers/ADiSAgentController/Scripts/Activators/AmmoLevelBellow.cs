using System.Runtime.InteropServices.WindowsRuntime;

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

        public AmmoLevelBellow(AmmoLevelBellow other) : base(other)
        {
            this.ammoLevel = other.ammoLevel;
        }

        public override void Init()
        {
            return;
        }

        public override bool IsActivated()
        {
            AmmoComponent healthComponent = context.gameObject.GetComponent<AmmoComponent>();

            if (healthComponent != null)
            {
                if (healthComponent.Ammo <= AmmoLevelToValue(ammoLevel))
                {
                    return true;
                }
            }

            return false;
        }

        public static int AmmoLevelToValue(AmmoLevel healthLevel)
        {
            switch (healthLevel)
            {
                case AmmoLevel.Low:
                    return 2;
                case AmmoLevel.Medium:
                    return 5;
                case AmmoLevel.High:
                    return 8;
                default:
                    return 0;
            }
        }

        public override ADiSComponent Clone()
        {
            return new AmmoLevelBellow(this);
        }

    }
}
