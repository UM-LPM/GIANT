using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using Base;
using Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

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
        [SerializeField] public RobostrikeAgentRespawnType AgentRespawnType = RobostrikeAgentRespawnType.StartPos;
        [SerializeField] public RobostrikeGameScenarioType GameScenarioType = RobostrikeGameScenarioType.Normal;

        [Header("Robostrike Movement Configuration")]
        [SerializeField] public float AgentMoveSpeed = 5f;
        [SerializeField] public float AgentRotationSpeed = 80f;
        [SerializeField] public float AgentTurrentRotationSpeed = 90f;
        [HideInInspector] public float ForwardSpeed = 1f;

        [Header("Robostrike Missile Configuration")]
        [SerializeField] public GameObject MissilePrefab;
        [SerializeField, Tooltip("Destroy Missile After X seconds")] public float DestroyMissileAfter = 3.0f;
        [SerializeField] public float MissileShootCooldown = 1.0f;
        [SerializeField] public float MissleLaunchSpeed = 20f;
        [SerializeField] public static int MissileDamage = 2;

        [Header("Robostrike PowerUps Configuration")]
        [SerializeField] int HealthPowerUpValue = 5;
        [SerializeField] int ShieldPowerUpValue = 5;
        [SerializeField] int AmmoPowerUpValue = 10;

        [Header("Robostrike PowerUps Prefabs")]
        [SerializeField] public float MinPowerUpDistance = 8f;
        [SerializeField] public float MinPowerUpDistanceFromAgents = 8f;
        [SerializeField] public Vector3 PowerUpColliderExtendsMultiplier = new Vector3(0.505f, 0.495f, 0.505f);
        [SerializeField] public GameObject HealthBoxPrefab;
        [SerializeField] public int HealthBoxSpawnAmount = 1;
        [SerializeField] public GameObject ShieldBoxPrefab;
        [SerializeField] public int ShieldBoxSpawnAmount = 2;
        [SerializeField] public GameObject AmmoBoxPrefab;
        [SerializeField] public int AmmoBoxSpawnAmount = 2;

        public MissileController MissileController { get; set; }

        private RobostrikePowerUpSpawner PowerUpSpawner;
        private List<PowerUpComponent> PowerUps;

        // Sectors
        private SectorComponent[] Sectors;

        // Fitness calculation
        private float sectorExplorationFitness;
        private float healthPowerUpsFitness;
        private float ammoPowerUpsFitness;
        private float shieldPowerUpsFitness;
        private float allPossibleMissilesFired;
        private float missilesFired;
        private float missilesFiredAccuracy;
        private float survivalBonus;
        private int numOfOpponents;
        private float opponentsDestroyedBonus;
        private int numOfFiredOpponentMissiles;
        private float damageTakenPenalty;
        private float opponentTrackingBonus;

        private List<PowerUpComponent> pickedPowerUps;

        private RobostrikeAgentComponent agent;
        private Vector3 sectorPosition;

        private RobostrikeAgentComponent targetAgent;
        private RobostrikeAgentComponent senderAgent;

        private string agentFitnessLog;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();

            PowerUpSpawner = GetComponent<RobostrikePowerUpSpawner>();
            if (PowerUpSpawner == null)
            {
                throw new Exception("RobostrikePowerUpSpawner is not defined");
            }

            Sectors = GetComponentsInChildren<SectorComponent>();

            MissileController = GetComponent<MissileController>();
            if (MissileController == null)
            {
                throw new Exception("MissileController is not defined");
            }
        }

        protected override void DefineAdditionalDataOnPostStart()
        {
            // Generate random color and assign it to the every agent stat bar
            Color color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0.0f, 1f), UnityEngine.Random.Range(0f, 1f), 1);

            // Set Agent stats
            foreach (RobostrikeAgentComponent agent in Agents)
            {
                agent.HealthComponent.Health = AgentStartHealth;
                agent.ShieldComponent.Shield = AgentStartShield;
                agent.AmmoComponent.Ammo = AgentStartAmmo;
                agent.SetEnvironmentColor(color);
            }

            // Spawn powerUps
            PowerUps = PowerUpSpawner.Spawn<PowerUpComponent>(this).ToList();

            // Register event for Ray sensor
            RayHitObject.OnTargetHit += RayHitObject_OnTargetHit;
        }

        private void OnDestroy()
        {
            RayHitObject.OnTargetHit -= RayHitObject_OnTargetHit;
        }

        protected override void OnPostFixedUpdate()
        {
            if (GameState == GameState.RUNNING)
            {
                CheckAgentsPickedPowerUps();
                MissileController.UpdateMissilePosAndCheckForColls();
                CheckAgentsExploration();
                UpdateAgentsSurvivalTime();
                ResetAgentOpponentTracking();
            }
        }

        protected override void OnPreFinishGame()
        {
            SetAgentsFitness();
        }

        public void ReadParamsFromMainConfiguration()
        {
            if (MenuManager.Instance != null && MenuManager.Instance.MainConfiguration != null)
            {
                MainConfiguration conf = MenuManager.Instance.MainConfiguration;

                RobostrikeFitness.FitnessValues = conf.FitnessValues;

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

                if (conf.ProblemConfiguration.ContainsKey("MinPowerUpDistanceFromAgents"))
                {
                    MinPowerUpDistanceFromAgents = float.Parse(conf.ProblemConfiguration["MinPowerUpDistanceFromAgents"]);
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
                    pickedPowerUps = PhysicsUtil.PhysicsOverlapTargetObjects<PowerUpComponent>(PhysicsScene, PhysicsScene2D, GameType, agent.gameObject, agent.transform.position, AgentColliderExtendsMultiplier.x, Vector3.zero, agent.transform.rotation, PhysicsOverlapType.OverlapSphere, false, gameObject.layer);

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
                    }
            }
        }

        public bool HealthBoxPickedUp(PowerUpComponent replacedPowerUpComponent, AgentComponent agent)
        {
            bool operationSuccess = ((RobostrikeAgentComponent)agent).SetHealth(HealthPowerUpValue);
            if (operationSuccess)
            {
                (agent as RobostrikeAgentComponent).HealtPowerUpsCollected++;

                // Spawn new health box
                if (HealthBoxPrefab != null)
                {
                    PowerUps.Remove(replacedPowerUpComponent);
                    PowerUps.Add(PowerUpSpawner.SpawnPowerUp<PowerUpComponent>(this, HealthBoxPrefab, PowerUps.Select(p => p.transform.position).ToList()));
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
                (agent as RobostrikeAgentComponent).ShieldPowerUpsCollected++;

                // Spawn new shield box
                if (ShieldBoxPrefab != null)
                {
                    PowerUps.Remove(replacedPowerUpComponent);
                    PowerUps.Add(PowerUpSpawner.SpawnPowerUp<PowerUpComponent>(this, ShieldBoxPrefab, PowerUps.Select(p => p.transform.position).ToList()));
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
                (agent as RobostrikeAgentComponent).AmmoPowerUpsCollected++;

                // Spawn new ammo box
                if (AmmoBoxPrefab != null)
                {
                    PowerUps.Remove(replacedPowerUpComponent);
                    PowerUps.Add(PowerUpSpawner.SpawnPowerUp<PowerUpComponent>(this, AmmoBoxPrefab, PowerUps.Select(p => p.transform.position).ToList()));
                }
                (agent as RobostrikeAgentComponent).UpdatetStatBars();
            }

            return operationSuccess;
        }

        private void CheckAgentsExploration()
        {
            // Exploration bonus
            for(int i = 0; i < Agents.Length; i++)
            {
                agent = Agents[i] as RobostrikeAgentComponent;
                if (agent.gameObject.activeSelf)
                {
                    foreach (SectorComponent sector in Sectors)
                    {
                        sectorPosition = sector.transform.position;
                        if (IsAgentInSector(agent.transform.position, sector.gameObject.GetComponent<Collider2D>()))
                        {
                            if (agent.LastSectorPosition == null || agent.LastSectorPosition != sectorPosition)
                            {
                                if (!agent.LastKnownSectorPositions.Contains(sectorPosition))
                                {
                                    // Agent explored new sector
                                    agent.SectorsExplored++;

                                    agent.LastKnownSectorPositions.Add(sectorPosition);
                                }

                                agent.LastSectorPosition = sector.transform.position;
                            }

                            // Agent can only be in one sector at once
                            break;
                        }
                    }
                }
            }
        }

        private void UpdateAgentsSurvivalTime()
        {
            foreach (RobostrikeAgentComponent agent in Agents)
            {
                if (agent.gameObject.activeSelf)
                {
                    agent.CurrentSurvivalTime++;
                }
            }
        }

        private void ResetAgentOpponentTracking()
        {
            foreach (RobostrikeAgentComponent agent in Agents)
            {
                if (agent.gameObject.activeSelf)
                {
                    agent.AlreadyTrackingOpponent = false;
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

        public void TankHit(MissileComponent missile, AgentComponent hitAgent)
        {
            (missile.Parent as RobostrikeAgentComponent).MissileHitOpponent();
            (hitAgent as RobostrikeAgentComponent).HitByOpponentMissile();

            UpdateAgentHealth(missile, hitAgent as RobostrikeAgentComponent);
        }

        void UpdateAgentHealth(MissileComponent missile, RobostrikeAgentComponent hitAgent)
        {
            hitAgent.TakeDamage(MissileDamage);

            if (hitAgent.HealthComponent.Health <= 0)
            {
                switch (GameScenarioType)
                {
                    case RobostrikeGameScenarioType.Normal:
                        hitAgent.gameObject.SetActive(false);
                        CheckEndingState();
                        break;
                    case RobostrikeGameScenarioType.Deathmatch:
                        (missile.Parent as RobostrikeAgentComponent).OpponentsDestroyed++;

                        ResetAgent(hitAgent);

                        break;
                }
            }
        }

        public override int GetNumOfActiveAgents()
        {
            int counter = 0;

            foreach (var agent in Agents)
            {
                if (agent.gameObject.activeSelf)
                    counter++;
            }

            return counter;
        }

        private void ResetAgent(RobostrikeAgentComponent agent)
        {
            agent.LastSectorPosition = null;
            agent.ResetSurvivalTime();

            // Restore health
            agent.HealthComponent.Health = AgentStartHealth;

            // Restore shield
            agent.ShieldComponent.Shield = AgentStartShield;

            // Restore ammo
            agent.AmmoComponent.Ammo = AgentStartAmmo;

            // Update Healthbar
            agent.UpdatetStatBars();

            // Set to new position
            MatchSpawner.Respawn<AgentComponent>(this, agent);
        }

        public override void CheckEndingState()
        {
            if (GetNumOfActiveAgents() == 1)
            {
                FinishGame();
            }
        }

        private void SetAgentsFitness()
        {
            foreach (RobostrikeAgentComponent agent in Agents)
            {
                // Sector exploration
                sectorExplorationFitness = agent.SectorsExplored / (float)Sectors.Length;
                sectorExplorationFitness = (float)Math.Round(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.SectorExploration.ToString()] * sectorExplorationFitness, 4);
                agent.AgentFitness.UpdateFitness(sectorExplorationFitness, RobostrikeFitness.FitnessKeys.SectorExploration.ToString());

                // Health powerUps
                healthPowerUpsFitness = agent.HealtPowerUpsCollected / (float)PowerUpSpawner.HealthBoxSpawned;
                healthPowerUpsFitness = (float)Math.Round(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.PowerUp_Pickup_Health.ToString()] * healthPowerUpsFitness, 4);
                agent.AgentFitness.UpdateFitness(healthPowerUpsFitness, RobostrikeFitness.FitnessKeys.PowerUp_Pickup_Health.ToString());

                // Ammo powerUps
                ammoPowerUpsFitness = agent.AmmoPowerUpsCollected / (float)PowerUpSpawner.AmmoBoxSpawned;
                ammoPowerUpsFitness = (float)Math.Round(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.PowerUp_Pickup_Ammo.ToString()] * ammoPowerUpsFitness, 4);
                agent.AgentFitness.UpdateFitness(ammoPowerUpsFitness, RobostrikeFitness.FitnessKeys.PowerUp_Pickup_Ammo.ToString());

                // Shield powerUps
                shieldPowerUpsFitness = agent.ShieldPowerUpsCollected / (float)PowerUpSpawner.ShieldBoxSpawned;
                shieldPowerUpsFitness = (float)Math.Round(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.PowerUp_Pickup_Shield.ToString()] * shieldPowerUpsFitness, 4);
                agent.AgentFitness.UpdateFitness(shieldPowerUpsFitness, RobostrikeFitness.FitnessKeys.PowerUp_Pickup_Shield.ToString());

                // Missiles fired
                allPossibleMissilesFired = (CurrentSimulationSteps * Time.fixedDeltaTime) / MissileShootCooldown;
                missilesFired = agent.MissilesFired / allPossibleMissilesFired;
                missilesFired = (float)Math.Round(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.MissilesFired.ToString()] * missilesFired, 4);
                agent.AgentFitness.UpdateFitness(missilesFired, RobostrikeFitness.FitnessKeys.MissilesFired.ToString());

                // Missiles fired accuracy
                if (agent.MissilesFired > 0)
                {
                    missilesFiredAccuracy = agent.MissilesHitOpponent / (float)agent.MissilesFired;
                    missilesFiredAccuracy = (float)Math.Round(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.MissilesFiredAccuracy.ToString()] * missilesFiredAccuracy, 4);
                    agent.AgentFitness.UpdateFitness(missilesFiredAccuracy, RobostrikeFitness.FitnessKeys.MissilesFiredAccuracy.ToString());
                }

                // Survival bonus
                agent.ResetSurvivalTime();

                survivalBonus = agent.MaxSurvivalTime / (float)CurrentSimulationSteps;
                survivalBonus = (float)Math.Round(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.SurvivalBonus.ToString()] * survivalBonus, 4);
                agent.AgentFitness.UpdateFitness(survivalBonus, RobostrikeFitness.FitnessKeys.SurvivalBonus.ToString());

                // Opponent tracking bonus
                opponentTrackingBonus = agent.OpponentTrackCounter / (CurrentSimulationSteps / (float)DecisionRequestInterval);
                opponentTrackingBonus = (float)Math.Round(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.OpponentTrackingBonus.ToString()] * opponentTrackingBonus, 4);
                agent.AgentFitness.UpdateFitness(opponentTrackingBonus, RobostrikeFitness.FitnessKeys.OpponentTrackingBonus.ToString());

                // Opponents destroyed
                numOfOpponents = Agents.Where(a => a.TeamIdentifier.TeamID != agent.TeamIdentifier.TeamID).Select(a => (a as RobostrikeAgentComponent).NumOfSpawns).Sum();
                if (numOfOpponents > 0)
                {
                    opponentsDestroyedBonus = agent.OpponentsDestroyed / (float)numOfOpponents;
                    opponentsDestroyedBonus = (float)Math.Round(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.OpponentDestroyedBonus.ToString()] * opponentsDestroyedBonus, 4);
                    agent.AgentFitness.UpdateFitness(opponentsDestroyedBonus, RobostrikeFitness.FitnessKeys.OpponentDestroyedBonus.ToString());
                }

                // Damage taken
                numOfFiredOpponentMissiles = Agents.Where(a => a.TeamIdentifier.TeamID != agent.TeamIdentifier.TeamID).Select(a => (a as RobostrikeAgentComponent).MissilesFired).Sum();
                if (numOfFiredOpponentMissiles > 0)
                {
                    damageTakenPenalty = agent.HitByOpponentMissiles / (float)numOfFiredOpponentMissiles;
                    damageTakenPenalty = (float)Math.Round(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.DamageTakenPenalty.ToString()] * damageTakenPenalty, 4);
                    agent.AgentFitness.UpdateFitness(damageTakenPenalty, RobostrikeFitness.FitnessKeys.DamageTakenPenalty.ToString());
                }

                agentFitnessLog = "========================================\n" +
                    $"[Agent]: Team ID + {agent.TeamIdentifier.TeamID} , ID: " + agent.IndividualID + "\n" +
                    $"[Sectors explored]: " + agent.SectorsExplored + " / " + Sectors.Length + " = " + sectorExplorationFitness + "\n" +
                    $"[Health powerUps]: " + agent.HealtPowerUpsCollected + " / " + PowerUpSpawner.HealthBoxSpawned + " = " + healthPowerUpsFitness + "\n" +
                    $"[Ammo powerUps]: " + agent.AmmoPowerUpsCollected + " / " + PowerUpSpawner.AmmoBoxSpawned + " = " + ammoPowerUpsFitness + "\n" +
                    $"[Shield powerUps]: " + agent.ShieldPowerUpsCollected + " / " + PowerUpSpawner.ShieldBoxSpawned + " = " + shieldPowerUpsFitness + "\n" +
                    $"[Missiles fired]: " + agent.MissilesFired + " / " + allPossibleMissilesFired + " = " + missilesFired + "\n" +
                    $"[Missiles fired accuracy]: " + agent.MissilesHitOpponent + " / " + agent.MissilesFired + " = " + missilesFiredAccuracy + "\n" +
                    $"[Survival bonus]: " + agent.MaxSurvivalTime + " / " + CurrentSimulationSteps + " = " + survivalBonus + "\n" + 
                    $"[Opponent tracking bonus]: " + agent.OpponentTrackCounter + " / " + (CurrentSimulationSteps / (float)DecisionRequestInterval) + " = " + opponentTrackingBonus + "\n" +
                    $"[Opponents destroyed bonus]: " + agent.OpponentsDestroyed + " / " + numOfOpponents + " = " + opponentsDestroyedBonus + "\n" +
                    $"[Damage taken]: " + agent.HitByOpponentMissiles + " / " + numOfFiredOpponentMissiles + " = " + damageTakenPenalty + "\n" +
                    "========================================\n";

                DebugSystem.LogVerbose(agentFitnessLog);                
            }
        }

        private void RayHitObject_OnTargetHit(object sender, OnTargetHitEventargs e)
        {
            targetAgent = e.TargetGameObject.GetComponent<RobostrikeAgentComponent>();
            senderAgent = e.Agent as RobostrikeAgentComponent;
            if (targetAgent != null && !senderAgent.AlreadyTrackingOpponent)
            {
                senderAgent.OpponentTrackCounter++;
                senderAgent.AlreadyTrackingOpponent = true;
            }
        }

        public void ResetAgentTrackingOpponent(RobostrikeAgentComponent agent)
        {
            agent.AlreadyTrackingOpponent = false;
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
        RandomPos
    }

}