using Base;
using UnityEngine;

namespace Problems.Robostrike
{
    public class RobostrikeAgentComponent : AgentComponent
    {
        public HullComponent Hull{ get; set; }
        public TurretComponent Turret { get; set; }

        public MissileSpawnPointComponent MissileSpawnPoint { get; set; }
        public float NextShootTime { get; set; }

        public HealthComponent HealthComponent { get; set; }
        public ShieldComponent ShieldComponent { get; set; }
        public AmmoComponent AmmoComponent { get; set; }

        private AgentStatBars StatBars;

        // Agent fitness variables
        public int SectorsExplored { get; set; }

        public int HealtPowerUpsCollected { get; set; }
        public int ShieldPowerUpsCollected { get; set; }
        public int AmmoPowerUpsCollected { get; set; }

        public int MissilesFired { get; set; }
        public int MissilesHitOpponent { get; set; }

        public int HitByOpponentMissiles { get; set; }

        public int MaxSurvivalTime { get; set; }
        public int CurrentSurvivalTime { get; set; }
        public int OpponentsDestroyed { get; set; }

        public int NumOfSpawns { get; set; }

        public int OpponentTrackCounter { get; set; }
        public bool AlreadyTrackingOpponent { get; set; }

        protected override void DefineAdditionalDataOnAwake()
        {
            Hull = GetComponentInChildren<HullComponent>();
            Turret = GetComponentInChildren<TurretComponent>();

            MissileSpawnPoint = GetComponentInChildren<MissileSpawnPointComponent>();
            StatBars = GetComponent<AgentStatBars>();

            HealthComponent = GetComponent<HealthComponent>();
            ShieldComponent = GetComponent<ShieldComponent>();
            AmmoComponent = GetComponent<AmmoComponent>();

            CheckComponentValidity();
        }

        protected override void DefineAdditionalDataOnStart()
        {
            UpdatetStatBars();
        }

        void CheckComponentValidity()
        {
            if (Hull == null)
            {
                throw new System.Exception("HullComponent component is missing");
            }

            if (Turret == null)
            {
                throw new System.Exception("TurretComponent component is missing");
            }

            if (StatBars == null)
            {
                throw new System.Exception("AgentStatBars component is missing");
            }

            if (HealthComponent == null)
            {
                throw new System.Exception("HealthComponent component is missing");
            }

            if (ShieldComponent == null)
            {
                throw new System.Exception("ShieldComponent component is missing");
            }

            if (AmmoComponent == null)
            {
                throw new System.Exception("AmmoComponent component is missing");
            }

            if (MissileSpawnPoint == null)
            {
                throw new System.Exception("MissileSpawnPointComponent component is missing");
            }
        }

        public bool SetHealth(int value)
        {
            if (HealthComponent.Health + value <= RobostrikeEnvironmentController.MAX_HEALTH)
            {
                HealthComponent.Health += value;
                return true;
            }
            else if (HealthComponent.Health < RobostrikeEnvironmentController.MAX_HEALTH && HealthComponent.Health + value > RobostrikeEnvironmentController.MAX_HEALTH)
            {
                HealthComponent.Health = RobostrikeEnvironmentController.MAX_HEALTH;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SetShield(int value)
        {
            if (ShieldComponent.Shield + value <= RobostrikeEnvironmentController.MAX_SHIELD)
            {
                ShieldComponent.Shield += value;
                return true;
            }
            else if (ShieldComponent.Shield < RobostrikeEnvironmentController.MAX_SHIELD && ShieldComponent.Shield + value > RobostrikeEnvironmentController.MAX_SHIELD)
            {
                ShieldComponent.Shield = RobostrikeEnvironmentController.MAX_SHIELD;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SetAmmo(int value)
        {
            if (AmmoComponent.Ammo + value <= RobostrikeEnvironmentController.MAX_AMMO)
            {
                AmmoComponent.Ammo += value;
                return true;
            }
            else if (AmmoComponent.Ammo < RobostrikeEnvironmentController.MAX_AMMO && AmmoComponent.Ammo + value > RobostrikeEnvironmentController.MAX_AMMO)
            {
                AmmoComponent.Ammo = RobostrikeEnvironmentController.MAX_AMMO;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void MissileFired()
        {
            if (AmmoComponent.Ammo > 0)
            {
                AmmoComponent.Ammo--;
                MissilesFired++;
                UpdatetStatBars();
            }
        }

        public void TakeDamage(int value)
        {
            if (ShieldComponent.Shield > 0)
            {
                if (ShieldComponent.Shield - (value * 2) < 0)
                {
                    ShieldComponent.Shield = 0;
                }
                else
                {
                    ShieldComponent.Shield -= value;
                }
                HealthComponent.Health -= value * 0.5f;
            }
            else
            {
                HealthComponent.Health -= value;
            }

            // Update Stat Bars 
            UpdatetStatBars();
        }

        public void UpdatetStatBars()
        {
            if (StatBars != null)
                StatBars.SetStats(HealthComponent.Health, ShieldComponent.Shield, AmmoComponent.Ammo);
        }

        public void SetEnvironmentColor(Color color)
        {
            if(StatBars != null)
                StatBars.SetEnvironmentColor(color);
        }

        public void MissileHitOpponent()
        {
            MissilesHitOpponent++;
        }

        public void HitByOpponentMissile()
        {
            HitByOpponentMissiles++;
        }

        public void ResetSurvivalTime()
        {
            if(CurrentSurvivalTime > MaxSurvivalTime)
            {
                MaxSurvivalTime = CurrentSurvivalTime;
            }
            CurrentSurvivalTime = 0;
        }

        public void SetTeamColor(Color teamColor)
        {
            Turret.SetTurretColor(teamColor);
        }
    }
}