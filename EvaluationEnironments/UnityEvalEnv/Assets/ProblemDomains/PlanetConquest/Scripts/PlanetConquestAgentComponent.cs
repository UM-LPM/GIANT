using Base;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.PlanetConquest
{
    public class PlanetConquestAgentComponent : AgentComponent
    {
        [SerializeField]  public AgentType AgentType;
        public LaserSpawnPointComponent LaserSpawnPoint { get; set; }
        public float NextShootTime { get; set; }

        public HealthComponent HealthComponent { get; set; }
        public EnergyComponent EnergyComponent { get; set; }

        public LineRenderer LineRenderer { get; set; }

        private AgentStatBars StatBars;

        // Fitness variables
        public int SectorsExplored { get; set; }
        public int LasersFired { get; set; }
        public int MissilesHitOpponent { get; set; }
        public int MissilesHitTeammate { get; set; }
        public int MissilesHitBase { get; set; }
        public int MissilesHitOwnBase { get; set; }
        public int HitByOpponentLasers { get; set; }
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
            LaserSpawnPoint = GetComponentInChildren<LaserSpawnPointComponent>();

            StatBars = GetComponent<AgentStatBars>();

            HealthComponent = GetComponent<HealthComponent>();
            EnergyComponent = GetComponent<EnergyComponent>();

            LineRenderer = GetComponentInChildren<LineRenderer>();

            CheckComponentValidity();
        }

        protected override void DefineAdditionalDataOnStart()
        {
            UpdatetStatBars();
        }

        void CheckComponentValidity()
        {
            if (LaserSpawnPoint == null)
            {
                throw new System.Exception("LaserSpawnPoint component is missing");
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

            if (LineRenderer == null)
            {
                UnityEngine.Debug.LogWarning("LineRenderer component is missing");
            }
        }

        public bool SetHealth(int value)
        {
            if (HealthComponent.Health + value <= PlanetConquestEnvironmentController.MAX_AGENT_HEALTH)
            {
                HealthComponent.Health += value;
                return true;
            }
            else if (HealthComponent.Health < PlanetConquestEnvironmentController.MAX_AGENT_HEALTH && HealthComponent.Health + value > PlanetConquestEnvironmentController.MAX_AGENT_HEALTH)
            {
                HealthComponent.Health = PlanetConquestEnvironmentController.MAX_AGENT_HEALTH;
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

        public void LaserHitOpponent(PlanetConquestEnvironmentController planetConquestEnvironmentController)
        {
            MissilesHitOpponent++;
            EnergyComponent.Energy += planetConquestEnvironmentController.LaserHitEnergyBonus;
        }

        public void LaserHitTeammate()
        {
            MissilesHitTeammate++;
        }

        public void MissileHitBase(PlanetConquestEnvironmentController planetConquestEnvironmentController)
        {
            MissilesHitBase++;
            EnergyComponent.Energy += planetConquestEnvironmentController.LaserHitEnergyBonus;
        }

        public void LaserHitOwnBase()
        {
            MissilesHitOwnBase++;
        }

        public void HitByOpponentLaser()
        {
            HitByOpponentLasers++;
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