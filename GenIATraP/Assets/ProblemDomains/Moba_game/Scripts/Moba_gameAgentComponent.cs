using Base;
using UnityEngine;

namespace Problems.Moba_game
{
    public class Moba_gameAgentComponent : AgentComponent
    {
        public HullComponent Hull { get; set; }
        public TurretComponent Turret { get; set; }
        public GunComponent Gun { get; set; }

        public MissileSpawnPointComponent MissileSpawnPoint { get; set; }
        public MissileSpawnPointComponent MissileSpawnPoint1 { get; set; }
        public float NextShootTime { get; set; }

        public HealthComponent HealthComponent { get; set; }
        public EnergyComponent EnergyComponent { get; set; }
        public AmmoComponent AmmoComponent { get; set; }

        private AgentStatBars StatBars;
        private int LaserEnergyConsumption = 5;
        private int LaserHitEnergyBonus = 10;
        // Agent fitness variables
        public int SectorsExplored { get; set; }

        public int HealtPowerUpsCollected { get; set; }
        public int ShieldPowerUpsCollected { get; set; }
        public int AmmoPowerUpsCollected { get; set; }
        public int MissilesFired { get; set; }
        public int LasersFired { get; set; }
        public int MissilesHitOpponent { get; set; }
        public int MissilesHitTeammate { get; set; }
        public int MissilesHitBase { get; set; }
        public int MissilesHitOwnBase { get; set; }

        public int HitByOpponentMissiles { get; set; }

        public int MaxSurvivalTime { get; set; }
        public int CurrentSurvivalTime { get; set; }
        public int OpponentsDestroyed { get; set; }

        public int NumOfSpawns { get; set; }

        public int OpponentTrackCounter { get; set; }
        public bool AlreadyTrackingOpponent { get; set; }
        public string AgentType { get; set; }

        protected override void DefineAdditionalDataOnAwake()
        {
            Hull = GetComponentInChildren<HullComponent>();
            Turret = GetComponentInChildren<TurretComponent>();
            Gun = GetComponentInChildren<GunComponent>();

            MissileSpawnPointComponent[] missileSpawnPoints = GetComponentsInChildren<MissileSpawnPointComponent>();
            if (missileSpawnPoints.Length >= 2)
            {
                MissileSpawnPoint = missileSpawnPoints[0];  // First spawn point
                MissileSpawnPoint1 = missileSpawnPoints[1]; // Second spawn point
            }
            else
            {
                MissileSpawnPoint = GetComponentInChildren<MissileSpawnPointComponent>();

            }

            StatBars = GetComponent<AgentStatBars>();

            HealthComponent = GetComponent<HealthComponent>();
            EnergyComponent = GetComponent<EnergyComponent>();
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
                // TODO Add error reporting here
            }

            if (Turret == null)
            {
                throw new System.Exception("TurretComponent component is missing");
                // TODO Add error reporting here
            }

            if (Gun == null)
            {
                throw new System.Exception("GunComponent component is missing");
                // TODO Add error reporting here
            }

            if (StatBars == null)
            {
                throw new System.Exception("AgentStatBars component is missing");
                // TODO Add error reporting here
            }

            if (HealthComponent == null)
            {
                throw new System.Exception("HealthComponent component is missing");
                // TODO Add error reporting here
            }
            if (EnergyComponent == null)
            {
                throw new System.Exception("EnergyComponent component is missing");
                // TODO Add error reporting here
            }

            if (AmmoComponent == null)
            {
                throw new System.Exception("AmmoComponent component is missing");
                // TODO Add error reporting here
            }

            if (MissileSpawnPoint == null)
            {
                throw new System.Exception("MissileSpawnPointComponent component is missing");
                // TODO Add error reporting here
            }
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

        public void LaserFired()
        {
            if (EnergyComponent.Energy > 0)
            {
                EnergyComponent.Energy -= LaserEnergyConsumption;
                LasersFired++;
                UpdatetStatBars();
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
            HealthComponent.Health -= value;
            UpdatetStatBars();
        }

        public void UpdatetStatBars()
        {
            if (StatBars != null)
            {
                StatBars.SetStats(HealthComponent.Health, EnergyComponent.Energy);
            }
        }

        public void MissileHitOpponent()
        {
            MissilesHitOpponent++;
            EnergyComponent.Energy += LaserHitEnergyBonus;
        }

        public void MissileHitTeammate()
        {
            MissilesHitTeammate++;
        }

        public void MissileHitBase()
        {
            MissilesHitBase++;
            EnergyComponent.Energy += LaserHitEnergyBonus;
        }

        public void MissileHitOwnBase()
        {
            MissilesHitOwnBase++;
        }

        public void HitByOpponentMissile()
        {
            HitByOpponentMissiles++;
        }

        public void ResetSurvivalTime()
        {
            if (CurrentSurvivalTime > MaxSurvivalTime)
            {
                MaxSurvivalTime = CurrentSurvivalTime;
            }
            CurrentSurvivalTime = 0;
        }
    }
}