using Problems.Dummy;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Problems.Robostrike
{
    public class RobostrikeEnvironmentController : EnvironmentControllerBase
    {
        [SerializeField] public static int MAX_HEALTH = 10;
        [SerializeField] public static int MAX_SHIELD = 10;
        [SerializeField] public static int MAX_AMMO = 20;

        [Header("Robostrike General Configuration")]
        [SerializeField] int AgentStartHealth = 10;
        [SerializeField] int AgentStartShield = 0;
        [SerializeField] int AgentStartAmmo = 0;
        [SerializeField] RobostrikeAgentRespawnType AgentRespawnType = RobostrikeAgentRespawnType.StartPos;
        [SerializeField] RobostrikeGameScenarioType GameScenarioType = RobostrikeGameScenarioType.Normal;

        [Header("Robostrike Movement Configuration")]
        [SerializeField] public float AgentMoveSpeed = 5f;
        [SerializeField] public float AgentRotationSpeed = 80f;
        [SerializeField] public float AgentTurrentRotationSpeed = 90f;
        [HideInInspector] public float ForwardSpeed = 1f;

        [Header("Robostrike Missile Configuration")]
        [SerializeField] GameObject MissilePrefab;
        [SerializeField, Tooltip("Destroy Missile After X seconds")] public float DestroyMissileAfter = 3.0f;
        [SerializeField] float MissileShootCooldown = 1.0f;
        [SerializeField] float MissleLaunchSpeed = 30f;
        [SerializeField] public static int MissileDamage = 2;

        [Header("Robostrike PowerUps Configuration")]
        [SerializeField] int HealthPowerUpValue = 5;
        [SerializeField] int ShieldPowerUpValue = 5;
        [SerializeField] int AmmoPowerUpValue = 10;

        [Header("Robostrike PowerUps Prefabs")]
        [SerializeField] public float MinPowerUpDistance = 8f;
        [SerializeField] public Vector3 PowerUpColliderExtendsMultiplier = new Vector3(0.505f, 0.495f, 0.505f);
        [SerializeField] public GameObject HealthBoxPrefab;
        [SerializeField] public int HealthBoxSpawnAmount = 2;
        [SerializeField] public GameObject ShieldBoxPrefab;
        [SerializeField] public int ShieldBoxSpawnAmount = 2;
        [SerializeField] public GameObject AmmoBoxPrefab;
        [SerializeField] public int AmmoBoxSpawnAmount = 2;

        private RobostrikePowerUpSpawner PowerUpSpawner;
        private List<PowerUpComponent> PowerUps;

        // Sectors
        SectorComponent[] sectors;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();

            PowerUpSpawner = GetComponent<RobostrikePowerUpSpawner>();
            if (PowerUpSpawner == null)
            {
                throw new Exception("RobostrikePowerUpSpawner is not defined");
                // TODO Add error reporting here
            }
        }

        protected override void DefineAdditionalDataOnPostStart()
        {
            // Set Agent stats
            foreach (RobostrikeAgentComponent agent in Agents)
            {
                agent.HealthComponent.Health = AgentStartHealth;
                agent.ShieldComponent.Shield = AgentStartShield;
                agent.AmmoComponent.Ammo = AgentStartAmmo;
            }

            // Spawn powerUps
            PowerUps = PowerUpSpawner.Spawn<PowerUpComponent>(this);
        }

        protected override void OnPostFixedUpdate()
        {
            CheckAgentsPickedPowerUps();
            CheckAgentsExploration();
        }

        public void ReadParamsFromMainConfiguration()
        {
            if (MenuManager.Instance != null && MenuManager.Instance.MainConfiguration != null)
            {
                MainConfiguration conf = MenuManager.Instance.MainConfiguration;

                RobostrikeFitness.FitnessValues = conf.FitnessValues;

                if (conf.ProblemConfiguration.ContainsKey("ArenaSizeX"))
                {
                    ArenaSize.x = float.Parse(conf.ProblemConfiguration["ArenaSizeX"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("ArenaSizeZ"))
                {
                    ArenaSize.z = float.Parse(conf.ProblemConfiguration["ArenaSizeZ"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("ArenaOffset"))
                {
                    ArenaOffset = float.Parse(conf.ProblemConfiguration["ArenaOffset"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentMoveSpeed"))
                {
                    AgentMoveSpeed = float.Parse(conf.ProblemConfiguration["AgentMoveSpeed"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentRotationSpeed"))
                {
                    AgentRotationSpeed = float.Parse(conf.ProblemConfiguration["AgentRotationSpeed"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentTurrentRotationSpeed"))
                {
                    AgentTurrentRotationSpeed = float.Parse(conf.ProblemConfiguration["AgentTurrentRotationSpeed"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("DestroyMissileAfter"))
                {
                    DestroyMissileAfter = float.Parse(conf.ProblemConfiguration["DestroyMissileAfter"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("MissileShootCooldown"))
                {
                    MissileShootCooldown = float.Parse(conf.ProblemConfiguration["MissileShootCooldown"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("MissleLaunchSpeed"))
                {
                    MissleLaunchSpeed = float.Parse(conf.ProblemConfiguration["MissleLaunchSpeed"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("MissileDamage"))
                {
                    MissileDamage = int.Parse(conf.ProblemConfiguration["MissileDamage"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentStartHealth"))
                {
                    AgentStartHealth = int.Parse(conf.ProblemConfiguration["AgentStartHealth"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentStartShield"))
                {
                    AgentStartShield = int.Parse(conf.ProblemConfiguration["AgentStartShield"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentStartAmmo"))
                {
                    AgentStartAmmo = int.Parse(conf.ProblemConfiguration["AgentStartAmmo"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("GameScenarioType"))
                {
                    GameScenarioType = (RobostrikeGameScenarioType)int.Parse(conf.ProblemConfiguration["GameScenarioType"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentRespawnType"))
                {
                    AgentRespawnType = (RobostrikeAgentRespawnType)int.Parse(conf.ProblemConfiguration["AgentRespawnType"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("HealthPowerUpValue"))
                {
                    HealthPowerUpValue = int.Parse(conf.ProblemConfiguration["HealthPowerUpValue"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("ShieldPowerUpValue"))
                {
                    ShieldPowerUpValue = int.Parse(conf.ProblemConfiguration["ShieldPowerUpValue"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AmmoPowerUpValue"))
                {
                    AmmoPowerUpValue = int.Parse(conf.ProblemConfiguration["AmmoPowerUpValue"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("MaxHealth"))
                {
                    MAX_HEALTH = int.Parse(conf.ProblemConfiguration["MaxHealth"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("MaxShield"))
                {
                    MAX_SHIELD = int.Parse(conf.ProblemConfiguration["MaxShield"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("MaxAmmo"))
                {
                    MAX_AMMO = int.Parse(conf.ProblemConfiguration["MaxAmmo"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("MinPowerUpDistance"))
                {
                    MinPowerUpDistance = float.Parse(conf.ProblemConfiguration["MinPowerUpDistance"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("HealthBoxSpawnAmount"))
                {
                    HealthBoxSpawnAmount = int.Parse(conf.ProblemConfiguration["HealthBoxSpawnAmount"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("ShieldBoxSpawnAmount"))
                {
                    ShieldBoxSpawnAmount = int.Parse(conf.ProblemConfiguration["ShieldBoxSpawnAmount"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AmmoBoxSpawnAmount"))
                {
                    AmmoBoxSpawnAmount = int.Parse(conf.ProblemConfiguration["AmmoBoxSpawnAmount"]);
                }
            }
        }

        private void CheckAgentsPickedPowerUps()
        {
            foreach (RobostrikeAgentComponent agent in Agents)
            {
                if (agent.gameObject.activeSelf)
                {
                    List<PowerUpComponent> pickedPowerUps = PhysicsUtil.PhysicsOverlapTargetObjects<PowerUpComponent>(GameType, agent.gameObject, agent.transform.position, AgentColliderExtendsMultiplier.x, Vector3.zero, agent.transform.rotation, PhysicsOverlapType.OverlapSphere, false, gameObject.layer, DefaultLayer);

                    if (pickedPowerUps != null && pickedPowerUps.Count > 0)
                    {
                        foreach (PowerUpComponent powerUp in pickedPowerUps)
                        {
                            if (AgentPickedUpPowerUp(agent, powerUp))
                                Destroy(powerUp.gameObject);
                        }
                    }
                }
            }
        }

        private bool AgentPickedUpPowerUp(RobostrikeAgentComponent agent, PowerUpComponent powerUpComponent)
        {
            switch (powerUpComponent.PowerUpType)
            {
                case PowerUpType.Health:
                    return HealthBoxPickedUp(powerUpComponent, agent);
                case PowerUpType.Shield:
                    return ShieldBoxPickedUp(powerUpComponent, agent);
                case PowerUpType.Ammo:
                    return AmmoBoxPickedUp(powerUpComponent, agent);
                default:
                    {
                        throw new Exception("Unsuported PowerUpType!");
                        // TODO Add error reporting here
                    }
            }
        }

        public bool HealthBoxPickedUp(PowerUpComponent replacedPowerUpComponent, AgentComponent agent)
        {
            bool operationSuccess = ((RobostrikeAgentComponent)agent).SetHealth(HealthPowerUpValue);
            if (operationSuccess)
            {
                //Update agent fitness
                agent.AgentFitness.UpdateFitness(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.AgentPickedUpHealthBoxPowerUp.ToString()], RobostrikeFitness.FitnessKeys.AgentPickedUpHealthBoxPowerUp.ToString());

                // Spawn new health box
                if (HealthBoxPrefab != null)
                {
                    PowerUpSpawner.SpawnPowerUp<PowerUpComponent>(this, HealthBoxPrefab);
                }
                (agent as RobostrikeAgentComponent).UpdatetStatBars();
            }
            return operationSuccess;
        }

        public bool ShieldBoxPickedUp(PowerUpComponent replacedPowerUpComponent, AgentComponent agent)
        {
            bool operationSuccess = ((RobostrikeAgentComponent)agent).SetShield(ShieldPowerUpValue);
            if (operationSuccess)
            {
                //Update agent fitness
                agent.AgentFitness.UpdateFitness(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.AgentPickedUpShieldBoxPowerUp.ToString()], RobostrikeFitness.FitnessKeys.AgentPickedUpShieldBoxPowerUp.ToString());

                // Spawn new shield box
                if (ShieldBoxPrefab != null)
                {
                    PowerUpSpawner.SpawnPowerUp<PowerUpComponent>(this, ShieldBoxPrefab);
                }
                (agent as RobostrikeAgentComponent).UpdatetStatBars();
            }

            return operationSuccess;
        }

        public bool AmmoBoxPickedUp(PowerUpComponent replacedPowerUpComponent, AgentComponent agent)
        {
            bool operationSuccess = ((RobostrikeAgentComponent)agent).SetAmmo(AmmoPowerUpValue);
            if (operationSuccess)
            {
                //Update agent fitness
                agent.AgentFitness.UpdateFitness(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.AgentPickedUpAmmoBoxPowerUp.ToString()], RobostrikeFitness.FitnessKeys.AgentPickedUpAmmoBoxPowerUp.ToString());

                // Spawn new ammo box
                if (AmmoBoxPrefab != null)
                {
                    PowerUpSpawner.SpawnPowerUp<PowerUpComponent>(this, AmmoBoxPrefab);
                }
                (agent as RobostrikeAgentComponent).UpdatetStatBars();
            }

            return operationSuccess;
        }

        private void CheckAgentsExploration()
        {
            // Exploration bonus
            foreach (RobostrikeAgentComponent agent in Agents)
            {
                if (agent.gameObject.activeSelf)
                {
                    foreach (SectorComponent sector in sectors)
                    {
                        Vector3 sectorPosition = sector.transform.position;
                        if (IsAgentInSector(agent.transform.position, sector.gameObject.GetComponent<Collider2D>()))
                        {
                            if (agent.LastSectorPosition == Vector3.zero)
                            {
                                // Agent explored new sector
                                if (RobostrikeFitness.FitnessValues[RobostrikeFitness.Keys[(int)RobostrikeFitness.FitnessKeys.AgentExploredSector]] != 0)
                                {
                                    agent.AgentFitness.UpdateFitness((RobostrikeFitness.FitnessValues[RobostrikeFitness.Keys[(int)RobostrikeFitness.FitnessKeys.AgentExploredSector]]), RobostrikeFitness.FitnessKeys.AgentExploredSector.ToString());
                                }

                                agent.LastKnownPositions.Add(sectorPosition);
                                agent.LastSectorPosition = sector.transform.position;
                            }
                            else
                            {
                                if (agent.LastSectorPosition != sectorPosition)
                                {
                                    if (!agent.LastKnownPositions.Contains(sectorPosition))
                                    {
                                        // Agent explored new sector
                                        if (RobostrikeFitness.FitnessValues[RobostrikeFitness.Keys[(int)RobostrikeFitness.FitnessKeys.AgentExploredSector]] != 0)
                                        {
                                            agent.AgentFitness.UpdateFitness((RobostrikeFitness.FitnessValues[RobostrikeFitness.Keys[(int)RobostrikeFitness.FitnessKeys.AgentExploredSector]]), RobostrikeFitness.FitnessKeys.AgentExploredSector.ToString());
                                        }

                                        agent.LastKnownPositions.Add(sectorPosition);
                                    }

                                    agent.LastSectorPosition = sector.transform.position;
                                }
                            }

                            // Agent can only be in one sector at once
                            return;
                        }
                    }
                }
            }
        }

        private bool IsAgentInSector(Vector3 agentPosition, Collider2D colliderComponent)
        {
            if (colliderComponent.bounds.Contains(agentPosition))
            {
                return true;
            }

            return false;
        }
    }

    public enum RobostrikeGameScenarioType
    {
        Normal,
        Deathmatch
    }

    public enum RobostrikeAgentRespawnType
    {
        StartPos,
        Random
    }

}