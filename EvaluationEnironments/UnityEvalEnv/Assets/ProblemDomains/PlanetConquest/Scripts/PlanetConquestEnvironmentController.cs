using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using Base;
using Configuration;
using Problems.Robostrike;
using System;
using System.Collections.Generic;
using System.Linq;
using AgentOrganizations;
using UnityEngine;

namespace Problems.PlanetConquest {
    public class PlanetConquestEnvironmentController : EnvironmentControllerBase
    {
        [SerializeField] public static int MAX_BASE_HEALTH = 20;
        [SerializeField] public static int MAX_AGENT_HEALTH = 10;
        [SerializeField] public static int MAX_AGENT_ENERGY = 30;

        [Header(" PlanetConquest General Configuration")]
        [SerializeField] public int AgentStartHealth = 10;
        [SerializeField] public int AgentStartEnergy = 30;
        [SerializeField] public int BaseStartHealth = 20;
        [SerializeField] public int LaserEnergyConsumption = 5;
        [SerializeField] public int LaserHitEnergyBonus = 10;
        [SerializeField] public bool UnlimitedEnergy = false;
        [SerializeField] public PlanetConquestAgentRespawnType AgentRespawnType = PlanetConquestAgentRespawnType.StartPos;
        [SerializeField] public PlanetConquestGameScenarioType GameScenarioType = PlanetConquestGameScenarioType.Normal;
        [SerializeField] public bool FrienlyFire = false;
        [SerializeField] private int LavaAgentCost = 5;
        [SerializeField] private int IceAgentCost = 5;
        [SerializeField] public Individual FixedOpponent;
        [SerializeField] public int MaxNumOfAgents = 10;

        [SerializeField] public GameObject LavaAgentPrefab;
        [SerializeField] public GameObject IceAgentPrefab;

        [SerializeField] public Color PlanetOrbColor = new Color(1, 1, 1, 0.2f);
        [SerializeField] public Color[] TeamColors =
        {
            new Color(1, 0, 0, 0.6f),    // Team 1: Red
            new Color(0, 0, 1, 0.6f),   // Team 2: Blue
            new Color(0, 1, 0, 0.6f),  // Team 3: Green
            new Color(1, 0, 1, 0.6f)  // Team 4: Purple
        };

        private PlanetConquest1vs1MatchSpawner Match1v1Spawner;


        [Header("PlanetConquest Base Configuration")]
        [SerializeField] public GameObject BasePrefab;
        private List<BaseComponent> Bases;

        [Header("PlanetConquest Planets Configuration")]

        [SerializeField] public GameObject LavaPlanetPrefab;
        [SerializeField] public int LavaPlanetSpawnAmount = 3;
        [SerializeField] public GameObject IcePlanetPrefab;
        [SerializeField] public int IcePlanetSpawnAmount = 3;

        [SerializeField] public float PlanetCaptureTime = 5f;
        [HideInInspector] public float PlanetCaptureSpeed;

        private PlanetSpawner PlanetSpawner;
        private List<PlanetComponent> Planets;

        [Header("PlanetConquest Movement Configuration")]
        [SerializeField] public float AgentMoveSpeed = 5f;
        [SerializeField] public float AgentRotationSpeed = 80f;
        [SerializeField] public float LavaAgentForwardThrust = 5f;
        [SerializeField] public float LavaAgentTourque = 1f;
        [SerializeField] public float IceAgentForwardThrust = 2.5f;
        [SerializeField] public float IceAgentTourque = 0.5f;

        [Header("PlanetConquest Laser Configuration")]
        [SerializeField] public float LaserRange = 10f;
        [SerializeField] public float LaserShootCooldown = 1.0f;
        [SerializeField] public static int LaserDamage = 2;

        // Sectors
        private SectorComponent[] Sectors;

