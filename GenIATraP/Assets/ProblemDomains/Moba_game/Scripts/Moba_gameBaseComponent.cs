using Base;
using UnityEngine;

namespace Problems.Moba_game
{
    public class Moba_gameBaseComponent : BaseComponent
    {
        public HealthComponent HealthComponent { get; set; }
        public int HitByOpponentMissiles { get; set; }
        private BaseStatBars StatBars;

        protected override void DefineAdditionalDataOnAwake()
        {
            HealthComponent = GetComponent<HealthComponent>();
            StatBars = GetComponent<BaseStatBars>();
            if (HealthComponent == null)
            {
                throw new System.Exception("HealthComponent component is missing");
                // TODO Add error reporting here
            }
            if (StatBars == null)
            {
                throw new System.Exception("AgentStatBars component is missing");
                // TODO Add error reporting here
            }
        }
        protected override void DefineAdditionalDataOnStart()
        {
            UpdatetStatBars();
        }

        public bool SetHealth(int value)
        {
            if (HealthComponent.Health + value <= Moba_gameEnvironmentController.MAX_HEALTH)
            {
                HealthComponent.Health += value;
                return true;
            }
            else if (HealthComponent.Health < Moba_gameEnvironmentController.MAX_HEALTH && HealthComponent.Health + value > Moba_gameEnvironmentController.MAX_HEALTH)
            {
                HealthComponent.Health = Moba_gameEnvironmentController.MAX_HEALTH;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void TakeDamage(int value)
        {
            HealthComponent.Health -= value;

            UpdatetStatBars();
        }

        public void HitByOpponentMissile()
        {
            HitByOpponentMissiles++;
        }

        public void UpdatetStatBars()
        {
            if (StatBars != null)
            {
                StatBars.SetStats(HealthComponent.Health);
            }
        }
    }
}