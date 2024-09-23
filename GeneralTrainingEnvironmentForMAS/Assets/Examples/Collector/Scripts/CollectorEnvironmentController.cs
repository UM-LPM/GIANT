using TheKiwiCoder;
using UnityEngine;

namespace Collector
{

    public enum CollectorGameMode
    {
        SingleTargetPickup,
        InfiniteTargetPickup
    }
    public class CollectorEnvironmentController : EnvironmentControllerBase
    {

        [Header("Collector target configuration")]
        [SerializeField] GameObject TargetPrefab;
        [SerializeField] float TargetMinDistanceFromAgents = 3f;
        [SerializeField] float TargetExtends = 0.245f;
        [Range(1, 30)]
        [SerializeField] int StartNumberOfTargets = 1;
        [Range(1f, 30f)]
        [SerializeField] float TargetToTargetDistance = 1f;

        [Header("Collector configuration Movement")]
        [SerializeField] float AgentMoveSpeed = 5f;
        [SerializeField] float AgentRotationSpeed = 80f;

        [Header("Collector configuration Agent Move Fitness")]
        [SerializeField] bool CheckOnlyLastKnownPosition = true;
        [SerializeField] float AgentMoveFitnessUpdateInterval = 4f;
        [SerializeField] float AgentMoveFitnessMinDistance = 3f;

        [Header("Collector configuration Collector Game Mode")]
        [SerializeField] CollectorGameMode GameMode = CollectorGameMode.SingleTargetPickup;

        float ForwardSpeed = 1f;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();
        }

        protected override void DefineAdditionalDataOnPostStart()
        {
            foreach (CollectorAgentComponent agent in Agents)
            {
                agent.Rigidbody = agent.GetComponent<Rigidbody>();
            }

            foreach (CollectorAgentComponent agent in AgentsPredefinedBehaviour)
            {
                agent.Rigidbody = agent.GetComponent<Rigidbody>();
            }

            // Spawn target
            SpawnTargets();

            // Register event for Ray sensor
            RayHitObject.OnTargetHit += RayHitObject_OnTargetHit;
        }

        public override void UpdateAgents(bool updateBTs)
        {
            if (ManualAgentControl)
            {
                MoveAgentsWithController2(Agents);
            }
            else
            {
                UpdateAgentsWithBTs(Agents, updateBTs);
            }

            if (ManualAgentPredefinedBehaviourControl)
            {
                MoveAgentsWithController2(AgentsPredefinedBehaviour);
            }
            else
            {
                UpdateAgentsWithBTs(AgentsPredefinedBehaviour, updateBTs);
            }

            // Time penalty
            //AddTimePenaltyToAgents(Agents);
            //AddTimePenaltyToAgents(AgentsPredefinedBehaviour);
        }

        void ReadParamsFromMainConfiguration()
        {
            if (MenuManager.Instance != null && MenuManager.Instance.MainConfiguration != null)
            {
                MainConfiguration conf = MenuManager.Instance.MainConfiguration;

                CollectorFitness.FitnessValues = conf.FitnessValues;

                if(conf.ProblemConfiguration.ContainsKey("GameMode"))
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

                if (conf.ProblemConfiguration.ContainsKey("DecisionRequestInterval"))
                {
                    DecisionRequestInterval = int.Parse(conf.ProblemConfiguration["DecisionRequestInterval"]);
                }

            }
        }

        void UpdateAgentsWithBTs(AgentComponent[] agents, bool updateBTs)
        {
            foreach (CollectorAgentComponent agent in agents)
            {
                ActionBuffer actionBuffer;
                if (agent.gameObject.activeSelf)
                {
                    if (updateBTs)
                    {
                        actionBuffer = new ActionBuffer(null, new int[] { 0, 0, 0 }); // Forward, Side, Rotate

                        agent.BehaviourTree.UpdateTree(actionBuffer);
                        agent.ActionBuffer = actionBuffer;
                    }
                    else
                    {
                        actionBuffer = agent.ActionBuffer;
                    }
                    MoveAgent(agent, actionBuffer);
                }
            }

        }

