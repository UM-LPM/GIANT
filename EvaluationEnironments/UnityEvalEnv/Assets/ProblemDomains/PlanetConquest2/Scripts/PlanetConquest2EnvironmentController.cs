using AgentOrganizations;
using Base;
using Configuration;
using Problems.PlanetConquest;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Problems.PlanetConquest2
{
    public class PlanetConquest2EnvironmentController : EnvironmentControllerBase
    {
        [SerializeField] public static int MAX_BASE_HEALTH = 20;
        [SerializeField] public static int MAX_AGENT_HEALTH = 10;
        [SerializeField] public static int MAX_AGENT_ENERGY = 30;

        [Header("PlanetConquest General Configuration")]
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
        [SerializeField]
        public Color[] TeamColors =
        {
            new Color(1, 0, 0, 0.6f),
            new Color(0, 0, 1, 0.6f)
        };

        [Header("PlanetConquest Base Configuration")]
        [SerializeField] public GameObject BasePrefab;

        [Header("PlanetConquest Planets Configuration")]
        [SerializeField] public GameObject LavaPlanetPrefab;
        [SerializeField] public int LavaPlanetSpawnAmount = 3;
        [SerializeField] public GameObject IcePlanetPrefab;
        [SerializeField] public int IcePlanetSpawnAmount = 3;

        [SerializeField] public float PlanetCaptureTime = 5f;
        [HideInInspector] public float PlanetCaptureSpeed;

        [Header("PlanetConquest Movement Configuration")]
        [SerializeField] public float AgentMoveSpeed = 5f;
        [SerializeField] public float AgentRotationSpeed = 80f;
        [SerializeField] public float LavaAgentForwardThrust = 5f;
        [SerializeField] public float LavaAgentTourque = 1f;
        [SerializeField] public float IceAgentForwardThrust = 2.5f;
        [SerializeField] public float IceAgentTourque = 0.5f;
        [HideInInspector] public float ForwardSpeed = 1f;

        [Header("PlanetConquest Laser Configuration")]
        [SerializeField] public float LaserRange = 10f;
        [SerializeField] public float LaserShootCooldown = 1.0f;
        [SerializeField] public static int LaserDamage = 2;

        // Spawners
        private PlanetConquest2BaseSpawner baseSpawner;
        private PlanetConquest2PlanetSpawner PlanetSpawner;

        // Sectors, Planets, Bases
        private SectorComponent[] Sectors;
        private List<PlanetComponent> Planets;
        private List<BaseComponent> Bases;

        // Temporary variables
        private PlanetConquest2AgentComponent agent;
        private Vector3 sectorPosition;
        private float baseMoneyUpdateTime = 0f;

        float lava;
        float ice;
        bool canSpawn;

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
        private int numOfAllLavaPlanets;
        private double lavaPlanetOrbitCaptures;
        private int numOfAllIcePlanets;
        private double icePlanetOrbitCaptures;
        private int numOfOpponentBases;
        private double opponentsDestroyedBonus;
        private double opponentBasesDestroyedBonus;
        private int numOfFiredOpponentMissiles;
        private double damageTakenPenalty;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();

            baseSpawner = gameObject.GetComponent<PlanetConquest2BaseSpawner>();
            if (baseSpawner == null)
            {
                throw new Exception("baseSpawner is not defined");
                // TODO Add error reporting here
            }

            PlanetSpawner = GetComponent<PlanetConquest2PlanetSpawner>();
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

            if (FixedOpponent != null)
            {
                FixedOpponent = FixedOpponent.Clone();
            }

        }

        protected override void DefineAdditionalDataOnPostStart()
        {
            // Spawn planets
            Planets = PlanetSpawner.Spawn<PlanetComponent>(this).ToList();

            // Spawn bases
            Bases = baseSpawner.Spawn<BaseComponent>(this).ToList();
        }

        protected override void OnPostFixedUpdate()
        {
            if (GameState == GameState.RUNNING)
            {
                // Update planet capturing
                UpdatePlanetCapture();

                CheckAgentsExploration();
                UpdateAgentsSurvivalTime();
                SpawnNewAgents();

                baseMoneyUpdateTime += Time.fixedDeltaTime;
                if (baseMoneyUpdateTime >= 1f)
                {
                    UpdateBaseMoney();
                    if (!UnlimitedEnergy)
                    {
                        UpdateAgentEnergy();
                    }
                    baseMoneyUpdateTime = 0f;
                }
            }
        }

        public void UpdatePlanetCapture()
        {
            foreach (PlanetComponent planet in Planets)
            {
                planet.UpdateCaptureProgress(this);
            }
        }

        private void CheckAgentsExploration()
        {
            // Exploration bonus
            for (int i = 0; i < Agents.Length; i++)
            {
                agent = Agents[i] as PlanetConquest2AgentComponent;
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

        private void SpawnNewAgents()
        {
            // TODO
        }

        private void UpdateBaseMoney()
        {
            foreach (BaseComponent baseComponent in Bases)
            {
                foreach (PlanetComponent planet in Planets)
                {
                    if (planet.CapturedTeamIdentifier.TeamID == baseComponent.TeamIdentifier.TeamID)
                    {
                        if (planet.PlanetType == PlanetType.Lava)
                            baseComponent.LavaAmount++;
                        else
                        {
                            baseComponent.IceAmount++;
                        }
                    }
                }
            }
        }

        private void UpdateAgentEnergy()
        {
            foreach (PlanetConquest2AgentComponent agent in Agents)
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

        private void UpdateAgentsSurvivalTime()
        {
            foreach (PlanetConquest2AgentComponent agent in Agents)
            {
                if (agent.gameObject.activeSelf)
                {
                    agent.CurrentSurvivalTime++;
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

        void UpdateAgentHealthLaser(PlanetConquest2AgentComponent agent, PlanetConquest2AgentComponent hitAgent)
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

        void UpdateBaseHealth(PlanetConquest2AgentComponent agent, BaseComponent hitBase)
        {
            if (hitBase == null)
            {
                Debug.LogError("hitBase is null in UpdateBaseHealth!");
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

        private void ResetAgent(PlanetConquest2AgentComponent agent)
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

        public void LaserSpaceShipHit(PlanetConquest2AgentComponent agent, PlanetConquest2AgentComponent hitAgent)
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

        public void LaserBaseHit(PlanetConquest2AgentComponent agent, BaseComponent hitBase)
        {
            if (FrienlyFire)
            {
                if (hitBase.TeamIdentifier.TeamID == agent.TeamIdentifier.TeamID)
                {
                    agent.LaserHitOwnBase();
                }
                else
                {
                    agent.LaserHitBase(this);
                }
                UpdateBaseHealth(agent, hitBase);
            }
            else
            {
                if (hitBase.TeamIdentifier.TeamID != agent.TeamIdentifier.TeamID)
                {
                    agent.LaserHitBase(this);
                    UpdateBaseHealth(agent, hitBase);
                }
            }
        }

        protected override void OnPreFinishGame()
        {
            SetAgentsFitness();
        }

        private void SetAgentsFitness()
        {
            numOfAllLavaPlanets = Planets.Where(p => p.PlanetType == PlanetType.Lava).Count();
            numOfAllIcePlanets = Planets.Where(p => p.PlanetType == PlanetType.Ice).Count();

            foreach (PlanetConquest2AgentComponent agent in Agents)
            {
                // Check if agent is 

                // SectorExploration
                sectorExplorationFitness = agent.SectorsExplored / (double)Sectors.Length;
                sectorExplorationFitness = (double)(Math.Round(PlanetConquest2Fitness.FitnessValues[PlanetConquest2Fitness.FitnessKeys.SectorExploration.ToString()] * sectorExplorationFitness, 4) / Agents.Length);
                agent.AgentFitness.UpdateFitness((float)sectorExplorationFitness, PlanetConquest2Fitness.FitnessKeys.SectorExploration.ToString());

                // SurvivalBonus
                survivalBonus = agent.MaxSurvivalTime / (double)CurrentSimulationSteps;
                survivalBonus = (double)(Math.Round(PlanetConquest2Fitness.FitnessValues[PlanetConquest2Fitness.FitnessKeys.SurvivalBonus.ToString()] * survivalBonus, 4) / Agents.Length);
                agent.AgentFitness.UpdateFitness((float)survivalBonus, PlanetConquest2Fitness.FitnessKeys.SurvivalBonus.ToString());
                agent.ResetSurvivalTime();

                // LasersFired
                allPossibleLasersFired = (CurrentSimulationSteps * Time.fixedDeltaTime) / LaserShootCooldown;
                lasersFired = agent.LasersFired / allPossibleLasersFired;
                lasersFired = (double)(Math.Round(PlanetConquest2Fitness.FitnessValues[PlanetConquest2Fitness.FitnessKeys.LasersFired.ToString()] * lasersFired, 4) / Agents.Length);
                agent.AgentFitness.UpdateFitness((float)lasersFired, PlanetConquest2Fitness.FitnessKeys.LasersFired.ToString());

                // LaserOpponentAccuracy
                if (agent.LasersFired > 0)
                {
                    lasersFiredOpponentAccuracy = agent.LaserHitOpponents / (double)agent.LasersFired;
                    lasersFiredOpponentAccuracy = (double)(Math.Round(PlanetConquest2Fitness.FitnessValues[PlanetConquest2Fitness.FitnessKeys.LaserOpponentAccuracy.ToString()] * lasersFiredOpponentAccuracy, 4) / Agents.Length);
                    agent.AgentFitness.UpdateFitness((float)lasersFiredOpponentAccuracy, PlanetConquest2Fitness.FitnessKeys.LaserOpponentAccuracy.ToString());
                }

                // LaserOpponentBaseLaserAccuracy
                if (agent.LasersFired > 0)
                {
                    lasersFiredBaseAccuracy = agent.LaserHitBases / (double)agent.LasersFired;
                    lasersFiredBaseAccuracy = (double)(Math.Round(PlanetConquest2Fitness.FitnessValues[PlanetConquest2Fitness.FitnessKeys.LaserOpponentBaseLaserAccuracy.ToString()] * lasersFiredBaseAccuracy, 4) / Agents.Length);
                    agent.AgentFitness.UpdateFitness((float)lasersFiredBaseAccuracy, PlanetConquest2Fitness.FitnessKeys.LaserOpponentBaseLaserAccuracy.ToString());
                }

                // OpponentDestroyedBonus
                numOfOpponents = Agents.Where(a => a.TeamIdentifier.TeamID != agent.TeamIdentifier.TeamID).Select(a => (a as PlanetConquest2AgentComponent).NumOfSpawns).Sum();
                if (numOfOpponents > 0)
                {
                    opponentsDestroyedBonus = agent.OpponentsDestroyed / (double)numOfOpponents;
                    opponentsDestroyedBonus = (double)Math.Round(PlanetConquest2Fitness.FitnessValues[PlanetConquest2Fitness.FitnessKeys.OpponentDestroyedBonus.ToString()] * opponentsDestroyedBonus, 4);
                    agent.AgentFitness.UpdateFitness((float)opponentsDestroyedBonus, PlanetConquest2Fitness.FitnessKeys.OpponentDestroyedBonus.ToString());
                }

                // OpponentBaseDestroyedBonus
                numOfOpponentBases = Match.Teams.Length - 1;
                if (numOfOpponentBases > 0)
                {
                    opponentBasesDestroyedBonus = agent.OpponentBasesDestroyed / (double)numOfOpponentBases;
                    opponentBasesDestroyedBonus = (double)Math.Round(PlanetConquest2Fitness.FitnessValues[PlanetConquest2Fitness.FitnessKeys.OpponentBaseDestroyedBonus.ToString()] * opponentBasesDestroyedBonus, 4);
                    agent.AgentFitness.UpdateFitness((float)opponentBasesDestroyedBonus, PlanetConquest2Fitness.FitnessKeys.OpponentBaseDestroyedBonus.ToString());
                }

                // DamageTakenPenalty
                numOfFiredOpponentMissiles = Agents.Where(a => a.TeamIdentifier.TeamID != agent.TeamIdentifier.TeamID).Select(a => (a as PlanetConquest2AgentComponent).LasersFired).Sum();
                if (numOfFiredOpponentMissiles > 0)
                {
                    damageTakenPenalty = agent.HitByOpponentLasers / (double)numOfFiredOpponentMissiles;
                    damageTakenPenalty = (double)(Math.Round(PlanetConquest2Fitness.FitnessValues[PlanetConquest2Fitness.FitnessKeys.DamageTakenPenalty.ToString()] * damageTakenPenalty, 4) / Agents.Length);
                    agent.AgentFitness.UpdateFitness((float)damageTakenPenalty, PlanetConquest2Fitness.FitnessKeys.DamageTakenPenalty.ToString());
                }

                // LavaPlanetOrbitEnter
                numOfAllLavaPlanetOrbitEnters = Agents.Select(a => (a as PlanetConquest2AgentComponent).EnteredLavaPlanetOrbit).Sum();
                if (numOfAllLavaPlanetOrbitEnters > 0)
                {
                    lavaPlanetOrbitEnters = agent.EnteredLavaPlanetOrbit / (double)numOfAllLavaPlanetOrbitEnters;
                    lavaPlanetOrbitEnters = (double)(Math.Round(PlanetConquest2Fitness.FitnessValues[PlanetConquest2Fitness.FitnessKeys.LavaPlanetOrbitEnter.ToString()] * lavaPlanetOrbitEnters, 4) / Agents.Length);
                    agent.AgentFitness.UpdateFitness((float)lavaPlanetOrbitEnters, PlanetConquest2Fitness.FitnessKeys.LavaPlanetOrbitEnter.ToString());
                }

                // IcePlanetOrbitEnter
                numOfAllIcePlanetOrbitEnters = Agents.Select(a => (a as PlanetConquest2AgentComponent).EnteredIcePlanetOrbit).Sum();
                if (numOfAllIcePlanetOrbitEnters > 0)
                {
                    icePlanetOrbitEnters = agent.EnteredIcePlanetOrbit / (double)numOfAllIcePlanetOrbitEnters;
                    icePlanetOrbitEnters = (double)(Math.Round(PlanetConquest2Fitness.FitnessValues[PlanetConquest2Fitness.FitnessKeys.IcePlanetOrbitEnter.ToString()] * icePlanetOrbitEnters, 4) / Agents.Length);
                    agent.AgentFitness.UpdateFitness((float)icePlanetOrbitEnters, PlanetConquest2Fitness.FitnessKeys.IcePlanetOrbitEnter.ToString());
                }

                // LavaPlanetOrbitCapture
                if (numOfAllLavaPlanets > 0)
                {
                    lavaPlanetOrbitCaptures = agent.CapturedLavaPlanet / (double)numOfAllLavaPlanets;
                    lavaPlanetOrbitCaptures = (double)Math.Round(PlanetConquest2Fitness.FitnessValues[PlanetConquest2Fitness.FitnessKeys.LavaPlanetOrbitCapture.ToString()] * lavaPlanetOrbitCaptures, 4);
                    agent.AgentFitness.UpdateFitness((float)lavaPlanetOrbitCaptures, PlanetConquest2Fitness.FitnessKeys.LavaPlanetOrbitCapture.ToString());
                }

                // IcePlanetOrbitCapture
                if (numOfAllIcePlanets > 0)
                {
                    icePlanetOrbitCaptures = agent.CapturedIcePlanet / (double)numOfAllIcePlanets;
                    icePlanetOrbitCaptures = (double)Math.Round(PlanetConquest2Fitness.FitnessValues[PlanetConquest2Fitness.FitnessKeys.IcePlanetOrbitCapture.ToString()] * icePlanetOrbitCaptures, 4);
                    agent.AgentFitness.UpdateFitness((float)icePlanetOrbitCaptures, PlanetConquest2Fitness.FitnessKeys.IcePlanetOrbitCapture.ToString());
                }

                Debug.Log("========================================");
                Debug.Log("Agent: Team ID" + agent.TeamIdentifier.TeamID + ", ID: " + agent.IndividualID);
                Debug.Log("Sectors explored: " + agent.SectorsExplored + " / " + Sectors.Length + " = " + sectorExplorationFitness);
                Debug.Log("Survival bonus: " + agent.MaxSurvivalTime + " / " + CurrentSimulationSteps + " = " + survivalBonus);
                Debug.Log("Lasers fired: " + agent.LasersFired + " / " + allPossibleLasersFired + " = " + lasersFired);
                Debug.Log("Laser fired opponent accuracy: " + agent.LaserHitOpponents + " / " + agent.LasersFired + " = " + lasersFiredOpponentAccuracy);
                Debug.Log("Laser fired base accuracy: " + agent.LaserHitBases + " / " + agent.LasersFired + " = " + opponentsDestroyedBonus);
                Debug.Log("Opponents destroyed bonus: " + agent.OpponentsDestroyed + " / " + numOfOpponents + " = " + opponentsDestroyedBonus);
                Debug.Log("Opponent bases destroyed bonus: " + agent.OpponentBasesDestroyed + " / " + numOfOpponentBases + " = " + opponentBasesDestroyedBonus);
                Debug.Log("Damage taken penalty: " + agent.HitByOpponentLasers + " / " + numOfFiredOpponentMissiles + " = " + damageTakenPenalty);
                Debug.Log("Lava planet captures: " + agent.CapturedLavaPlanet + " / " + numOfAllLavaPlanets + " = " + lavaPlanetOrbitCaptures);
                Debug.Log("Ice planet captures: " + agent.CapturedIcePlanet + " / " + numOfAllIcePlanets + " = " + icePlanetOrbitCaptures);
                Debug.Log("========================================");
            }
        }

        public void ReadParamsFromMainConfiguration()
        {
            if (MenuManager.Instance != null && MenuManager.Instance.MainConfiguration != null)
            {
                MainConfiguration conf = MenuManager.Instance.MainConfiguration;

                PlanetConquest2Fitness.FitnessValues = conf.FitnessValues;

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

                if (conf.ProblemConfiguration.ContainsKey("MaxNumOfAgents"))
                {
                    MaxNumOfAgents = int.Parse(conf.ProblemConfiguration["MaxNumOfAgents"]);
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