        // Fitness calculation
        private double sectorExplorationFitness;
        private double allPossibleLasersFired;
        private double lasersFired;
        private double lasersFiredOpponentAccuracy;
        private double lasersFiredBaseAccuracy;
        private double survivalBonus;
        private int numOfOpponents;
        private int numOfAllLavaPlanetOrbitEnters;
        private double lavaPlanetOrbitEnters;
        private int numOfAllIcePlanetOrbitEnters;
        private double icePlanetOrbitEnters;
        private int numOfAllLavaPlanetOrbitCaptures;
        private double lavaPlanetOrbitCaptures;
        private int numOfAllIcePlanetOrbitCaptures;
        private double icePlanetOrbitCaptures;
        private int numOfOpponentBases;
        private double opponentsDestroyedBonus;
        private double opponentBasesDestroyedBonus;
        private int numOfFiredOpponentMissiles;
        private double damageTakenPenalty;
        private double opponentTrackingBonus;
        private PlanetConquestAgentComponent agent;
        private Vector3 sectorPosition;

        private PlanetConquestAgentComponent targetAgent;
        private PlanetConquestAgentComponent senderAgent;

        // variables
        private BaseSpawner baseSpawner;

        private float timer = 0f;

        private int[] lavaCounts = new int[4];
        private int[] iceCounts = new int[4];

        float lava;
        float ice;
        bool canSpawn;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();

            Match1v1Spawner = GetComponent<PlanetConquest1vs1MatchSpawner>();
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

            PlanetSpawner = GetComponent<PlanetSpawner>();
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
                Sectors = Environment.GetComponentsInChildren<SectorComponent>();
            }

            PlanetCaptureSpeed = 1f / PlanetCaptureTime;