        void MoveAgent(CollectorAgentComponent agent, ActionBuffer actionBuffer)
        {
            var dirToGo = Vector3.zero;
            var rotateDir = Vector3.zero;

            var forwardAxis = actionBuffer.DiscreteActions[0];
            var rightAxis = actionBuffer.DiscreteActions[1];
            var rotateAxis = actionBuffer.DiscreteActions[2];

            switch (forwardAxis)
            {
                case 1:
                    dirToGo = agent.transform.forward * ForwardSpeed;
                    break;
                case 2:
                    dirToGo = agent.transform.forward * -ForwardSpeed;
                    break;
            }

            switch (rotateAxis)
            {
                case 1:
                    rotateDir = agent.transform.up * -1f;
                    break;
                case 2:
                    rotateDir = agent.transform.up * 1f;
                    break;
            }

            // Movement Version 1 
            /*agent.transform.Translate(dirToGo * Time.fixedDeltaTime * AgentMoveSpeed);
            agent.transform.Rotate(rotateDir, Time.fixedDeltaTime * AgentRotationSpeed);*/

            // Movement Version 2
            agent.Rigidbody.MovePosition(agent.Rigidbody.position + (dirToGo * AgentMoveSpeed * Time.fixedDeltaTime));
            Quaternion turnRotation = Quaternion.Euler(0.0f, rotateDir.y * Time.fixedDeltaTime * AgentRotationSpeed, 0.0f);
            agent.Rigidbody.MoveRotation(agent.Rigidbody.rotation * turnRotation);
        }

