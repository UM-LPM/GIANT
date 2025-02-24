using Base;
using UnityEngine;

namespace Problems.Moba_game
{
    public class Moba_gameBaseComponent : BaseComponent
    {
        public HealthComponent HealthComponent { get; set; }
        public MoneyComponent MoneyComponent { get; set; }
        public int LavaAmount;
        public int IceAmount;


        protected override void DefineAdditionalDataOnAwake()
        {
            HealthComponent = GetComponent<HealthComponent>();
            if (HealthComponent == null)
            {
                throw new System.Exception("HealthComponent component is missing");
                // TODO Add error reporting here
            }

            MoneyComponent = GetComponent<MoneyComponent>();
            if (MoneyComponent == null)
            {
                throw new System.Exception("MoneyComponent component is missing");
                // TODO Add error reporting here
            }
        }

        // public bool SetHealth(int value)
        // {
        //     if (HealthComponent.Health + value <= Moba_gameEnvironmentController.MAX_HEALTH)
        //     {
        //         HealthComponent.Health += value;
        //         return true;
        //     }
        //     else if (HealthComponent.Health < Moba_gameEnvironmentController.MAX_HEALTH && HealthComponent.Health + value > Moba_gameEnvironmentController.MAX_HEALTH)
        //     {
        //         HealthComponent.Health = Moba_gameEnvironmentController.MAX_HEALTH;
        //         return true;
        //     }
        //     else
        //     {
        //         return false;
        //     }
        // }

        public void TakeDamage(int value)
        {
            HealthComponent.Health -= value;
        }
    }
}