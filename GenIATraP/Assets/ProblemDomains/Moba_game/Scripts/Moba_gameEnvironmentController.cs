using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using Base;
using Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

namespace Problems.Moba_game
{
    public class Moba_gameEnvironmentController : EnvironmentControllerBase
    {
        [SerializeField] public static int MAX_BASE_HEALTH = 20;
        [SerializeField] public static int MAX_HEALTH = 10;
        [SerializeField] public static int MAX_ENERGY = 30000;

        [Header("Moba_game General Configuration")]
        [SerializeField] int AgentStartHealth = 10;
        [SerializeField] int AgentStartEnergy = 30;
        [SerializeField] int BaseStartHealth = 20;
        [SerializeField] int AgentStartAmmo = 50;

        [SerializeField] public Moba_gameAgentRespawnType AgentRespawnType = Moba_gameAgentRespawnType.StartPos;
        [SerializeField] public Moba_gameGameScenarioType GameScenarioType = Moba_gameGameScenarioType.Normal;
        [SerializeField] public Boolean FrienlyFire = false;
        private Moba_game1vs1MatchSpawner Match1v1Spawner;


        [Header("Moba_game Base Configuration")]
        [SerializeField] public GameObject BasePrefab;
        private List<BaseComponent> Bases;

        [Header("Moba_game Planets Configuration")]

        [SerializeField] public GameObject LavaPlanetPrefab;
        [SerializeField] public int LavaPlanetSpawnAmount = 3;
        [SerializeField] public GameObject IcePlanetPrefab;
        [SerializeField] public int IcePlanetSpawnAmount = 3;
        private Moba_gamePlanetSpawner PlanetSpawner;
        private List<PlanetComponent> Planets;

        [Header("Moba_game Movement Configuration")]
        [SerializeField] public float AgentMoveSpeed = 5f;
        [SerializeField] public float AgentRotationSpeed = 80f;
        [SerializeField] public float AgentTurrentRotationSpeed = 90f;
        [HideInInspector] public float ForwardSpeed = 1f;
        [SerializeField] public float LavaAgentForwardThrust = 5f;
        [SerializeField] public float LavaAgentTourque = 1f;
        [SerializeField] public float IceAgentForwardThrust = 2.5f;
        [SerializeField] public float IceAgentTourque = 0.5f;

        [Header("Moba_game Missile Configuration")]
        [SerializeField] public GameObject MissilePrefab;
        [SerializeField, Tooltip("Destroy Missile After X seconds")] public float DestroyMissileAfter = 3.0f;
        [SerializeField] public float MissileShootCooldown = 1.0f;
        [SerializeField] public float MissleLaunchSpeed = 20f;
        [SerializeField] public static int MissileDamage = 2;


        [Header("Moba_game Planets Configuration")]
        [SerializeField] public float MinPowerUpDistance = 8f;
        [SerializeField] public float MinPowerUpDistanceFromAgents = 8f;
        [SerializeField] public Vector3 PowerUpColliderExtendsMultiplier = new Vector3(0.505f, 0.495f, 0.505f);

        [Header("Moba_game Stats Text Configuration")]
        [SerializeField] public TextMeshProUGUI Base0HealthText;
        [SerializeField] public TextMeshProUGUI Base0LavaText;
        [SerializeField] public TextMeshProUGUI Base0IceText;

        [SerializeField] public TextMeshProUGUI Base1HealthText;
        [SerializeField] public TextMeshProUGUI Base1LavaText;
        [SerializeField] public TextMeshProUGUI Base1IceText;

        [SerializeField] public TextMeshProUGUI Base2HealthText;
        [SerializeField] public TextMeshProUGUI Base2LavaText;
        [SerializeField] public TextMeshProUGUI Base2IceText;

        [SerializeField] public TextMeshProUGUI Base3HealthText;
        [SerializeField] public TextMeshProUGUI Base3LavaText;
        [SerializeField] public TextMeshProUGUI Base3IceText;

        public MissileController MissileController { get; set; }



        // Sectors
        private SectorComponent[] Sectors;

