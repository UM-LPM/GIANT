using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using Base;
using Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace Problems.Moba_game
{
    public class Moba_gameEnvironmentController : EnvironmentControllerBase
    {
        [SerializeField] public static int MAX_HEALTH = 10;
        [SerializeField] public static int MAX_SHIELD = 10;
        [SerializeField] public static int MAX_AMMO = 20;

        [Header("Moba_game General Configuration")]
        [SerializeField] int AgentStartHealth = 10;
        [SerializeField] int AgentStartShield = 0;
        [SerializeField] int AgentStartAmmo = 0;
        [SerializeField] public Moba_gameAgentRespawnType AgentRespawnType = Moba_gameAgentRespawnType.StartPos;
        [SerializeField] public Moba_gameGameScenarioType GameScenarioType = Moba_gameGameScenarioType.Normal;

        [Header("Moba_game Movement Configuration")]
        [SerializeField] public float AgentMoveSpeed = 5f;
        [SerializeField] public float AgentRotationSpeed = 80f;
        [SerializeField] public float AgentTurrentRotationSpeed = 90f;
        [HideInInspector] public float ForwardSpeed = 1f;

        [Header("Moba_game Missile Configuration")]
        [SerializeField] public GameObject MissilePrefab;
        [SerializeField, Tooltip("Destroy Missile After X seconds")] public float DestroyMissileAfter = 3.0f;
        [SerializeField] public float MissileShootCooldown = 1.0f;
        [SerializeField] public float MissleLaunchSpeed = 20f;
        [SerializeField] public static int MissileDamage = 2;

        [Header("Moba_game PowerUps Configuration")]
        [SerializeField] int HealthPowerUpValue = 5;
        [SerializeField] int ShieldPowerUpValue = 5;
        [SerializeField] int AmmoPowerUpValue = 10;

        [Header("Moba_game PowerUps Prefabs")]
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

        private Moba_gamePowerUpSpawner PowerUpSpawner;
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

        private Moba_gameAgentComponent agent;
        private Vector3 sectorPosition;

        private Moba_gameAgentComponent targetAgent;
        private Moba_gameAgentComponent senderAgent;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();

            PowerUpSpawner = GetComponent<Moba_gamePowerUpSpawner>();
            if (PowerUpSpawner == null)
            {
                throw new Exception("Moba_gamePowerUpSpawner is not defined");
                // TODO Add error reporting here
            }

            if(SceneLoadMode == SceneLoadMode.LayerMode)
            {
                // Only one problem environment exists
                Sectors = FindObjectsOfType<SectorComponent>();
            }
            else
            {
                // Each EnvironmentController contains its own problem environment
                Sectors = GetComponentsInChildren<SectorComponent>();
            }

            MissileController = GetComponent<MissileController>();
            if (MissileController == null)
            {
                throw new Exception("MissileController is not defined");
                // TODO Add error reporting here
            }
        }

        protected override void DefineAdditionalDataOnPostStart()
        {
            // Generate random color and assign it to the every agent stat bar
            Color color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0.0f, 1f), UnityEngine.Random.Range(0f, 1f), 1);

            // Set Agent stats
            foreach (Moba_gameAgentComponent agent in Agents)
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

                Moba_gameFitness.FitnessValues = conf.FitnessValues;

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
                    GameScenarioType = (Moba_gameGameScenarioType)int.Parse(conf.ProblemConfiguration["GameScenarioType"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentRespawnType"))
                {
                    AgentRespawnType = (Moba_gameAgentRespawnType)int.Parse(conf.ProblemConfiguration["AgentRespawnType"]);
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
            foreach (Moba_gameAgentComponent agent in Agents)
            {
                if (agent.gameObject.activeSelf)
                {
                    pickedPowerUps = PhysicsUtil.PhysicsOverlapTargetObjects<PowerUpComponent>(GameType, agent.gameObject, agent.transform.position, AgentColliderExtendsMultiplier.x, Vector3.zero, agent.transform.rotation, PhysicsOverlapType.OverlapSphere, false, gameObject.layer, DefaultLayer);

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

        private bool AgentPickedUpPowerUp(Moba_gameAgentComponent agent, PowerUpComponent powerUpComponent)
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
            bool operationSuccess = ((Moba_gameAgentComponent)agent).SetHealth(HealthPowerUpValue);
            if (operationSuccess)
            {
                (agent as Moba_gameAgentComponent).HealtPowerUpsCollected++;

                // Spawn new health box
                if (HealthBoxPrefab != null)
                {
                    PowerUps.Remove(replacedPowerUpComponent);
                    PowerUps.Add(PowerUpSpawner.SpawnPowerUp<PowerUpComponent>(this, HealthBoxPrefab, PowerUps.Select(p => p.transform.position).ToList()));
                }
                (agent as Moba_gameAgentComponent).UpdatetStatBars();
            }
            return operationSuccess;
        }

        public bool ShieldBoxPickedUp(PowerUpComponent replacedPowerUpComponent, AgentComponent agent)
        {
            bool operationSuccess = ((Moba_gameAgentComponent)agent).SetShield(ShieldPowerUpValue);
            if (operationSuccess)
            {
                (agent as Moba_gameAgentComponent).ShieldPowerUpsCollected++;

                // Spawn new shield box
                if (ShieldBoxPrefab != null)
                {
                    PowerUps.Remove(replacedPowerUpComponent);
                    PowerUps.Add(PowerUpSpawner.SpawnPowerUp<PowerUpComponent>(this, ShieldBoxPrefab, PowerUps.Select(p => p.transform.position).ToList()));
                }
                (agent as Moba_gameAgentComponent).UpdatetStatBars();
            }

            return operationSuccess;
        }

        public bool AmmoBoxPickedUp(PowerUpComponent replacedPowerUpComponent, AgentComponent agent)
        {
            bool operationSuccess = ((Moba_gameAgentComponent)agent).SetAmmo(AmmoPowerUpValue);
            if (operationSuccess)
            {
                (agent as Moba_gameAgentComponent).AmmoPowerUpsCollected++;

                // Spawn new ammo box
                if (AmmoBoxPrefab != null)
                {
                    PowerUps.Remove(replacedPowerUpComponent);
                    PowerUps.Add(PowerUpSpawner.SpawnPowerUp<PowerUpComponent>(this, AmmoBoxPrefab, PowerUps.Select(p => p.transform.position).ToList()));
                }
                (agent as Moba_gameAgentComponent).UpdatetStatBars();
            }

            return operationSuccess;
        }

        private void CheckAgentsExploration()
        {
            // Exploration bonus
            for(int i = 0; i < Agents.Length; i++)
            {
                agent = Agents[i] as Moba_gameAgentComponent;
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
                                    // TODO Remove
                                    /*// Agent explored new sector
                                    if (Moba_gameFitness.FitnessValues[Moba_gameFitness.Keys[(int)Moba_gameFitness.FitnessKeys.AgentExploredSector]] != 0)
                                    {
                                        agent.AgentFitness.UpdateFitness((Moba_gameFitness.FitnessValues[Moba_gameFitness.Keys[(int)Moba_gameFitness.FitnessKeys.AgentExploredSector]]), Moba_gameFitness.FitnessKeys.AgentExploredSector.ToString());
                                    }*/

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
            foreach (Moba_gameAgentComponent agent in Agents)
            {
                if (agent.gameObject.activeSelf)
                {
                    agent.CurrentSurvivalTime++;
                }
            }
        }

        private void ResetAgentOpponentTracking()
        {
            foreach (Moba_gameAgentComponent agent in Agents)
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
            (missile.Parent as Moba_gameAgentComponent).MissileHitOpponent();
            (hitAgent as Moba_gameAgentComponent).HitByOpponentMissile();

            UpdateAgentHealth(missile, hitAgent as Moba_gameAgentComponent);
        }

        void UpdateAgentHealth(MissileComponent missile, Moba_gameAgentComponent hitAgent)
        {
            hitAgent.TakeDamage(MissileDamage);

            if (hitAgent.HealthComponent.Health <= 0)
            {
                switch (GameScenarioType)
                {
                    case Moba_gameGameScenarioType.Normal:
                        hitAgent.gameObject.SetActive(false);
                        CheckEndingState();
                        break;
                    case Moba_gameGameScenarioType.Deathmatch:
                        (missile.Parent as Moba_gameAgentComponent).OpponentsDestroyed++;

                        ResetAgent(hitAgent);

                        break;
                }
            }
        }

        public int GetNumOfActiveAgents()
        {
            int counter = 0;

            foreach (var agent in Agents)
            {
                if (agent.gameObject.activeSelf)
                    counter++;
            }

            return counter;
        }

        private void ResetAgent(Moba_gameAgentComponent agent)
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
            foreach (Moba_gameAgentComponent agent in Agents)
            {
                // Sector exploration
                sectorExplorationFitness = agent.SectorsExplored / (float)Sectors.Length;
                sectorExplorationFitness = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.SectorExploration.ToString()] * sectorExplorationFitness, 4);
                agent.AgentFitness.UpdateFitness(sectorExplorationFitness, Moba_gameFitness.FitnessKeys.SectorExploration.ToString());

                // Health powerUps
                healthPowerUpsFitness = agent.HealtPowerUpsCollected / (float)PowerUpSpawner.HealthBoxSpawned;
                healthPowerUpsFitness = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.PowerUp_Pickup_Health.ToString()] * healthPowerUpsFitness, 4);
                agent.AgentFitness.UpdateFitness(healthPowerUpsFitness, Moba_gameFitness.FitnessKeys.PowerUp_Pickup_Health.ToString());

                // Ammo powerUps
                ammoPowerUpsFitness = agent.AmmoPowerUpsCollected / (float)PowerUpSpawner.AmmoBoxSpawned;
                ammoPowerUpsFitness = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.PowerUp_Pickup_Ammo.ToString()] * ammoPowerUpsFitness, 4);
                agent.AgentFitness.UpdateFitness(ammoPowerUpsFitness, Moba_gameFitness.FitnessKeys.PowerUp_Pickup_Ammo.ToString());

                // Shield powerUps
                shieldPowerUpsFitness = agent.ShieldPowerUpsCollected / (float)PowerUpSpawner.ShieldBoxSpawned;
                shieldPowerUpsFitness = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.PowerUp_Pickup_Shield.ToString()] * shieldPowerUpsFitness, 4);
                agent.AgentFitness.UpdateFitness(shieldPowerUpsFitness, Moba_gameFitness.FitnessKeys.PowerUp_Pickup_Shield.ToString());

                // Missiles fired
                allPossibleMissilesFired = (CurrentSimulationSteps * Time.fixedDeltaTime) / MissileShootCooldown;
                missilesFired = agent.MissilesFired / allPossibleMissilesFired;
                missilesFired = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.MissilesFired.ToString()] * missilesFired, 4);
                agent.AgentFitness.UpdateFitness(missilesFired, Moba_gameFitness.FitnessKeys.MissilesFired.ToString());

                // Missiles fired accuracy
                if (agent.MissilesFired > 0)
                {
                    missilesFiredAccuracy = agent.MissilesHitOpponent / (float)agent.MissilesFired;
                    missilesFiredAccuracy = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.MissilesFiredAccuracy.ToString()] * missilesFiredAccuracy, 4);
                    agent.AgentFitness.UpdateFitness(missilesFiredAccuracy, Moba_gameFitness.FitnessKeys.MissilesFiredAccuracy.ToString());
                }

                // Survival bonus
                agent.ResetSurvivalTime();

                survivalBonus = agent.MaxSurvivalTime / (float)CurrentSimulationSteps;
                survivalBonus = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.SurvivalBonus.ToString()] * survivalBonus, 4);
                agent.AgentFitness.UpdateFitness(survivalBonus, Moba_gameFitness.FitnessKeys.SurvivalBonus.ToString());

                // Opponent tracking bonus
                opponentTrackingBonus = agent.OpponentTrackCounter / (CurrentSimulationSteps / (float)DecisionRequestInterval);
                opponentTrackingBonus = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.OpponentTrackingBonus.ToString()] * opponentTrackingBonus, 4);
                agent.AgentFitness.UpdateFitness(opponentTrackingBonus, Moba_gameFitness.FitnessKeys.OpponentTrackingBonus.ToString());

                // Opponents destroyed
                numOfOpponents = Agents.Where(a => a.TeamID != agent.TeamID).Select(a => (a as Moba_gameAgentComponent).NumOfSpawns).Sum();
                if (numOfOpponents > 0)
                {
                    opponentsDestroyedBonus = agent.OpponentsDestroyed / (float)numOfOpponents;
                    opponentsDestroyedBonus = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.OpponentDestroyedBonus.ToString()] * opponentsDestroyedBonus, 4);
                    agent.AgentFitness.UpdateFitness(opponentsDestroyedBonus, Moba_gameFitness.FitnessKeys.OpponentDestroyedBonus.ToString());
                }

                // Damage taken
                numOfFiredOpponentMissiles = Agents.Where(a => a.TeamID != agent.TeamID).Select(a => (a as Moba_gameAgentComponent).MissilesFired).Sum();
                if (numOfFiredOpponentMissiles > 0)
                {
                    damageTakenPenalty = agent.HitByOpponentMissiles / (float)numOfFiredOpponentMissiles;
                    damageTakenPenalty = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.DamageTakenPenalty.ToString()] * damageTakenPenalty, 4);
                    agent.AgentFitness.UpdateFitness(damageTakenPenalty, Moba_gameFitness.FitnessKeys.DamageTakenPenalty.ToString());
                }

                /*
                Debug.Log("========================================");
                Debug.Log("Agent: Team ID" + agent.TeamID + ", ID: " + agent.IndividualID);
                Debug.Log("Sectors explored: " + agent.SectorsExplored + " / " + Sectors.Length + " =");
                Debug.Log("Health powerUps: " + agent.HealtPowerUpsCollected + " / " + PowerUpSpawner.HealthBoxSpawned + " =");
                Debug.Log("Ammo powerUps: " + agent.AmmoPowerUpsCollected + " / " + PowerUpSpawner.AmmoBoxSpawned + " =");
                Debug.Log("Shield powerUps: " + agent.ShieldPowerUpsCollected + " / " + PowerUpSpawner.ShieldBoxSpawned + " =");
                Debug.Log("Missiles fired: " + agent.MissilesFired + " / " + allPossibleMissilesFired + " =");
                Debug.Log("Missiles fired accuracy: " + agent.MissilesHitOpponent + " / " + agent.MissilesFired + " =");
                Debug.Log("Survival bonus: " + agent.MaxSurvivalTime + " / " + CurrentSimulationSteps + " =");
                Debug.Log("Opponent tracking bonus: " + agent.OpponentTrackCounter + " / " + (CurrentSimulationSteps / (float)DecisionRequestInterval) + " =");
                Debug.Log("Opponents destroyed bonus: " + agent.OpponentsDestroyed + " / " + numOfOpponents + " =");
                Debug.Log("Damage taken: " + agent.HitByOpponentMissiles + " / " + numOfFiredOpponentMissiles + " =");
                Debug.Log("========================================");
                */
            }
        }

        private void RayHitObject_OnTargetHit(object sender, OnTargetHitEventargs e)
        {
            targetAgent = e.TargetGameObject.GetComponent<Moba_gameAgentComponent>();
            senderAgent = e.Agent as Moba_gameAgentComponent;
            if (targetAgent != null && !senderAgent.AlreadyTrackingOpponent)
            {
                senderAgent.OpponentTrackCounter++;
                senderAgent.AlreadyTrackingOpponent = true;
            }
        }

        public void ResetAgentTrackingOpponent(Moba_gameAgentComponent agent)
        {
            agent.AlreadyTrackingOpponent = false;
        }
    }

    public enum Moba_gameGameScenarioType
    {
        Normal,
        Deathmatch
    }

    public enum Moba_gameAgentRespawnType
    {
        StartPos,
        RandomPos
    }

}