        void MoveAgentsWithController2(AgentComponent[] agents)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            foreach (CollectorAgentComponent agent in agents)
            {
                if (agent.gameObject.activeSelf)
                {
                    Vector3 moveDirection = agent.transform.forward * verticalInput * AgentMoveSpeed * Time.fixedDeltaTime;
                    agent.Rigidbody.MovePosition(agent.Rigidbody.position + moveDirection);

                    float rotation = horizontalInput * AgentRotationSpeed * Time.fixedDeltaTime;
                    Quaternion turnRotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                    agent.Rigidbody.MoveRotation(agent.Rigidbody.rotation * turnRotation);
                }
            }
        }

        void SpawnTargets()
        {
            for (int i = 0; i < StartNumberOfTargets; i++)
            {
                SpawnTarget();
            }
        }

        void SpawnTarget()
        {
            Vector3 spawnPos;
            Quaternion rotation;

            bool isFarEnough;
            do
            {
                isFarEnough = true;
                spawnPos = GetRandomSpawnPointInRadius(ArenaRadius, ArenaOffset);
                if (SceneLoadMode == SceneLoadMode.GridMode)
                    spawnPos += GridCell.GridCellPosition;

                rotation = GetRandomRotation();

                if (!TargetSpawnPointSuitable(spawnPos, rotation))
                {
                    isFarEnough = false;
                }

                // Check if current spawn point is far enough from the agents
                foreach (CollectorAgentComponent agent in Agents)
                {
                    if (Vector3.Distance(agent.transform.position, spawnPos) < TargetMinDistanceFromAgents)
                    {
                        isFarEnough = false;
                        break;
                    }
                }

                foreach (CollectorAgentComponent agent in AgentsPredefinedBehaviour)
                {
                    if (Vector3.Distance(agent.transform.position, spawnPos) < TargetMinDistanceFromAgents)
                    {
                        isFarEnough = false;
                        break;
                    }
                }

                // Check if current spawn point is far enough from other targets
                foreach (TargetComponent target in GetComponentsInChildren<TargetComponent>())
                {
                    if (Vector3.Distance(target.transform.position, spawnPos) < TargetToTargetDistance)
                    {
                        isFarEnough = false;
                        break;
                    }
                }

            } while (!isFarEnough);


            GameObject obj = Instantiate(TargetPrefab, spawnPos, rotation, gameObject.transform);
            obj.layer = gameObject.layer;
            obj.GetComponent<TargetComponent>().CollectorEnvironmentController = this;
        }

        public void TargetAquired(AgentComponent agent)
        {
            // Update agent fitness
            agent.AgentFitness.Fitness.UpdateFitness(CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.AgentPickedTarget.ToString()], CollectorFitness.FitnessKeys.AgentPickedTarget.ToString());
            agent.GetComponent<CollectorAgentComponent>().TargetsAquired++;

            if (GameMode == CollectorGameMode.SingleTargetPickup)
            {
                FinishGame(); // Finish game when target is aquired
            }
            else if (GameMode == CollectorGameMode.InfiniteTargetPickup)
            {
                SpawnTarget();
            }

            // Reset last known positions
            agent.LastKnownPositions.Clear();
            agent.GetComponent<CollectorAgentComponent>().NearTarget = false;
            agent.GetComponent<CollectorAgentComponent>().HitObjects.Clear();
        }

        public virtual bool TargetSpawnPointSuitable(Vector3 newSpawnPos, Quaternion newRotation)
        {
            Collider[] colliders = Physics.OverlapBox(newSpawnPos, Vector3.one * TargetExtends, newRotation, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)) + DefaultLayer);
            if (colliders.Length > 0)
            {
                return false;
            }

            return true;
        }

        void AddTimePenaltyToAgents(AgentComponent[] agents)
        {
            foreach (CollectorAgentComponent agent in agents)
            {
                agent.AgentFitness.Fitness.UpdateFitness(CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.TimePassedPenalty.ToString()], CollectorFitness.FitnessKeys.TimePassedPenalty.ToString());
            }
        }

        private void RayHitObject_OnTargetHit(object sender, OnTargetHitEventargs e)
        {
            if (e.TargetGameObject.GetComponent<TargetComponent>() != null)
            {
                // Update agent fitness
                e.Agent.AgentFitness.Fitness.UpdateFitness(CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.AgentSpottedTarget.ToString()], CollectorFitness.FitnessKeys.AgentSpottedTarget.ToString());
            }
        }

        private void OnDestroy()
        {
            RayHitObject.OnTargetHit -= RayHitObject_OnTargetHit;
        }

        protected override void OnPreFinishGame()
        {
            UpdateAgentFitnessBTNodePenalty(Agents);
            UpdateAgentFitnessBTNodePenalty(AgentsPredefinedBehaviour);
            UpdateAgentFitnessRayHitObject1(Agents);
            UpdateAgentFitnessRayHitObject1(AgentsPredefinedBehaviour);
        }

        /// <summary>
        /// Update the fitness of each tree for each RayHitObject node where target game object is object 1
        /// </summary>
        private void UpdateAgentFitnessRayHitObject1(AgentComponent[] agents)
        { 
            foreach (CollectorAgentComponent agent in agents)
            {
                // Loop through all the nodes in the behaviour tree
                foreach (Node node in agent.BehaviourTree.nodes)
                {
                    if (node is RayHitObject)
                    {
                        RayHitObject rayHitObject = (RayHitObject)node;
                        if (rayHitObject.targetGameObject == TargetGameObject.Object1)
                        {
                            // Update the fitness of the tree
                            agent.AgentFitness.Fitness.UpdateFitness(CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.AgentsBtContainsMainObject.ToString()], CollectorFitness.FitnessKeys.AgentsBtContainsMainObject.ToString());
                        }
                    }
                }
            }
        }

        private void UpdateAgentFitnessBTNodePenalty(AgentComponent[] agents)
        {
            foreach (CollectorAgentComponent agent in agents)
            {
                float fitnessPenalty = agent.BehaviourTree.nodes.Count * CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.AgentBTNodePenalty.ToString()];
                agent.AgentFitness.Fitness.UpdateFitness(fitnessPenalty, CollectorFitness.FitnessKeys.AgentBTNodePenalty.ToString());
            }
        }
    }
}