        // Fitness calculation
        private float sectorExplorationFitness;
        private float allPossibleLasersFired;
        private float lasersFired;
        private float lasersFiredOpponentAccuracy;
        private float lasersFiredBaseAccuracy;
        private float survivalBonus;
        private float baseHitsBonus;
        private float teammateHitsPenatly;
        private float ownBaseHitsPenalty;
        private int numOfOpponents;
        private int numOfAllLavaPlanetOrbitEnters;
        private float lavaPlanetOrbitEnters;
        private int numOfAllIcePlanetOrbitEnters;
        private float icePlanetOrbitEnters;
        private int numOfAllLavaPlanetOrbitCaptures;
        private float lavaPlanetOrbitCaptures;
        private int numOfAllIcePlanetOrbitCaptures;
        private float icePlanetOrbitCaptures;
        private int numOfOpponentBases;
        private float opponentsDestroyedBonus;
        private float opponentBasesDestroyedBonus;
        private int numOfFiredOpponentMissiles;
        private float damageTakenPenalty;
        private float opponentTrackingBonus;
        private Moba_gameAgentComponent agent;
        private Vector3 sectorPosition;

        private Moba_gameAgentComponent targetAgent;
        private Moba_gameAgentComponent senderAgent;

        // variables
        private BaseSpawner baseSpawner;
        private float lastBase0Health = -1;
        private float lastBase0Lava = -1;
        private float lastBase0Ice = -1;

        private float lastBase1Health = -1;
        private float lastBase1Lava = -1;
        private float lastBase1Ice = -1;

        private float lastBase2Health = -1;
        private float lastBase2Lava = -1;
        private float lastBase2Ice = -1;

        private float lastBase3Health = -1;
        private float lastBase3Lava = -1;
        private float lastBase3Ice = -1;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();

            Match1v1Spawner = GetComponent<Moba_game1vs1MatchSpawner>();
            if (Match1v1Spawner == null)
            {
                throw new Exception("Match1v1Spawner is not defined");
                // TODO Add error reporting here
            }

            baseSpawner = gameObject.GetComponent<BaseSpawner>();
            if (baseSpawner == null)
            {
                throw new Exception("baseSpawner is not defined");
                // TODO Add error reporting here
            }

            PlanetSpawner = GetComponent<Moba_gamePlanetSpawner>();
            if (PlanetSpawner == null)
            {
                throw new Exception("PlanetSpawner is not defined");
                // TODO Add error reporting here
            }

            if (SceneLoadMode == SceneLoadMode.LayerMode)
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

            // Spawn bases
            Bases = baseSpawner.Spawn<BaseComponent>(this).ToList();

            // Spawn planets
            Planets = PlanetSpawner.Spawn<PlanetComponent>(this).ToList();

            // Register event for Ray sensor
            RayHitObject.OnTargetHit += RayHitObject_OnTargetHit;
        }

