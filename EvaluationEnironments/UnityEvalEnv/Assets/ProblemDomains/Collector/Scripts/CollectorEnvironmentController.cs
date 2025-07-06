using Base;
using Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace Problems.Collector
{
    [RequireComponent(typeof(CollectorTargetSpawner))]
    public class CollectorEnvironmentController : EnvironmentControllerBase
    {
        [Header("Collector Game Configuration")]
        [SerializeField] public CollectorGameMode GameMode = CollectorGameMode.SingleTargetPickup;

        [Header("Collector Movement Configuration")]
        [SerializeField] public float AgentMoveSpeed = 5f;
        [SerializeField] public float AgentRotationSpeed = 80f;
        [HideInInspector] public float ForwardSpeed = 1f;

        [Header("Collector Target configuration")]
        [SerializeField] public GameObject TargetPrefab;
        [SerializeField] public float TargetMinDistanceFromAgents = 3f;
        [SerializeField] public float TargetExtends = 0.245f;
        [SerializeField] public int MaxTargets = 30;
        [Range(0, 30)]
        [SerializeField] public int StartNumberOfTargets = 1;
        [Range(1f, 30f)]
        [SerializeField] public float TargetToTargetDistance = 5f;
        [SerializeField] public Vector3 TargetColliderExtendsMultiplier = new Vector3(0.50f, 0.495f, 0.505f);

        CollectorTargetSpawner TargetSpawner;

        private List<TargetComponent> Targets;

        // Sectors
        private SectorComponent[] Sectors;

        private CollectorAgentComponent agent;
        private Vector3 sectorPosition;

        // Fitness calculation
        private float sectorExplorationFitness;
        private float targetsAcquiredFitness;
        private float timePenalty;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();

            TargetSpawner = GetComponent<CollectorTargetSpawner>();
            if(TargetSpawner == null)
            {
                throw new Exception("TargetSpawner is not defined");
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
        }

        protected override void DefineAdditionalDataOnPostStart()
        {
            foreach (CollectorAgentComponent agent in Agents)
            {
                agent.Rigidbody = agent.GetComponent<Rigidbody>();
            }

            // Spawn target
            Targets = TargetSpawner.Spawn<TargetComponent>(this).ToList();
        }

        protected override void OnPostFixedUpdate()
        {
            // Check if Any agent overlaps the target
            CheckAgentTargetOverlaps();

            // Check if Any agent overlaps any Sectors
            CheckAgentSectorOverlaps();
        }

        void CheckAgentTargetOverlaps()
        {
            foreach (CollectorAgentComponent agent in Agents)
            {
                TargetComponent component = PhysicsUtil.PhysicsOverlapTargetObject<TargetComponent>(GameType, agent.gameObject, agent.transform.position, 0, AgentColliderExtendsMultiplier, agent.transform.rotation, PhysicsOverlapType.OverlapBox, false, gameObject.layer, DefaultLayer);
                if (component != null)
                {
                    agent.GetComponent<CollectorAgentComponent>().TargetsAquired++;

                    if (GameMode == CollectorGameMode.SingleTargetPickup)
                    {
                        FinishGame(); // Finish game when target is aquired
                    }
                    else if (GameMode == CollectorGameMode.InfiniteTargetPickup)
                    {
                        if(agent.TargetsAquired == MaxTargets)
                        {
                            FinishGame(); // Finish game when max targets are acquired
                        }

                        // Respawn target
                        List<Vector3> targetPositions = new List<Vector3>();
                        foreach (TargetComponent target in Targets)
                        {
                            targetPositions.Add(target.transform.position);
                        }

                        Targets.Add(TargetSpawner.SpawnTarget<TargetComponent>(this, targetPositions));
                    }

                    Targets.Remove(component);
                    Destroy(component.gameObject);
                }
            }
        }

        void CheckAgentSectorOverlaps()
        {
            // Exploration bonus
            for (int i = 0; i < Agents.Length; i++)
            {
                agent = Agents[i] as CollectorAgentComponent;
                if (agent.gameObject.activeSelf)
                {
                    foreach (SectorComponent sector in Sectors)
                    {
                        sectorPosition = sector.transform.position;
                        if (IsAgentInSector(agent.transform.position, sector.gameObject.GetComponent<BoxCollider>()))
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

        private bool IsAgentInSector(Vector3 agentPosition, BoxCollider colliderComponent)
        {
            if (colliderComponent.bounds.Contains(agentPosition))
            {
                return true;
            }

            return false;
        }

        protected override void OnPreFinishGame()
        {
            SetAgentsFitness();
        }

        private void SetAgentsFitness()
        {
            foreach (CollectorAgentComponent agent in Agents)
            {
                // Sector exploration
                sectorExplorationFitness = agent.SectorsExplored / (float)Sectors.Length;
                sectorExplorationFitness = (float)Math.Round(CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.SectorExploration.ToString()] * sectorExplorationFitness, 4);
                agent.AgentFitness.UpdateFitness(sectorExplorationFitness, CollectorFitness.FitnessKeys.SectorExploration.ToString());

                if (agent.TargetsAquired > 0)
                {
                    targetsAcquiredFitness = agent.TargetsAquired / (float)MaxTargets;
                    targetsAcquiredFitness = (float)Math.Round(CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.TargetsAcquired.ToString()] * targetsAcquiredFitness, 4);
                    agent.AgentFitness.UpdateFitness(targetsAcquiredFitness, CollectorFitness.FitnessKeys.TargetsAcquired.ToString());
                }

                // Time penalty
                timePenalty = CurrentSimulationSteps / SimulationSteps;
                timePenalty = (float)Math.Round(CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.TimePenalty.ToString()] * timePenalty, 4);
                agent.AgentFitness.UpdateFitness(timePenalty, CollectorFitness.FitnessKeys.TimePenalty.ToString());

                Debug.Log("========================================");
                Debug.Log("Agent: Team ID" + agent.TeamIdentifier.TeamID + ", ID: " + agent.IndividualID);
                Debug.Log("Sectors explored: " + agent.SectorsExplored + " / " + Sectors.Length + " =");
                Debug.Log("Targets Acquired: " + agent.TargetsAquired + " / " + MaxTargets + " =");
                Debug.Log("Time penalty: " + CurrentSimulationSteps + " / " + SimulationSteps + " =");
                Debug.Log("========================================");
                
            }
        }

        void ReadParamsFromMainConfiguration()
        {
            if (MenuManager.Instance != null && MenuManager.Instance.MainConfiguration != null)
            {
                MainConfiguration conf = MenuManager.Instance.MainConfiguration;

                CollectorFitness.FitnessValues = conf.FitnessValues;

                if (conf.ProblemConfiguration.ContainsKey("GameMode"))
                {
                    GameMode = (CollectorGameMode)int.Parse(conf.ProblemConfiguration["GameMode"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentMoveSpeed"))
                {
                    AgentMoveSpeed = float.Parse(conf.ProblemConfiguration["AgentMoveSpeed"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentRotationSpeed"))
                {
                    AgentRotationSpeed = float.Parse(conf.ProblemConfiguration["AgentRotationSpeed"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("StartNumberOfTargets"))
                {
                    StartNumberOfTargets = int.Parse(conf.ProblemConfiguration["StartNumberOfTargets"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("TargetToTargetMinDistance"))
                {
                    TargetToTargetDistance = int.Parse(conf.ProblemConfiguration["TargetToTargetMinDistance"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("TargetMinDistanceFromAgents"))
                {
                    TargetMinDistanceFromAgents = int.Parse(conf.ProblemConfiguration["TargetMinDistanceFromAgents"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("MaxTargets"))
                {
                    MaxTargets = int.Parse(conf.ProblemConfiguration["MaxTargets"]);
                }
            }
        }
    }

    public enum CollectorGameMode
    {
        SingleTargetPickup,
        InfiniteTargetPickup
    }
}