            if(FixedOpponent != null)
            {
                FixedOpponent = FixedOpponent.Clone();
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

        protected override void OnPostFixedUpdate()
        {
            if (GameState == GameState.RUNNING)
            {
                // Update planet capturing
                UpdatePlanetCapture();

                CheckAgentsExploration();
                UpdateAgentsSurvivalTime();
                ResetAgentOpponentTracking();
                SpawnNewAgents();

                timer += Time.fixedDeltaTime;
                if (timer >= 1f)
                {
                    UpdateBaseMoney();
                    if (!UnlimitedEnergy)
                    {
                        UpdateAgentEnergy();
                    }
                    timer = 0f;
                }
            }
        }
        private void UpdateAgentEnergy()
        {
            foreach (PlanetConquestAgentComponent agent in Agents)
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
            Array.Clear(lavaCounts, 0, lavaCounts.Length);
            Array.Clear(iceCounts, 0, iceCounts.Length);

            foreach (PlanetComponent planet in Planets)
            {
                if (planet.CapturedTeamIdentifier.TeamID >= 0 && planet.CapturedTeamIdentifier.TeamID <= 3)
                {
                    int teamIndex = planet.CapturedTeamIdentifier.TeamID;
                    if (planet.PlanetType == PlanetType.Lava)
                    {
                        lavaCounts[teamIndex]++;
                    }
                    else
                    {
                        iceCounts[teamIndex]++;
                    }
                }
            }

            foreach (BaseComponent baseComponent in Bases)
            {
                if (baseComponent.TeamIdentifier.TeamID >= 0 && baseComponent.TeamIdentifier.TeamID <= 3)
                {
                    int teamIndex = baseComponent.TeamIdentifier.TeamID;
                    if (baseComponent.isActiveAndEnabled)
                    {
                        baseComponent.LavaAmount += lavaCounts[teamIndex];
                        baseComponent.IceAmount += iceCounts[teamIndex];
                    }
                }
            }
        }

        private void SpawnNewAgents()
        {
            canSpawn = true;
            foreach (BaseComponent baseComponent in Bases)
            {
                lava = baseComponent.LavaAmount;
                ice = baseComponent.IceAmount;

                if (lava >= LavaAgentCost || ice >= IceAgentCost)
                {
                    if (Agents != null)
                    {
                        foreach (PlanetConquestAgentComponent agent in Agents)
                        {
                            if (agent.isActiveAndEnabled)
                            {
                                if (Vector3.Distance(agent.transform.position, Match1v1Spawner.SpawnPoints[baseComponent.TeamIdentifier.TeamID].position) < 1.5f)
                                {
                                    canSpawn = false;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (canSpawn && Agents.Length < MaxNumOfAgents)
                {
                    if (lava >= LavaAgentCost && ice >= IceAgentCost)
                    {
                        if (lava >= ice)
                        {
                            PlanetConquestAgentComponent obj = (PlanetConquestAgentComponent)Match1v1Spawner.SpawnAgent<AgentComponent>(this, baseComponent.TeamIdentifier.TeamID, AgentType.Lava);
                            List<AgentComponent> agentList = Agents.ToList();
                            agentList.Add(obj);
                            Agents = agentList.ToArray();
                            baseComponent.LavaAmount -= LavaAgentCost;
                        }
                        else
                        {
                            PlanetConquestAgentComponent obj = (PlanetConquestAgentComponent)Match1v1Spawner.SpawnAgent<AgentComponent>(this, baseComponent.TeamIdentifier.TeamID, AgentType.Ice);
                            List<AgentComponent> agentList = Agents.ToList();
                            agentList.Add(obj);
                            Agents = agentList.ToArray();
                            baseComponent.IceAmount -= IceAgentCost;
                        }
                    }
                    else if (lava >= LavaAgentCost)
                    {
                        PlanetConquestAgentComponent obj = (PlanetConquestAgentComponent)Match1v1Spawner.SpawnAgent<AgentComponent>(this, baseComponent.TeamIdentifier.TeamID, AgentType.Lava);
                        List<AgentComponent> agentList = Agents.ToList();
                        agentList.Add(obj);
                        Agents = agentList.ToArray();
                        baseComponent.LavaAmount -= LavaAgentCost;
                    }
                    else if (ice >= IceAgentCost)
                    {
                        PlanetConquestAgentComponent obj = (PlanetConquestAgentComponent)Match1v1Spawner.SpawnAgent<AgentComponent>(this, baseComponent.TeamIdentifier.TeamID, AgentType.Ice);
                        List<AgentComponent> agentList = Agents.ToList();
                        agentList.Add(obj);
                        Agents = agentList.ToArray();
                        baseComponent.IceAmount -= IceAgentCost;
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

                PlanetConquestFitness.FitnessValues = conf.FitnessValues;

                if (conf.ProblemConfiguration.ContainsKey("MaxBaseHealth"))
                {
                    MAX_BASE_HEALTH = int.Parse(conf.ProblemConfiguration["MaxBaseHealth"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("MaxAgentHealth"))
                {
                    MAX_AGENT_HEALTH = int.Parse(conf.ProblemConfiguration["MaxAgentHealth"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("MaxAgentEnergy"))
                {
                    MAX_AGENT_ENERGY = int.Parse(conf.ProblemConfiguration["MaxAgentEnergy"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentStartHealth"))
                {
                    AgentStartHealth = int.Parse(conf.ProblemConfiguration["AgentStartHealth"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("AgentStartEnergy"))
                {
                    AgentStartEnergy = int.Parse(conf.ProblemConfiguration["AgentStartEnergy"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("BaseStartHealth"))
                {
                    BaseStartHealth = int.Parse(conf.ProblemConfiguration["BaseStartHealth"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("LaserEnergyConsumption"))
                {
                    LaserEnergyConsumption = int.Parse(conf.ProblemConfiguration["LaserEnergyConsumption"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("LaserHitEnergyBonus"))
                {
                    LaserHitEnergyBonus = int.Parse(conf.ProblemConfiguration["LaserHitEnergyBonus"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("UnlimitedEnergy"))
                {
                    UnlimitedEnergy = bool.Parse(conf.ProblemConfiguration["UnlimitedEnergy"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("FrienlyFire"))
                {
                    FrienlyFire = bool.Parse(conf.ProblemConfiguration["FrienlyFire"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("LavaAgentCost"))
                {
                    LavaAgentCost = int.Parse(conf.ProblemConfiguration["LavaAgentCost"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("IceAgentCost"))
                {
                    IceAgentCost = int.Parse(conf.ProblemConfiguration["IceAgentCost"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("GameScenarioType"))
                {
                    GameScenarioType = (PlanetConquestGameScenarioType)int.Parse(conf.ProblemConfiguration["GameScenarioType"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentRespawnType"))
                {
                    AgentRespawnType = (PlanetConquestAgentRespawnType)int.Parse(conf.ProblemConfiguration["AgentRespawnType"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("LavaPlanetSpawnAmount"))
                {
                    LavaPlanetSpawnAmount = int.Parse(conf.ProblemConfiguration["LavaPlanetSpawnAmount"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("IcePlanetSpawnAmount"))
                {
                    IcePlanetSpawnAmount = int.Parse(conf.ProblemConfiguration["IcePlanetSpawnAmount"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("PlanetCaptureTime"))
                {
                    PlanetCaptureTime = float.Parse(conf.ProblemConfiguration["PlanetCaptureTime"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentMoveSpeed"))
                {
                    AgentMoveSpeed = float.Parse(conf.ProblemConfiguration["AgentMoveSpeed"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("AgentRotationSpeed"))
                {
                    AgentRotationSpeed = float.Parse(conf.ProblemConfiguration["AgentRotationSpeed"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("LavaAgentForwardThrust"))
                {
                    LavaAgentForwardThrust = float.Parse(conf.ProblemConfiguration["LavaAgentForwardThrust"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("LavaAgentTourque"))
                {
                    LavaAgentTourque = float.Parse(conf.ProblemConfiguration["LavaAgentTourque"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("IceAgentForwardThrust"))
                {
                    IceAgentForwardThrust = float.Parse(conf.ProblemConfiguration["IceAgentForwardThrust"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("IceAgentTourque"))
                {
                    IceAgentTourque = float.Parse(conf.ProblemConfiguration["IceAgentTourque"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("LaserRange"))
                {
                    LaserRange = float.Parse(conf.ProblemConfiguration["LaserRange"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("LaserShootCooldown"))
                {
                    LaserShootCooldown = float.Parse(conf.ProblemConfiguration["LaserShootCooldown"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("LaserDamage"))
                {
                    LaserDamage = int.Parse(conf.ProblemConfiguration["LaserDamage"]);
                }

            }
        }

        private void CheckAgentsExploration()
        {
            // Exploration bonus
            for (int i = 0; i < Agents.Length; i++)
            {
                agent = Agents[i] as PlanetConquestAgentComponent;
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
                                    if (PlanetConquestFitness.FitnessValues[PlanetConquestFitness.Keys[(int)PlanetConquestFitness.FitnessKeys.AgentExploredSector]] != 0)
                                    {
                                        agent.AgentFitness.UpdateFitness((PlanetConquestFitness.FitnessValues[PlanetConquestFitness.Keys[(int)PlanetConquestFitness.FitnessKeys.AgentExploredSector]]), PlanetConquestFitness.FitnessKeys.AgentExploredSector.ToString());
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
            foreach (PlanetConquestAgentComponent agent in Agents)
            {
                if (agent.gameObject.activeSelf)
                {
                    agent.CurrentSurvivalTime++;
                }
            }
        }

        private void ResetAgentOpponentTracking()
        {
            foreach (PlanetConquestAgentComponent agent in Agents)
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

        void UpdateAgentHealthLaser(PlanetConquestAgentComponent agent, PlanetConquestAgentComponent hitAgent)
        {
            hitAgent.TakeDamage(LaserDamage);
            if (hitAgent.HealthComponent.Health <= 0)
            {
                switch (GameScenarioType)
                {
                    case PlanetConquestGameScenarioType.Normal:
                        hitAgent.gameObject.SetActive(false);
                        agent.OpponentsDestroyed++;
                        CheckEndingState();
                        break;
                    case PlanetConquestGameScenarioType.Deathmatch:
                        agent.OpponentsDestroyed++;

                        ResetAgent(hitAgent);
                        break;
                }
            }
        }

        void UpdateBaseHealth(PlanetConquestAgentComponent agent, BaseComponent hitBase)
        {
            if (hitBase == null)
            {
                Debug.LogError("hitBase je null v UpdateBaseHealth!");
                return;
            }
            hitBase.TakeDamage(LaserDamage);
            if (hitBase.HealthComponent.Health <= 0)
            {
                agent.OpponentBasesDestroyed++;
                hitBase.gameObject.SetActive(false);
            }

            CheckEndingState();
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

        private void ResetAgent(PlanetConquestAgentComponent agent)
        {
            agent.LastSectorPosition = null;
            agent.ResetSurvivalTime();

            // Restore health
            agent.HealthComponent.Health = AgentStartHealth;

            // Restore energy
            agent.EnergyComponent.Energy = AgentStartEnergy;

            // Update Healthbar
            agent.UpdatetStatBars();

            // Set to new position
            MatchSpawner.Respawn<AgentComponent>(this, agent);
        }

        public override void CheckEndingState()
        {
            int activeBases = 0;
            foreach (BaseComponent _base in Bases)
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
            foreach (PlanetConquestAgentComponent agent in Agents)
            {
                // Check if agent is 

                // SectorExploration
                sectorExplorationFitness = agent.SectorsExplored / (double)Sectors.Length;
                sectorExplorationFitness = (double)(Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.SectorExploration.ToString()] * sectorExplorationFitness, 4) / Agents.Length);
                agent.AgentFitness.UpdateFitness((int)sectorExplorationFitness, PlanetConquestFitness.FitnessKeys.SectorExploration.ToString());

                // SurvivalBonus
                survivalBonus = agent.MaxSurvivalTime / (double)CurrentSimulationSteps;
                survivalBonus = (double)(Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.SurvivalBonus.ToString()] * survivalBonus, 4) / Agents.Length);
                agent.AgentFitness.UpdateFitness((int)survivalBonus, PlanetConquestFitness.FitnessKeys.SurvivalBonus.ToString());
                agent.ResetSurvivalTime();

                // LasersFired
                allPossibleLasersFired = (CurrentSimulationSteps * Time.fixedDeltaTime) / LaserShootCooldown;
                lasersFired = agent.LasersFired / allPossibleLasersFired;
                lasersFired = (double)(Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.LasersFired.ToString()] * lasersFired, 4) / Agents.Length);
                agent.AgentFitness.UpdateFitness((int)lasersFired, PlanetConquestFitness.FitnessKeys.LasersFired.ToString());

                // LaserOpponentAccuracy
                if (agent.LasersFired > 0)
                {
                    lasersFiredOpponentAccuracy = agent.MissilesHitOpponent / (double)agent.LasersFired;
                    lasersFiredOpponentAccuracy = (double)(Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.LaserOpponentAccuracy.ToString()] * lasersFiredOpponentAccuracy, 4) / Agents.Length);
                    agent.AgentFitness.UpdateFitness((int)lasersFiredOpponentAccuracy, PlanetConquestFitness.FitnessKeys.LaserOpponentAccuracy.ToString());
                }

                // LaserOpponentBaseLaserAccuracy
                if (agent.LasersFired > 0)
                {
                    lasersFiredBaseAccuracy = agent.MissilesHitBase / (double)agent.LasersFired;
                    lasersFiredBaseAccuracy = (double)(Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.LaserOpponentBaseLaserAccuracy.ToString()] * lasersFiredBaseAccuracy, 4) / Agents.Length);
                    agent.AgentFitness.UpdateFitness((int)lasersFiredBaseAccuracy, PlanetConquestFitness.FitnessKeys.LaserOpponentBaseLaserAccuracy.ToString());
                }

                // OpponentDestroyedBonus
                numOfOpponents = Agents.Where(a => a.TeamIdentifier.TeamID != agent.TeamIdentifier.TeamID).Select(a => (a as PlanetConquestAgentComponent).NumOfSpawns).Sum();
                if (numOfOpponents > 0)
                {
                    opponentsDestroyedBonus = agent.OpponentsDestroyed / (double)numOfOpponents;
                    opponentsDestroyedBonus = (double)Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.OpponentDestroyedBonus.ToString()] * opponentsDestroyedBonus, 4);
                    agent.AgentFitness.UpdateFitness((int)opponentsDestroyedBonus, PlanetConquestFitness.FitnessKeys.OpponentDestroyedBonus.ToString());
                }

                // OpponentBaseDestroyedBonus
                numOfOpponentBases = Match.Teams.Length - 1;
                if (numOfOpponentBases > 0)
                {
                    opponentBasesDestroyedBonus = agent.OpponentBasesDestroyed / (double)numOfOpponentBases;
                    opponentBasesDestroyedBonus = (double)Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.OpponentBaseDestroyedBonus.ToString()] * opponentBasesDestroyedBonus, 4);
                    agent.AgentFitness.UpdateFitness((int)opponentBasesDestroyedBonus, PlanetConquestFitness.FitnessKeys.OpponentBaseDestroyedBonus.ToString());
                }

                // DamageTakenPenalty
                numOfFiredOpponentMissiles = Agents.Where(a => a.TeamIdentifier.TeamID != agent.TeamIdentifier.TeamID).Select(a => (a as PlanetConquestAgentComponent).LasersFired).Sum();
                if (numOfFiredOpponentMissiles > 0)
                {
                    damageTakenPenalty = agent.HitByOpponentLasers / (double)numOfFiredOpponentMissiles;
                    damageTakenPenalty = (double)(Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.DamageTakenPenalty.ToString()] * damageTakenPenalty, 4) / Agents.Length);
                    agent.AgentFitness.UpdateFitness((int)damageTakenPenalty, PlanetConquestFitness.FitnessKeys.DamageTakenPenalty.ToString());
                }

                // LavaPlanetOrbitEnter
                numOfAllLavaPlanetOrbitEnters = Agents.Select(a => (a as PlanetConquestAgentComponent).EnteredLavaPlanetOrbit).Sum();
                if (numOfAllLavaPlanetOrbitEnters > 0)
                {
                    lavaPlanetOrbitEnters = agent.EnteredLavaPlanetOrbit / (double)numOfAllLavaPlanetOrbitEnters;
                    lavaPlanetOrbitEnters = (double)(Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.LavaPlanetOrbitEnter.ToString()] * lavaPlanetOrbitEnters, 4) / Agents.Length);
                    agent.AgentFitness.UpdateFitness((int)lavaPlanetOrbitEnters, PlanetConquestFitness.FitnessKeys.LavaPlanetOrbitEnter.ToString());
                }

                // IcePlanetOrbitEnter
                numOfAllIcePlanetOrbitEnters = Agents.Select(a => (a as PlanetConquestAgentComponent).EnteredIcePlanetOrbit).Sum();
                if (numOfAllIcePlanetOrbitEnters > 0)
                {
                    icePlanetOrbitEnters = agent.EnteredIcePlanetOrbit / (double)numOfAllIcePlanetOrbitEnters;
                    icePlanetOrbitEnters = (double)(Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.IcePlanetOrbitEnter.ToString()] * icePlanetOrbitEnters, 4) / Agents.Length);
                    agent.AgentFitness.UpdateFitness((int)icePlanetOrbitEnters, PlanetConquestFitness.FitnessKeys.IcePlanetOrbitEnter.ToString());
                }

                // LavaPlanetOrbitCapture
                numOfAllLavaPlanetOrbitCaptures = Agents.Select(a => (a as PlanetConquestAgentComponent).CapturedLavaPlanet).Sum();
                if (numOfAllLavaPlanetOrbitCaptures > 0)
                {
                    lavaPlanetOrbitCaptures = agent.CapturedLavaPlanet / (double)numOfAllLavaPlanetOrbitCaptures;
                    lavaPlanetOrbitCaptures = (double)Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.LavaPlanetOrbitCapture.ToString()] * lavaPlanetOrbitCaptures, 4);
                    agent.AgentFitness.UpdateFitness((int)lavaPlanetOrbitCaptures, PlanetConquestFitness.FitnessKeys.LavaPlanetOrbitCapture.ToString());
                }

                // IcePlanetOrbitCapture
                numOfAllIcePlanetOrbitCaptures = Agents.Select(a => (a as PlanetConquestAgentComponent).CapturedIcePlanet).Sum();
                if (numOfAllIcePlanetOrbitCaptures > 0)
                {
                    icePlanetOrbitCaptures = agent.CapturedIcePlanet / (double)numOfAllIcePlanetOrbitCaptures;
                    icePlanetOrbitCaptures = (double)Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.IcePlanetOrbitCapture.ToString()] * icePlanetOrbitCaptures, 4);
                    agent.AgentFitness.UpdateFitness((int)icePlanetOrbitCaptures, PlanetConquestFitness.FitnessKeys.IcePlanetOrbitCapture.ToString());
                }

                Debug.Log("========================================");
                Debug.Log("Agent: Team ID" + agent.TeamIdentifier.TeamID + ", ID: " + agent.IndividualID);
                Debug.Log("Sectors explored: " + agent.SectorsExplored + " / " + Sectors.Length + " = " + sectorExplorationFitness);
                Debug.Log("Survival bonus: " + agent.MaxSurvivalTime + " / " + CurrentSimulationSteps + " = " + survivalBonus);
                Debug.Log("Lasers fired: " + agent.LasersFired + " / " + allPossibleLasersFired + " = " + lasersFired);
                Debug.Log("Laser fired opponent accuracy: " + agent.MissilesHitOpponent + " / " + agent.LasersFired + " = " + lasersFiredOpponentAccuracy);
                Debug.Log("Laser fired base accuracy: " + agent.MissilesHitBase + " / " + agent.LasersFired + " = " + opponentsDestroyedBonus);
                Debug.Log("Opponents destroyed bonus: " + agent.OpponentsDestroyed + " / " + numOfOpponents + " = " + opponentsDestroyedBonus);
                Debug.Log("Opponent bases destroyed bonus: " + agent.OpponentBasesDestroyed + " / " + numOfOpponentBases + " = " + opponentBasesDestroyedBonus);
                Debug.Log("Damage taken penalty: " + agent.HitByOpponentLasers + " / " + numOfFiredOpponentMissiles + " = " + damageTakenPenalty);
                Debug.Log("========================================");
            }
        }

        private void RayHitObject_OnTargetHit(object sender, OnTargetHitEventargs e)
        {
            targetAgent = e.TargetGameObject.GetComponent<PlanetConquestAgentComponent>();
            senderAgent = e.Agent as PlanetConquestAgentComponent;
            if (targetAgent != null && !senderAgent.AlreadyTrackingOpponent)
            {
                senderAgent.OpponentTrackCounter++;
                senderAgent.AlreadyTrackingOpponent = true;
            }
        }

        public void ResetAgentTrackingOpponent(PlanetConquestAgentComponent agent)
        {
            agent.AlreadyTrackingOpponent = false;
        }


        public void LaserSpaceShipHit(PlanetConquestAgentComponent agent, PlanetConquestAgentComponent hitAgent)
        {
            if (FrienlyFire)
            {
                if (hitAgent.TeamIdentifier.TeamID == agent.TeamIdentifier.TeamID)
                {
                    agent.LaserHitTeammate();
                }
                else
                {
                    agent.LaserHitOpponent(this);
                }
                hitAgent.HitByOpponentLaser();
                UpdateAgentHealthLaser(agent, hitAgent);
            }
            else
            {
                if (hitAgent.TeamIdentifier.TeamID != agent.TeamIdentifier.TeamID)
                {
                    agent.LaserHitOpponent(this);
                    hitAgent.HitByOpponentLaser();
                    UpdateAgentHealthLaser(agent, hitAgent);
                }
            }
        }

        public void LaserBaseHit(PlanetConquestAgentComponent agent, BaseComponent hitBase)
        {
            if (FrienlyFire)
            {
                if (hitBase.TeamIdentifier.TeamID == agent.TeamIdentifier.TeamID)
                {
                    agent.LaserHitOwnBase();
                }
                else
                {
                    agent.MissileHitBase(this);
                }
                UpdateBaseHealth(agent, hitBase);
            }
            else
            {
                if (hitBase.TeamIdentifier.TeamID != agent.TeamIdentifier.TeamID)
                {
                    agent.MissileHitBase(this);
                    UpdateBaseHealth(agent, hitBase);
                }
            }
        }

        public void UpdatePlanetCapture()
        {
            foreach(PlanetComponent planet in Planets)
            {
                planet.UpdateCaptureProgress(this);
            }
        }
    }

    public enum PlanetConquestGameScenarioType
    {
        Normal,
        Deathmatch
    }

    public enum PlanetConquestAgentRespawnType
    {
        StartPos,
        RandomPos
    }
}
