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
        [Range(0, 30)]
        [SerializeField] public int StartNumberOfTargets = 1;
        [Range(1f, 30f)]
        [SerializeField] public float TargetToTargetDistance = 5f;
        [SerializeField] public Vector3 TargetColliderExtendsMultiplier = new Vector3(0.50f, 0.495f, 0.505f);

        CollectorTargetSpawner TargetSpawner;

        private List<TargetComponent> Targets;
        
        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();

            TargetSpawner = GetComponent<CollectorTargetSpawner>();
            if(TargetSpawner == null)
            {
                throw new Exception("TargetSpawner is not defined");
                // TODO Add error reporting here
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
                    // 1. Update Agent fitnss
                    agent.AgentFitness.UpdateFitness(CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.AgentPickedTarget.ToString()], CollectorFitness.FitnessKeys.AgentPickedTarget.ToString());
                    agent.GetComponent<CollectorAgentComponent>().TargetsAquired++;

                    if (GameMode == CollectorGameMode.SingleTargetPickup)
                    {
                        FinishGame(); // Finish game when target is aquired
                    }
                    else if (GameMode == CollectorGameMode.InfiniteTargetPickup)
                    {
                        // Respawn target
                        List<Vector3> targetPositions = new List<Vector3>();
                        foreach (TargetComponent target in Targets)
                        {
                            targetPositions.Add(target.transform.position);
                        }

                        Targets.Add(TargetSpawner.SpawnTarget<TargetComponent>(this, targetPositions));
                    }

                    // Reset last known positions
                    agent.LastKnownSectorPositions.Clear();

                    Targets.Remove(component);
                    Destroy(component.gameObject);
                }
            }
        }

        void CheckAgentSectorOverlaps()
        {
            foreach (CollectorAgentComponent agent in Agents)
            {
                List<SectorComponent> components = PhysicsUtil.PhysicsOverlapTargetObjects<SectorComponent>(GameType, agent.gameObject, agent.transform.position, 0, AgentColliderExtendsMultiplier, agent.transform.rotation, PhysicsOverlapType.OverlapBox, false, gameObject.layer, DefaultLayer);
                if (components != null && components.Count > 0)
                {
                    foreach(var sectorComponent in components)
                    {
                        if (AgentExploredNewSector(agent, sectorComponent))
                        {
                            //Debug.Log("New Sector Explored"); // TODO Remove
                            if (CollectorFitness.FitnessValues[CollectorFitness.Keys[(int)CollectorFitness.FitnessKeys.AgentExploredSector]] != 0)
                            {
                                agent.AgentFitness.UpdateFitness((CollectorFitness.FitnessValues[CollectorFitness.Keys[(int)CollectorFitness.FitnessKeys.AgentExploredSector]]), CollectorFitness.FitnessKeys.AgentExploredSector.ToString());
                            }

                            // Add explored sector to the list of explored Sectors
                            agent.LastKnownSectorPositions.Add(sectorComponent.transform.position);
                            return;
                        }
                        // Re-explored Sector
                        else if (!AgentExploredNewSector(agent, sectorComponent))
                        {
                            if (CollectorFitness.FitnessValues[CollectorFitness.Keys[(int)CollectorFitness.FitnessKeys.AgentReExploredSector]] != 0)
                            {
                                agent.AgentFitness.UpdateFitness((CollectorFitness.FitnessValues[CollectorFitness.Keys[(int)CollectorFitness.FitnessKeys.AgentReExploredSector]]), CollectorFitness.FitnessKeys.AgentReExploredSector.ToString());
                            }
                        }
                    }
                }
            }
        }

        private bool AgentExploredNewSector(CollectorAgentComponent agentComponent, SectorComponent sectorComponent)
        {
            return !agentComponent.LastKnownSectorPositions.Contains(sectorComponent.transform.position);
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

            }
        }
    }

    public enum CollectorGameMode
    {
        SingleTargetPickup,
        InfiniteTargetPickup
    }
}