        private void OnDestroy()
        {
            RayHitObject.OnTargetHit -= RayHitObject_OnTargetHit;
        }
        private float timer = 0f;
        protected override void OnPostFixedUpdate()
        {
            if (GameState == GameState.RUNNING)
            {
                //CheckAgentsPickedPowerUps();
                //MissileController.UpdateMissilePosAndCheckForColls();
                CheckAgentsExploration();
                UpdateAgentsSurvivalTime();
                ResetAgentOpponentTracking();
                UpdateBaseUI();
                SpawnNewAgents();

                timer += Time.deltaTime;
                if (timer >= 1f)
                {
                    UpdateBaseMoney();
                    UpdateAgentEnergy();
                    timer = 0f;
                    // Moba_gameAgentComponent a = Agents[0] as Moba_gameAgentComponent;
                    // Debug.Log("EnteredLavaPlanetOrbit: " + a.EnteredLavaPlanetOrbit);
                    // Debug.Log("EnteredIcePlanetOrbit: " +a.EnteredIcePlanetOrbit);
                    // Debug.Log("CapturedLavaPlanet: "+a.CapturedLavaPlanet);
                    // Debug.Log("CapturedIcePlanet: "+a.CapturedIcePlanet);
                }
            }
        }
        private void UpdateAgentEnergy()
        {
            foreach (Moba_gameAgentComponent agent in Agents)
            {
                if (agent.isActiveAndEnabled)
                {
                    agent.EnergyComponent.Energy--;
                    agent.UpdatetStatBars();
                    if (agent.EnergyComponent.Energy <= 0)
                    {
                        agent.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void UpdateBaseMoney()
        {
            int[] lavaCounts = new int[4];
            int[] iceCounts = new int[4];

            foreach (Moba_gamePlanetComponent planet in Planets)
            {
                if (planet.CapturedTeamID >= 0 && planet.CapturedTeamID <= 3)
                {
                    int teamIndex = planet.CapturedTeamID;
                    if (planet.Type == "lava")
                    {
                        lavaCounts[teamIndex]++;
                    }
                    else
                    {
                        iceCounts[teamIndex]++;
                    }
                }
            }

            foreach (Moba_gameBaseComponent baseComponent in Bases)
            {
                if (baseComponent.TeamID >= 0 && baseComponent.TeamID <= 3)
                {
                    int teamIndex = baseComponent.TeamID;
                    if (baseComponent.isActiveAndEnabled)
                    {
                        baseComponent.LavaAmount += lavaCounts[teamIndex];
                        baseComponent.IceAmount += iceCounts[teamIndex];
                    }
                }
            }
        }
        private void UpdateBaseUI()
        {
            float health;
            float lava;
            float ice;

            foreach (Moba_gameBaseComponent baseComponent in Bases)
            {
                health = baseComponent.HealthComponent.Health;
                lava = baseComponent.LavaAmount;
                ice = baseComponent.IceAmount;

                switch (baseComponent.TeamID)
                {
                    case 0:
                        if (health != lastBase0Health) Base0HealthText.text = (lastBase0Health = health).ToString();
                        if (lava != lastBase0Lava) Base0LavaText.text = (lastBase0Lava = lava).ToString();
                        if (ice != lastBase0Ice) Base0IceText.text = (lastBase0Ice = ice).ToString();
                        break;
                    case 1:
                        if (health != lastBase1Health) Base1HealthText.text = (lastBase1Health = health).ToString();
                        if (lava != lastBase1Lava) Base1LavaText.text = (lastBase1Lava = lava).ToString();
                        if (ice != lastBase1Ice) Base1IceText.text = (lastBase1Ice = ice).ToString();
                        break;
                    case 2:
                        if (health != lastBase2Health) Base2HealthText.text = (lastBase2Health = health).ToString();
                        if (lava != lastBase2Lava) Base2LavaText.text = (lastBase2Lava = lava).ToString();
                        if (ice != lastBase2Ice) Base2IceText.text = (lastBase2Ice = ice).ToString();
                        break;
                    case 3:
                        if (health != lastBase3Health) Base3HealthText.text = (lastBase3Health = health).ToString();
                        if (lava != lastBase3Lava) Base3LavaText.text = (lastBase3Lava = lava).ToString();
                        if (ice != lastBase3Ice) Base3IceText.text = (lastBase3Ice = ice).ToString();
                        break;
                }
            }
        }

        private void SpawnNewAgents()
        {
            float lava;
            float ice;
            bool canSpawn = true;
            foreach (Moba_gameBaseComponent baseComponent in Bases)
            {
                lava = baseComponent.LavaAmount;
                ice = baseComponent.IceAmount;

                if (lava >= 5 || ice >= 5)
                {
                    if (Agents != null)
                    {
                        foreach (Moba_gameAgentComponent agent in Agents)
                        {
                            if (agent.isActiveAndEnabled)
                            {
                                if (Vector3.Distance(agent.transform.position, Match1v1Spawner.SpawnPoints[baseComponent.TeamID].position) < 1.5f)
                                {
                                    canSpawn = false;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (canSpawn)
                {
                    if (lava >= 5 && ice >= 5)
                    {
                        if (lava >= ice)
                        {
                            Moba_gameAgentComponent obj = (Moba_gameAgentComponent)Match1v1Spawner.SpawnAgent<AgentComponent>(this, baseComponent.TeamID, "lava");
                            List<AgentComponent> agentList = Agents.ToList();
                            agentList.Add(obj);
                            Agents = agentList.ToArray();
                            baseComponent.LavaAmount -= 5;
                        }
                        else
                        {
                            Moba_gameAgentComponent obj = (Moba_gameAgentComponent)Match1v1Spawner.SpawnAgent<AgentComponent>(this, baseComponent.TeamID, "ice");
                            List<AgentComponent> agentList = Agents.ToList();
                            agentList.Add(obj);
                            Agents = agentList.ToArray();
                            baseComponent.IceAmount -= 5;
                        }
                    }
                    else if (lava >= 5)
                    {
                        Moba_gameAgentComponent obj = (Moba_gameAgentComponent)Match1v1Spawner.SpawnAgent<AgentComponent>(this, baseComponent.TeamID, "lava");
                        List<AgentComponent> agentList = Agents.ToList();
                        agentList.Add(obj);
                        Agents = agentList.ToArray();
                        baseComponent.LavaAmount -= 5;
                    }
                    else if (ice >= 5)
                    {
                        Moba_gameAgentComponent obj = (Moba_gameAgentComponent)Match1v1Spawner.SpawnAgent<AgentComponent>(this, baseComponent.TeamID, "ice");
                        List<AgentComponent> agentList = Agents.ToList();
                        agentList.Add(obj);
                        Agents = agentList.ToArray();
                        baseComponent.IceAmount -= 5;
                    }
                }
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

                if (conf.ProblemConfiguration.ContainsKey("MaxHealth"))
                {
                    MAX_HEALTH = int.Parse(conf.ProblemConfiguration["MaxHealth"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("MaxEnergy"))
                {
                    MAX_ENERGY = int.Parse(conf.ProblemConfiguration["MaxEnergy"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("MinPowerUpDistance"))
                {
                    MinPowerUpDistance = float.Parse(conf.ProblemConfiguration["MinPowerUpDistance"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("MinPowerUpDistanceFromAgents"))
                {
                    MinPowerUpDistanceFromAgents = float.Parse(conf.ProblemConfiguration["MinPowerUpDistanceFromAgents"]);
                }
            }
        }

        private void CheckAgentsExploration()
        {
            // Exploration bonus
            for (int i = 0; i < Agents.Length; i++)
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
            if (FrienlyFire)
            {
                if (hitAgent.TeamID == missile.Parent.TeamID)
                {
                    (missile.Parent as Moba_gameAgentComponent).MissileHitTeammate();
                }
                else
                {
                    (missile.Parent as Moba_gameAgentComponent).MissileHitOpponent();
                }
                (hitAgent as Moba_gameAgentComponent).HitByOpponentMissile();
                UpdateAgentHealth(missile, hitAgent as Moba_gameAgentComponent);
            }
            else
            {
                if (hitAgent.TeamID == missile.Parent.TeamID)
                {
                    //(missile.Parent as Moba_gameAgentComponent).MissileHitTeammate();
                }
                else
                {
                    (missile.Parent as Moba_gameAgentComponent).MissileHitOpponent();
                    UpdateAgentHealth(missile, hitAgent as Moba_gameAgentComponent);
                    (hitAgent as Moba_gameAgentComponent).HitByOpponentMissile();
                }
            }
        }
        public void LaserTankHit(Moba_gameAgentComponent agent, Moba_gameAgentComponent hitAgent)
        {
            if (FrienlyFire)
            {
                if (hitAgent.TeamID == agent.TeamID)
                {
                    agent.MissileHitTeammate();
                }
                else
                {
                    agent.MissileHitOpponent();
                }
                hitAgent.HitByOpponentMissile();
                UpdateAgentHealthLaser(agent, hitAgent);
            }
            else
            {
                if (hitAgent.TeamID != agent.TeamID)
                {
                    agent.MissileHitOpponent();
                    hitAgent.HitByOpponentMissile();
                    UpdateAgentHealthLaser(agent, hitAgent);
                }
            }
        }
        public void BaseHit(MissileComponent missile, BaseComponent hitBase)
        {
            if (FrienlyFire)
            {
                if (hitBase.TeamID == missile.Parent.TeamID)
                {
                    (missile.Parent as Moba_gameAgentComponent).MissileHitOwnBase();
                }
                else
                {
                    (missile.Parent as Moba_gameAgentComponent).MissileHitBase();
                }
                //UpdateBaseHealth(hitBase as Moba_gameBaseComponent);
            }
            else
            {
                if (hitBase.TeamID == missile.Parent.TeamID)
                {
                    //(missile.Parent as Moba_gameAgentComponent).MissileHitOwnBase();
                }
                else
                {
                    (missile.Parent as Moba_gameAgentComponent).MissileHitBase();
                    //UpdateBaseHealth(hitBase as Moba_gameBaseComponent);
                }
            }
        }
        public void LaserBaseHit(Moba_gameAgentComponent agent, Moba_gameBaseComponent hitBase)
        {
            if (FrienlyFire)
            {
                if (hitBase.TeamID == agent.TeamID)
                {
                    agent.MissileHitOwnBase();
                }
                else
                {
                    agent.MissileHitBase();
                }
                UpdateBaseHealth(agent, hitBase);
            }
            else
            {
                if (hitBase.TeamID != agent.TeamID)
                {
                    agent.MissileHitBase();
                    UpdateBaseHealth(agent, hitBase);
                }
            }
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
                        //CheckEndingState();
                        break;
                    case Moba_gameGameScenarioType.Deathmatch:
                        (missile.Parent as Moba_gameAgentComponent).OpponentsDestroyed++;

                        ResetAgent(hitAgent);
                        break;
                }
            }
        }

        void UpdateAgentHealthLaser(Moba_gameAgentComponent agent, Moba_gameAgentComponent hitAgent)
        {
            hitAgent.TakeDamage(MissileDamage);
            if (hitAgent.HealthComponent.Health <= 0)
            {
                switch (GameScenarioType)
                {
                    case Moba_gameGameScenarioType.Normal:
                        hitAgent.gameObject.SetActive(false);
                        agent.OpponentsDestroyed++;
                        //CheckEndingState();
                        break;
                    case Moba_gameGameScenarioType.Deathmatch:
                        agent.OpponentsDestroyed++;

                        ResetAgent(hitAgent);
                        break;
                }
            }
        }

        void UpdateBaseHealth(Moba_gameAgentComponent agent, Moba_gameBaseComponent hitBase)
        {
            if (hitBase == null)
            {
                Debug.LogError("hitBase je null v UpdateBaseHealth!");
                return;
            }
            hitBase.TakeDamage(MissileDamage);
            if (hitBase.HealthComponent.Health <= 0)
            {
                agent.OpponentBasesDestroyed++;
                hitBase.gameObject.SetActive(false);
            }

            CheckEndingState();
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

            // Restore ammo
            agent.AmmoComponent.Ammo = AgentStartAmmo;

            // Update Healthbar
            agent.UpdatetStatBars();

            // Set to new position
            MatchSpawner.Respawn<AgentComponent>(this, agent);
        }

        public override void CheckEndingState()
        {
            int activeBases = 0;
            foreach (Moba_gameBaseComponent _base in Bases)
            {
                if (_base.isActiveAndEnabled)
                {
                    activeBases++;
                }
            }
            if (activeBases == 1)
            {
                FinishGame();
            }
        }

        private void SetAgentsFitness()
        {
            foreach (Moba_gameAgentComponent agent in Agents)
            {
                // SectorExploration
                sectorExplorationFitness = agent.SectorsExplored / (float)Sectors.Length;
                sectorExplorationFitness = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.SectorExploration.ToString()] * sectorExplorationFitness, 4);
                agent.AgentFitness.UpdateFitness(sectorExplorationFitness, Moba_gameFitness.FitnessKeys.SectorExploration.ToString());

                // SurvivalBonus
                survivalBonus = agent.MaxSurvivalTime / (float)CurrentSimulationSteps;
                survivalBonus = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.SurvivalBonus.ToString()] * survivalBonus, 4);
                agent.AgentFitness.UpdateFitness(survivalBonus, Moba_gameFitness.FitnessKeys.SurvivalBonus.ToString());
                agent.ResetSurvivalTime();

                // LasersFired
                allPossibleLasersFired = (CurrentSimulationSteps * Time.fixedDeltaTime) / MissileShootCooldown;
                lasersFired = agent.LasersFired / allPossibleLasersFired;
                lasersFired = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.LasersFired.ToString()] * lasersFired, 4);
                agent.AgentFitness.UpdateFitness(lasersFired, Moba_gameFitness.FitnessKeys.LasersFired.ToString());

                // LaserOpponentAccuracy
                if (agent.LasersFired > 0)
                {
                    lasersFiredOpponentAccuracy = agent.MissilesHitOpponent / (float)agent.LasersFired;
                    lasersFiredOpponentAccuracy = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.LaserOpponentAccuracy.ToString()] * lasersFiredOpponentAccuracy, 4);
                    agent.AgentFitness.UpdateFitness(lasersFiredOpponentAccuracy, Moba_gameFitness.FitnessKeys.LaserOpponentAccuracy.ToString());
                }

                // LaserOpponentBaseLaserAccuracy
                if (agent.LasersFired > 0)
                {
                    lasersFiredBaseAccuracy = agent.MissilesHitBase / (float)agent.LasersFired;
                    lasersFiredBaseAccuracy = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.LaserOpponentBaseLaserAccuracy.ToString()] * lasersFiredBaseAccuracy, 4);
                    agent.AgentFitness.UpdateFitness(lasersFiredBaseAccuracy, Moba_gameFitness.FitnessKeys.LaserOpponentBaseLaserAccuracy.ToString());
                }

                // OpponentDestroyedBonus
                numOfOpponents = Agents.Where(a => a.TeamID != agent.TeamID).Select(a => (a as Moba_gameAgentComponent).NumOfSpawns).Sum();
                if (numOfOpponents > 0)
                {
                    opponentsDestroyedBonus = agent.OpponentsDestroyed / (float)numOfOpponents;
                    opponentsDestroyedBonus = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.OpponentDestroyedBonus.ToString()] * opponentsDestroyedBonus, 4);
                    agent.AgentFitness.UpdateFitness(opponentsDestroyedBonus, Moba_gameFitness.FitnessKeys.OpponentDestroyedBonus.ToString());
                }

                // OpponentBaseDestroyedBonus
                numOfOpponentBases = Match.Teams.Length - 1;
                if (numOfOpponentBases > 0)
                {
                    opponentBasesDestroyedBonus = agent.OpponentBasesDestroyed / (float)numOfOpponentBases;
                    opponentBasesDestroyedBonus = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.OpponentBaseDestroyedBonus.ToString()] * opponentBasesDestroyedBonus, 4);
                    agent.AgentFitness.UpdateFitness(opponentBasesDestroyedBonus, Moba_gameFitness.FitnessKeys.OpponentBaseDestroyedBonus.ToString());
                }

                // DamageTakenPenalty
                numOfFiredOpponentMissiles = Agents.Where(a => a.TeamID != agent.TeamID).Select(a => (a as Moba_gameAgentComponent).LasersFired).Sum();
                if (numOfFiredOpponentMissiles > 0)
                {
                    damageTakenPenalty = agent.HitByOpponentMissiles / (float)numOfFiredOpponentMissiles;
                    damageTakenPenalty = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.DamageTakenPenalty.ToString()] * damageTakenPenalty, 4);
                    agent.AgentFitness.UpdateFitness(damageTakenPenalty, Moba_gameFitness.FitnessKeys.DamageTakenPenalty.ToString());
                }

                // LavaPlanetOrbitEnter
                numOfAllLavaPlanetOrbitEnters = Agents.Select(a => (a as Moba_gameAgentComponent).EnteredLavaPlanetOrbit).Sum();
                if (numOfAllLavaPlanetOrbitEnters > 0)
                {
                    lavaPlanetOrbitEnters = agent.EnteredLavaPlanetOrbit / (float)numOfAllLavaPlanetOrbitEnters;
                    lavaPlanetOrbitEnters = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.LavaPlanetOrbitEnter.ToString()] * lavaPlanetOrbitEnters, 4);
                    agent.AgentFitness.UpdateFitness(lavaPlanetOrbitEnters, Moba_gameFitness.FitnessKeys.LavaPlanetOrbitEnter.ToString());
                }

                // IcePlanetOrbitEnter
                numOfAllIcePlanetOrbitEnters = Agents.Select(a => (a as Moba_gameAgentComponent).EnteredIcePlanetOrbit).Sum();
                if (numOfAllIcePlanetOrbitEnters > 0)
                {
                    icePlanetOrbitEnters = agent.EnteredIcePlanetOrbit / (float)numOfAllIcePlanetOrbitEnters;
                    icePlanetOrbitEnters = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.IcePlanetOrbitEnter.ToString()] * icePlanetOrbitEnters, 4);
                    agent.AgentFitness.UpdateFitness(icePlanetOrbitEnters, Moba_gameFitness.FitnessKeys.IcePlanetOrbitEnter.ToString());
                }

                // LavaPlanetOrbitCapture
                numOfAllLavaPlanetOrbitCaptures = Agents.Select(a => (a as Moba_gameAgentComponent).CapturedLavaPlanet).Sum();
                if (numOfAllLavaPlanetOrbitCaptures > 0)
                {
                    lavaPlanetOrbitCaptures = agent.CapturedLavaPlanet / (float)numOfAllLavaPlanetOrbitCaptures;
                    lavaPlanetOrbitCaptures = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.LavaPlanetOrbitCapture.ToString()] * lavaPlanetOrbitCaptures, 4);
                    agent.AgentFitness.UpdateFitness(lavaPlanetOrbitCaptures, Moba_gameFitness.FitnessKeys.LavaPlanetOrbitCapture.ToString());
                }

                // IcePlanetOrbitCapture
                numOfAllIcePlanetOrbitCaptures = Agents.Select(a => (a as Moba_gameAgentComponent).CapturedIcePlanet).Sum();
                if (numOfAllIcePlanetOrbitCaptures > 0)
                {
                    icePlanetOrbitCaptures = agent.CapturedIcePlanet / (float)numOfAllIcePlanetOrbitCaptures;
                    icePlanetOrbitCaptures = (float)Math.Round(Moba_gameFitness.FitnessValues[Moba_gameFitness.FitnessKeys.IcePlanetOrbitCapture.ToString()] * icePlanetOrbitCaptures, 4);
                    agent.AgentFitness.UpdateFitness(icePlanetOrbitCaptures, Moba_gameFitness.FitnessKeys.IcePlanetOrbitCapture.ToString());
                }

                /*
                Debug.Log("========================================");
                Debug.Log("Agent: Team ID" + agent.TeamID + ", ID: " + agent.IndividualID);
                Debug.Log("Sectors explored: " + agent.SectorsExplored + " / " + Sectors.Length + " = " + sectorExplorationFitness);
                Debug.Log("Survival bonus: " + agent.MaxSurvivalTime + " / " + CurrentSimulationSteps + " = " + survivalBonus);
                Debug.Log("Lasers fired: " + agent.LasersFired + " / " + allPossibleLasersFired + " = " + lasersFired);
                Debug.Log("Laser fired opponent accuracy: " + agent.MissilesHitOpponent + " / " + agent.LasersFired + " = " + lasersFiredOpponentAccuracy);
                Debug.Log("Laser fired base accuracy: " + agent.MissilesHitBase + " / " + agent.LasersFired + " = " + opponentsDestroyedBonus);
                Debug.Log("Opponents destroyed bonus: " + agent.OpponentsDestroyed + " / " + numOfOpponents + " = " + opponentsDestroyedBonus);
                Debug.Log("Opponent bases destroyed bonus: " + agent.OpponentBasesDestroyed + " / " + numOfOpponentBases + " = " + opponentBasesDestroyedBonus);
                Debug.Log("Damage taken penalty: " + agent.HitByOpponentMissiles + " / " + numOfFiredOpponentMissiles + " = " + damageTakenPenalty);
                Debug.Log("========================================");
                */
                Debug.Log("LavaPlanetOrbitEnter: " + agent.EnteredLavaPlanetOrbit + " / " + numOfAllLavaPlanetOrbitEnters + " = " + lavaPlanetOrbitEnters);
                Debug.Log("IcePlanetOrbitEnter: " + agent.EnteredIcePlanetOrbit + " / " + numOfAllIcePlanetOrbitEnters + " = " + icePlanetOrbitEnters);
                Debug.Log("LavaPlanetOrbitCapture: " + agent.CapturedLavaPlanet + " / " + numOfAllLavaPlanetOrbitCaptures + " = " + lavaPlanetOrbitCaptures);
                Debug.Log("IcePlanetOrbitCapture: " + agent.CapturedIcePlanet + " / " + numOfAllIcePlanetOrbitCaptures + " = " + icePlanetOrbitCaptures);


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