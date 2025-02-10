using Base;
using UnityEngine;

namespace Problems.Moba_game
{
    public class Moba_gameGoldComponent : GoldComponent
    {
        public HealthComponent HealthComponent { get; set; }

        protected override void DefineAdditionalDataOnAwake()
        {
            HealthComponent = GetComponent<HealthComponent>();
            if (HealthComponent == null)
            {
                throw new System.Exception("HealthComponent component is missing");
                // TODO Add error reporting here
            }
        }

        public void TakeDamage(int value)
        {
            HealthComponent.Health -= value;
        }

    }
}