using Base;
using Configuration;
using Problems.Robostrike;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        // Sectors
        private SectorComponent[] Sectors;

        CollectorTargetSpawner TargetSpawner;

        private List<TargetComponent> Targets;

        public int SpawnedTargets { get; set; }

        // Fitness calculation
        private float sectorExplorationFitness;
        private float TargetsAcquiredFitness;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();

            TargetSpawner = GetComponent<CollectorTargetSpawner>();
            if (TargetSpawner == null)
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
                Sectors = Environment.GetComponentsInChildren<SectorComponent>();
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
            SpawnedTargets += Targets.Count;
        }

        public void TargetAcquired(TargetComponent acquiredTarget)
        {
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
                SpawnedTargets++;
            }

            Targets.Remove(acquiredTarget);
            Destroy(acquiredTarget.gameObject);
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

        protected override void OnPreFinishGame()
        {
            SetAgentsFitness();
        }

        private void SetAgentsFitness()
        {
            foreach (CollectorAgentComponent agent in Agents)
            {
                // SectorExploration
                sectorExplorationFitness = agent.SectorsExplored / (float)Sectors.Length;
                sectorExplorationFitness = (float)Math.Round(CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.SectorExploration.ToString()] * sectorExplorationFitness, 4);
                agent.AgentFitness.UpdateFitness(sectorExplorationFitness, CollectorFitness.FitnessKeys.SectorExploration.ToString());

             
                // TargetsAcquired
                TargetsAcquiredFitness = agent.TargetsAcquired / (float)SpawnedTargets;
                TargetsAcquiredFitness = (float)Math.Round(CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.TargetsAcquired.ToString()] * TargetsAcquiredFitness, 4);
                agent.AgentFitness.UpdateFitness(TargetsAcquiredFitness, CollectorFitness.FitnessKeys.TargetsAcquired.ToString());


                Debug.Log("========================================");
                Debug.Log("Agent: Team ID" + agent.TeamIdentifier.TeamID + ", ID: " + agent.IndividualID);
                Debug.Log("Sectors explored: " + agent.SectorsExplored + " / " + Sectors.Length + " = " + sectorExplorationFitness);
                Debug.Log("Targets acquired: " + agent.TargetsAcquired + " / " + SpawnedTargets + " = " + TargetsAcquiredFitness);
                Debug.Log("========================================");
            }
        }
    }

    public enum CollectorGameMode
    {
        SingleTargetPickup,
        InfiniteTargetPickup
    }
}