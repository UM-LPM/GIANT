using Base;
using Problems.PlanetConquest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.PlanetConquest2
{
    public class PlanetConquest2AgentComponent : AgentComponent
    {
        [SerializeField] public AgentType AgentType;
        public Rigidbody2D Rigidbody2D { get; set; }
        public LaserSpawnPointComponent LaserSpawnPoint { get; set; }
        public float NextShootTime { get; set; }

        public HealthComponent HealthComponent { get; set; }
        public EnergyComponent EnergyComponent { get; set; }

        public LineRenderer LineRenderer { get; set; }

        private AgentStatBars StatBars;

        // Fitness variables
        public int SectorsExplored { get; set; }
        public int LasersFired { get; set; }
        public int NumOfSpawns { get; set; }
        public int EnteredLavaPlanetOrbit { get; set; }
        public int EnteredIcePlanetOrbit { get; set; }
        public int CapturedLavaPlanet { get; set; }
        public int CapturedIcePlanet { get; set; }
        public int OpponentBasesDestroyed { get; set; }
        public int CurrentSurvivalTime { get; set; }
        public int MaxSurvivalTime { get; set; }
        public int LaserHitOpponents { get; set; }
        public int LaserHitTeammates { get; set; }
        public int LaserHitBases { get; set; }
        public int LaserHitOwnBases { get; set; }
        public int HitByOpponentLasers { get; set; }
        public int OpponentsDestroyed { get; set; }

        protected override void DefineAdditionalDataOnAwake()
        {
            Rigidbody2D = GetComponent<Rigidbody2D>();
            LaserSpawnPoint = GetComponentInChildren<LaserSpawnPointComponent>();
            HealthComponent = GetComponent<HealthComponent>();
            EnergyComponent = GetComponent<EnergyComponent>();
            LineRenderer = GetComponent<LineRenderer>();
            StatBars = GetComponentInChildren<AgentStatBars>();
            CheckComponentValidity();
        }

        protected override void DefineAdditionalDataOnStart()
        {
            UpdatetStatBars();
        }

        public void UpdatetStatBars()
        {
            if (StatBars != null)
            {
                StatBars.SetStats(HealthComponent.Health, EnergyComponent.Energy);
            }
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
            if (HealthComponent.Health + value <= PlanetConquest2EnvironmentController.MAX_AGENT_HEALTH)
            {
                HealthComponent.Health += value;
                return true;
            }
            else if (HealthComponent.Health < PlanetConquest2EnvironmentController.MAX_AGENT_HEALTH && HealthComponent.Health + value > PlanetConquest2EnvironmentController.MAX_AGENT_HEALTH)
            {
                HealthComponent.Health = PlanetConquest2EnvironmentController.MAX_AGENT_HEALTH;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void LaserFired(PlanetConquest2EnvironmentController PlanetConquest2EnvironmentController)
        {
            if (EnergyComponent.Energy > 0)
            {
                LasersFired++;
                if (PlanetConquest2EnvironmentController.UnlimitedEnergy == false)
                {
                    EnergyComponent.Energy -= PlanetConquest2EnvironmentController.LaserEnergyConsumption;
                    UpdatetStatBars();
                }
            }
        }

        public void TakeDamage(int value)
        {
            HealthComponent.Health -= value;
            UpdatetStatBars();
        }

        public void LaserHitOpponent(PlanetConquest2EnvironmentController PlanetConquest2EnvironmentController)
        {
            LaserHitOpponents++;
            EnergyComponent.Energy += PlanetConquest2EnvironmentController.LaserHitEnergyBonus;
        }

        public void LaserHitTeammate()
        {
            LaserHitTeammates++;
        }

        public void LaserHitBase(PlanetConquest2EnvironmentController PlanetConquest2EnvironmentController)
        {
            LaserHitBases++;
            EnergyComponent.Energy += PlanetConquest2EnvironmentController.LaserHitEnergyBonus;
        }

        public void LaserHitOwnBase()
        {
            LaserHitOwnBases++;
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
