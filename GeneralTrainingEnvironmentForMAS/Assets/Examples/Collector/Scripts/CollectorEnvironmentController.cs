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

        [Header("Collector configuration Agent Near Wall Fitness")]
        [SerializeField] float AgentNearWallUpdateInterval = 4f;
        [SerializeField] float AgentNearWallExtends = 0.6f;

        [Header("Collector configuration Agent Near Target Fitness")]
        [SerializeField] float AgentNearTargetUpdateInterval = 4f;
        [SerializeField] float AgentNearTargetExtends = 1.5f;

        [Header("Collector configuration Collector Game Mode")]
        [SerializeField] CollectorGameMode GameMode = CollectorGameMode.SingleTargetPickup;

        float ForwardSpeed = 1f;
        float NextAgentMoveFitnessUpdate = 0;
        float NextAgentNearWallFitnessUpdate = 0;
        float NextAgentNearTargetFitnessUpdate = 0;

        protected override void DefineAdditionalDataOnAwake()
        {
            ReadParamsFromMainConfiguration();
        }

        protected override void DefineAdditionalDataOnStart()
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

            // Update agent near wall fitness
            /*if (CurrentSimulationTime >= NextAgentNearTargetFitnessUpdate)
            {
                UpdateAgentNearTargetFitness(Agents);
                UpdateAgentNearTargetFitness(AgentsPredefinedBehaviour);
                NextAgentNearTargetFitnessUpdate += AgentNearTargetUpdateInterval;
            }*/

            // Update agent move fitness
            /*if (CurrentSimulationTime >= NextAgentMoveFitnessUpdate)
            {
                if (CheckOnlyLastKnownPosition)
                {
                    UpdateAgentMoveFitness(Agents);
                    UpdateAgentMoveFitness(AgentsPredefinedBehaviour);
                }
                else
                {
                    CheckAgentGloboalMovement(Agents);
                    CheckAgentGloboalMovement(AgentsPredefinedBehaviour);

                }

                NextAgentMoveFitnessUpdate += AgentMoveFitnessUpdateInterval;
            }*/

            // Time penalty
            //AddTimePenaltyToAgents(Agents);
            //AddTimePenaltyToAgents(AgentsPredefinedBehaviour);
        }

        void ReadParamsFromMainConfiguration()
        {
            if (MenuManager.Instance != null && MenuManager.Instance.MainConfiguration != null)
            {
                MainConfiguration conf = MenuManager.Instance.MainConfiguration;
                GameMode = (CollectorGameMode)conf.GameMode;

                CollectorFitness.FitnessValues = conf.FitnessValues;

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

                rotation = GetAgentRandomRotation();

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

        void UpdateAgentMoveFitness(AgentComponent[] agents)
        {
            foreach (CollectorAgentComponent agent in agents)
            {
                // Only update agents that are active
                if (agent.gameObject.activeSelf)
                {
                    if (agent.LastKnownPositions != null && agent.LastKnownPositions.Count == 0)
                    {
                        float distance = Vector3.Distance(agent.LastKnownPositions[0], agent.transform.position);
                        if (distance <= AgentMoveFitnessMinDistance)
                        {
                            agent.AgentFitness.Fitness.UpdateFitness(CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.AgentNotMoved.ToString()], CollectorFitness.FitnessKeys.AgentNotMoved.ToString());
                        }
                    }
                    agent.LastKnownPositions[0] = agent.transform.position;
                }
            }

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

        /*void UpdateAgentNearWallFitness(AgentComponent[] agents) {
            foreach (CollectorAgentComponent agent in agents) {
                if (agent.gameObject.activeSelf) {
                    Collider agentCol = agent.GetComponent<Collider>();

                    Collider[] colliders = Physics.OverlapBox(agent.transform.position, Vector3.one * AgentNearWallExtends, agent.transform.rotation, LayerMask.GetMask(LayerMask.LayerToName(agent.gameObject.layer)) + DefaultLayer);
                    foreach (Collider col in colliders) {
                        if (col.gameObject.tag.Contains("Wall") || col.gameObject.tag.Contains("Obstacle")) {
                            agent.AgentFitness.Fitness.UpdateFitness(CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.AgentTouchedStaticObject.ToString()], CollectorFitness.FitnessKeys.AgentTouchedStaticObject.ToString());
                        }
                    }
                }
            }
        }*/

        /*void UpdateAgentNearTargetFitness(AgentComponent[] agents)
        {
            foreach (CollectorAgentComponent agent in agents)
            {
                if (agent.gameObject.activeSelf)
                {
                    Collider agentCol = agent.GetComponent<Collider>();

                    Collider[] colliders = Physics.OverlapBox(agent.transform.position, Vector3.one * AgentNearTargetExtends, agent.transform.rotation, LayerMask.GetMask(LayerMask.LayerToName(agent.gameObject.layer)) + DefaultLayer);
                    foreach (Collider col in colliders)
                    {
                        if (col.gameObject.tag.Contains("Object1"))
                        {
                            agent.AgentFitness.Fitness.UpdateFitness(CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.AgentNearTarget.ToString()], CollectorFitness.FitnessKeys.AgentNearTarget.ToString());
                        }
                    }
                }
            }
        }*/

        void AddTimePenaltyToAgents(AgentComponent[] agents)
        {
            foreach (CollectorAgentComponent agent in agents)
            {
                agent.AgentFitness.Fitness.UpdateFitness(CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.TimePassedPenalty.ToString()], CollectorFitness.FitnessKeys.TimePassedPenalty.ToString());
            }
        }

        public void CheckAgentGloboalMovement(AgentComponent[] agents)
        {
            foreach (CollectorAgentComponent agent in agents)
            {
                foreach (Vector3 pos in agent.LastKnownPositions)
                {
                    if (Vector3.Distance(pos, agent.transform.position) <= AgentMoveFitnessMinDistance)
                    {
                        agent.AgentFitness.Fitness.UpdateFitness(CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.AgentNotMoved.ToString()], CollectorFitness.FitnessKeys.AgentNotMoved.ToString());
                        break;
                    }
                }
                agent.LastKnownPositions.Add(agent.transform.position);
            }
        }

        private void RayHitObject_OnTargetHit(object sender, OnTargetHitEventargs e)
        {
            if (e.TargetGameObject.GetComponent<TargetComponent>() != null)
            {
                //UnityEngine.Debug.Log("AgentSpottedTarget"); // TODO Remove
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