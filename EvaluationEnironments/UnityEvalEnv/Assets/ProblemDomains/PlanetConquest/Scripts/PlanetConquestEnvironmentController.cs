using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using Base;
using Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Problems.PlanetConquest {
    public class PlanetConquestEnvironmentController : EnvironmentControllerBase
    {
        [SerializeField] public static int MAX_BASE_HEALTH = 20;
        [SerializeField] public static int MAX_HEALTH = 10;
        [SerializeField] public static int MAX_ENERGY = 30;

        [Header(" PlanetConquest General Configuration")]
        [SerializeField] int AgentStartHealth = 10;
        [SerializeField] int AgentStartEnergy = 30;
        [SerializeField] int BaseStartHealth = 20;
        [SerializeField] public int LaserEnergyConsumption = 5;
        [SerializeField] public int LaserHitEnergyBonus = 10;
        [SerializeField] public bool UnlimitedEnergy = false;
        [SerializeField] public PlanetConquestAgentRespawnType AgentRespawnType = PlanetConquestAgentRespawnType.StartPos;
        [SerializeField] public PlanetConquestGameScenarioType GameScenarioType = PlanetConquestGameScenarioType.Normal;
        [SerializeField] public bool FrienlyFire = false;
        [SerializeField] private int LavaAgentCost = 5;
        [SerializeField] private int IceAgentCost = 5;

        [SerializeField] public Color PlanetOrbColor = new Color(1, 1, 1, 0.2f);
        [SerializeField]
        public Color[] TeamColors =
        {
            new Color(1, 0, 0, 0.6f),    // Team 1: Red
            new Color(0, 0, 1, 0.6f),   // Team 2: Blue
            new Color(0, 1, 0, 0.6f),  // Team 3: Green
            new Color(1, 0, 1, 0.6f)  // Team 4: Purple
        };

        private PlanetConquest1vs1MatchSpawner Match1v1Spawner;


        [Header(" PlanetConquest Base Configuration")]
        [SerializeField] public GameObject BasePrefab;
        private List<BaseComponent> Bases;

        [Header(" PlanetConquest Planets Configuration")]

        [SerializeField] public GameObject LavaPlanetPrefab;
        [SerializeField] public int LavaPlanetSpawnAmount = 3;
        [SerializeField] public GameObject IcePlanetPrefab;
        [SerializeField] public int IcePlanetSpawnAmount = 3;

        [SerializeField] public float PlanetCaptureTime = 5f;
        [HideInInspector] public float PlanetCaptureSpeed;

        private PlanetSpawner PlanetSpawner;
        private List<PlanetComponent> Planets;

        [Header(" PlanetConquest Movement Configuration")]
        [SerializeField] public float AgentMoveSpeed = 5f;
        [SerializeField] public float AgentRotationSpeed = 80f;
        [SerializeField] public float AgentTurrentRotationSpeed = 90f;
        [SerializeField] public float LavaAgentForwardThrust = 5f;
        [SerializeField] public float LavaAgentTourque = 1f;
        [SerializeField] public float IceAgentForwardThrust = 2.5f;
        [SerializeField] public float IceAgentTourque = 0.5f;

        [Header(" PlanetConquest Laser Configuration")]
        [SerializeField] public float LaserShootCooldown = 1.0f;
        [SerializeField] public static int LaserDamage = 2;


        [Header(" PlanetConquest Planets Configuration")]
        [SerializeField] public float MinPowerUpDistance = 8f;
        [SerializeField] public float MinPowerUpDistanceFromAgents = 8f;
        [SerializeField] public Vector3 PowerUpColliderExtendsMultiplier = new Vector3(0.505f, 0.495f, 0.505f);

        // Sectors
        private SectorComponent[] Sectors;

        // Fitness calculation
        private float sectorExplorationFitness;
        private float allPossibleLasersFired;
        private float lasersFired;
        private float lasersFiredOpponentAccuracy;
        private float lasersFiredBaseAccuracy;
        private float survivalBonus;
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
                Sectors = GetComponentsInChildren<SectorComponent>();
            }

            PlanetCaptureSpeed = 1f / PlanetCaptureTime;

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
                    if (planet.Type == PlanetType.Lava)
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
                if (canSpawn)
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

                if (conf.ProblemConfiguration.ContainsKey("LaserShootCooldown"))
                {
                    LaserShootCooldown = float.Parse(conf.ProblemConfiguration["LaserShootCooldown"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("LaserDamage"))
                {
                    LaserDamage = int.Parse(conf.ProblemConfiguration["LaserDamage"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentStartHealth"))
                {
                    AgentStartHealth = int.Parse(conf.ProblemConfiguration["AgentStartHealth"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("GameScenarioType"))
                {
                    GameScenarioType = (PlanetConquestGameScenarioType)int.Parse(conf.ProblemConfiguration["GameScenarioType"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentRespawnType"))
                {
                    AgentRespawnType = (PlanetConquestAgentRespawnType)int.Parse(conf.ProblemConfiguration["AgentRespawnType"]);
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
                // SectorExploration
                sectorExplorationFitness = agent.SectorsExplored / (float)Sectors.Length;
                sectorExplorationFitness = (float)Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.SectorExploration.ToString()] * sectorExplorationFitness, 4);
                agent.AgentFitness.UpdateFitness(sectorExplorationFitness, PlanetConquestFitness.FitnessKeys.SectorExploration.ToString());

                // SurvivalBonus
                survivalBonus = agent.MaxSurvivalTime / (float)CurrentSimulationSteps;
                survivalBonus = (float)Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.SurvivalBonus.ToString()] * survivalBonus, 4);
                agent.AgentFitness.UpdateFitness(survivalBonus, PlanetConquestFitness.FitnessKeys.SurvivalBonus.ToString());
                agent.ResetSurvivalTime();

                // LasersFired
                allPossibleLasersFired = (CurrentSimulationSteps * Time.fixedDeltaTime) / LaserShootCooldown;
                lasersFired = agent.LasersFired / allPossibleLasersFired;
                lasersFired = (float)Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.LasersFired.ToString()] * lasersFired, 4);
                agent.AgentFitness.UpdateFitness(lasersFired, PlanetConquestFitness.FitnessKeys.LasersFired.ToString());

                // LaserOpponentAccuracy
                if (agent.LasersFired > 0)
                {
                    lasersFiredOpponentAccuracy = agent.MissilesHitOpponent / (float)agent.LasersFired;
                    lasersFiredOpponentAccuracy = (float)Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.LaserOpponentAccuracy.ToString()] * lasersFiredOpponentAccuracy, 4);
                    agent.AgentFitness.UpdateFitness(lasersFiredOpponentAccuracy, PlanetConquestFitness.FitnessKeys.LaserOpponentAccuracy.ToString());
                }

                // LaserOpponentBaseLaserAccuracy
                if (agent.LasersFired > 0)
                {
                    lasersFiredBaseAccuracy = agent.MissilesHitBase / (float)agent.LasersFired;
                    lasersFiredBaseAccuracy = (float)Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.LaserOpponentBaseLaserAccuracy.ToString()] * lasersFiredBaseAccuracy, 4);
                    agent.AgentFitness.UpdateFitness(lasersFiredBaseAccuracy, PlanetConquestFitness.FitnessKeys.LaserOpponentBaseLaserAccuracy.ToString());
                }

                // OpponentDestroyedBonus
                numOfOpponents = Agents.Where(a => a.TeamIdentifier.TeamID != agent.TeamIdentifier.TeamID).Select(a => (a as PlanetConquestAgentComponent).NumOfSpawns).Sum();
                if (numOfOpponents > 0)
                {
                    opponentsDestroyedBonus = agent.OpponentsDestroyed / (float)numOfOpponents;
                    opponentsDestroyedBonus = (float)Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.OpponentDestroyedBonus.ToString()] * opponentsDestroyedBonus, 4);
                    agent.AgentFitness.UpdateFitness(opponentsDestroyedBonus, PlanetConquestFitness.FitnessKeys.OpponentDestroyedBonus.ToString());
                }

                // OpponentBaseDestroyedBonus
                numOfOpponentBases = Match.Teams.Length - 1;
                if (numOfOpponentBases > 0)
                {
                    opponentBasesDestroyedBonus = agent.OpponentBasesDestroyed / (float)numOfOpponentBases;
                    opponentBasesDestroyedBonus = (float)Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.OpponentBaseDestroyedBonus.ToString()] * opponentBasesDestroyedBonus, 4);
                    agent.AgentFitness.UpdateFitness(opponentBasesDestroyedBonus, PlanetConquestFitness.FitnessKeys.OpponentBaseDestroyedBonus.ToString());
                }

                // DamageTakenPenalty
                numOfFiredOpponentMissiles = Agents.Where(a => a.TeamIdentifier.TeamID != agent.TeamIdentifier.TeamID).Select(a => (a as PlanetConquestAgentComponent).LasersFired).Sum();
                if (numOfFiredOpponentMissiles > 0)
                {
                    damageTakenPenalty = agent.HitByOpponentMissiles / (float)numOfFiredOpponentMissiles;
                    damageTakenPenalty = (float)Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.DamageTakenPenalty.ToString()] * damageTakenPenalty, 4);
                    agent.AgentFitness.UpdateFitness(damageTakenPenalty, PlanetConquestFitness.FitnessKeys.DamageTakenPenalty.ToString());
                }

                // LavaPlanetOrbitEnter
                numOfAllLavaPlanetOrbitEnters = Agents.Select(a => (a as PlanetConquestAgentComponent).EnteredLavaPlanetOrbit).Sum();
                if (numOfAllLavaPlanetOrbitEnters > 0)
                {
                    lavaPlanetOrbitEnters = agent.EnteredLavaPlanetOrbit / (float)numOfAllLavaPlanetOrbitEnters;
                    lavaPlanetOrbitEnters = (float)Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.LavaPlanetOrbitEnter.ToString()] * lavaPlanetOrbitEnters, 4);
                    agent.AgentFitness.UpdateFitness(lavaPlanetOrbitEnters, PlanetConquestFitness.FitnessKeys.LavaPlanetOrbitEnter.ToString());
                }

                // IcePlanetOrbitEnter
                numOfAllIcePlanetOrbitEnters = Agents.Select(a => (a as PlanetConquestAgentComponent).EnteredIcePlanetOrbit).Sum();
                if (numOfAllIcePlanetOrbitEnters > 0)
                {
                    icePlanetOrbitEnters = agent.EnteredIcePlanetOrbit / (float)numOfAllIcePlanetOrbitEnters;
                    icePlanetOrbitEnters = (float)Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.IcePlanetOrbitEnter.ToString()] * icePlanetOrbitEnters, 4);
                    agent.AgentFitness.UpdateFitness(icePlanetOrbitEnters, PlanetConquestFitness.FitnessKeys.IcePlanetOrbitEnter.ToString());
                }

                // LavaPlanetOrbitCapture
                numOfAllLavaPlanetOrbitCaptures = Agents.Select(a => (a as PlanetConquestAgentComponent).CapturedLavaPlanet).Sum();
                if (numOfAllLavaPlanetOrbitCaptures > 0)
                {
                    lavaPlanetOrbitCaptures = agent.CapturedLavaPlanet / (float)numOfAllLavaPlanetOrbitCaptures;
                    lavaPlanetOrbitCaptures = (float)Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.LavaPlanetOrbitCapture.ToString()] * lavaPlanetOrbitCaptures, 4);
                    agent.AgentFitness.UpdateFitness(lavaPlanetOrbitCaptures, PlanetConquestFitness.FitnessKeys.LavaPlanetOrbitCapture.ToString());
                }

                // IcePlanetOrbitCapture
                numOfAllIcePlanetOrbitCaptures = Agents.Select(a => (a as PlanetConquestAgentComponent).CapturedIcePlanet).Sum();
                if (numOfAllIcePlanetOrbitCaptures > 0)
                {
                    icePlanetOrbitCaptures = agent.CapturedIcePlanet / (float)numOfAllIcePlanetOrbitCaptures;
                    icePlanetOrbitCaptures = (float)Math.Round(PlanetConquestFitness.FitnessValues[PlanetConquestFitness.FitnessKeys.IcePlanetOrbitCapture.ToString()] * icePlanetOrbitCaptures, 4);
                    agent.AgentFitness.UpdateFitness(icePlanetOrbitCaptures, PlanetConquestFitness.FitnessKeys.IcePlanetOrbitCapture.ToString());
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
                Debug.Log("Damage taken penalty: " + agent.HitByOpponentMissiles + " / " + numOfFiredOpponentMissiles + " = " + damageTakenPenalty);
                Debug.Log("========================================");

                // Debug.Log("LavaPlanetOrbitEnter: " + agent.EnteredLavaPlanetOrbit + " / " + numOfAllLavaPlanetOrbitEnters + " = " + lavaPlanetOrbitEnters);
                // Debug.Log("IcePlanetOrbitEnter: " + agent.EnteredIcePlanetOrbit + " / " + numOfAllIcePlanetOrbitEnters + " = " + icePlanetOrbitEnters);
                // Debug.Log("LavaPlanetOrbitCapture: " + agent.CapturedLavaPlanet + " / " + numOfAllLavaPlanetOrbitCaptures + " = " + lavaPlanetOrbitCaptures);
                // Debug.Log("IcePlanetOrbitCapture: " + agent.CapturedIcePlanet + " / " + numOfAllIcePlanetOrbitCaptures + " = " + icePlanetOrbitCaptures);


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
                    agent.MissileHitTeammate();
                }
                else
                {
                    agent.MissileHitOpponent(this);
                }
                hitAgent.HitByOpponentMissile();
                UpdateAgentHealthLaser(agent, hitAgent);
            }
            else
            {
                if (hitAgent.TeamIdentifier.TeamID != agent.TeamIdentifier.TeamID)
                {
                    agent.MissileHitOpponent(this);
                    hitAgent.HitByOpponentMissile();
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
                    agent.MissileHitOwnBase();
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
