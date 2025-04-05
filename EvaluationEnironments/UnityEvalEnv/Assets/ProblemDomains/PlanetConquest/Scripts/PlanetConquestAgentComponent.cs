using Base;
using Problems.Robostrike;
using UnityEngine;

namespace Problems.PlanetConquest
{
    public class PlanetConquestAgentComponent : AgentComponent
    {
        [SerializeField]  public AgentType AgentType;
        public HullComponent Hull { get; set; }
        public TurretComponent Turret { get; set; }
        public GunComponent Gun { get; set; }

        public MissileSpawnPointComponent MissileSpawnPoint { get; set; }
        public MissileSpawnPointComponent MissileSpawnPoint1 { get; set; }
        public float NextShootTime { get; set; }

        public HealthComponent HealthComponent { get; set; }
        public EnergyComponent EnergyComponent { get; set; }

        public LineRenderer LineRenderer { get; set; }

        private AgentStatBars StatBars;

        // Agent fitness variables
        public int SectorsExplored { get; set; }
        public int LasersFired { get; set; }
        public int MissilesHitOpponent { get; set; }
        public int MissilesHitTeammate { get; set; }
        public int MissilesHitBase { get; set; }
        public int MissilesHitOwnBase { get; set; }
        public int HitByOpponentMissiles { get; set; }
        public int EnteredLavaPlanetOrbit { get; set; }
        public int EnteredIcePlanetOrbit { get; set; }
        public int CapturedLavaPlanet { get; set; }
        public int CapturedIcePlanet { get; set; }
        public int MaxSurvivalTime { get; set; }
        public int CurrentSurvivalTime { get; set; }
        public int OpponentsDestroyed { get; set; }
        public int OpponentBasesDestroyed { get; set; }

        public int NumOfSpawns { get; set; }

        public int OpponentTrackCounter { get; set; }
        public bool AlreadyTrackingOpponent { get; set; }


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

            LineRenderer = GetComponent<LineRenderer>();

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

            if (MissileSpawnPoint == null)
            {
                throw new System.Exception("MissileSpawnPointComponent component is missing");
                // TODO Add error reporting here
            }

            if (LineRenderer == null)
            {
                UnityEngine.Debug.LogWarning("LineRenderer component is missing");
            }
        }

        public bool SetHealth(int value)
        {
            if (HealthComponent.Health + value <= PlanetConquestEnvironmentController.MAX_HEALTH)
            {
                HealthComponent.Health += value;
                return true;
            }
            else if (HealthComponent.Health < PlanetConquestEnvironmentController.MAX_HEALTH && HealthComponent.Health + value > PlanetConquestEnvironmentController.MAX_HEALTH)
            {
                HealthComponent.Health = PlanetConquestEnvironmentController.MAX_HEALTH;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void LaserFired(PlanetConquestEnvironmentController planetConquestEnvironmentController)
        {
            if (EnergyComponent.Energy > 0)
            {
                LasersFired++;
                if (planetConquestEnvironmentController.UnlimitedEnergy == false)
                {
                    EnergyComponent.Energy -= planetConquestEnvironmentController.LaserEnergyConsumption;
                    UpdatetStatBars();
                }
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

        public void MissileHitOpponent(PlanetConquestEnvironmentController planetConquestEnvironmentController)
        {
            MissilesHitOpponent++;
            EnergyComponent.Energy += planetConquestEnvironmentController.LaserHitEnergyBonus;
        }

        public void MissileHitTeammate()
        {
            MissilesHitTeammate++;
        }

        public void MissileHitBase(PlanetConquestEnvironmentController planetConquestEnvironmentController)
        {
            MissilesHitBase++;
            EnergyComponent.Energy += planetConquestEnvironmentController.LaserHitEnergyBonus;
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

    public enum AgentType
    {
        Lava,
        Ice
    }
}