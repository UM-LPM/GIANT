using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public enum CollectorGameMode {
    SingleTargetPickup,
    InfiniteTargetPickup
}
public class CollectorEnvironmentController : EnvironmentControllerBase {

    [Header("Collector target configuration")]
    [SerializeField] GameObject TargetPrefab;
    [SerializeField] float TargetMinDistanceFromAgents = 3f;
    [SerializeField] float TargetExtends = 0.245f;

    [Header("Collector configuration Movement")]
    [SerializeField] float AgentMoveSpeed = 5f;
    [SerializeField] float AgentRotationSpeed = 80f;

    [Header("Collector configuration Agent Move Fitness")]
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

    protected override void DefineAdditionalDataOnStart() {
        foreach (CollectorAgentComponent agent in Agents) {
            agent.Rigidbody = agent.GetComponent<Rigidbody>();
        }

        foreach (CollectorAgentComponent agent in AgentsPredefinedBehaviour) {
            agent.Rigidbody = agent.GetComponent<Rigidbody>();
        }

        // Spawn target
        SpawnTarget();
    }

    public override void UpdateAgents() {
        if (ManualAgentControl) {
            MoveAgentsWithController2(Agents);
        }
        else {
            UpdateAgentsWithBTs(Agents);
        }

        if (ManualAgentPredefinedBehaviourControl) {
            MoveAgentsWithController2(AgentsPredefinedBehaviour);
        }
        else {
            UpdateAgentsWithBTs(AgentsPredefinedBehaviour);
        }

        // Update agent move fitness
        if (CurrentSimulationTime >= NextAgentMoveFitnessUpdate) {
            UpdateAgentMoveFitness(Agents);
            UpdateAgentMoveFitness(AgentsPredefinedBehaviour);
            NextAgentMoveFitnessUpdate += AgentMoveFitnessUpdateInterval;
        }

        // Update agent near wall fitness
        if (CurrentSimulationTime >= NextAgentNearWallFitnessUpdate) {
            UpdateAgentNearWallFitness(Agents);
            UpdateAgentNearWallFitness(AgentsPredefinedBehaviour);
            NextAgentNearWallFitnessUpdate += AgentNearWallUpdateInterval;
        }


        // Update agent near wall fitness
        if (CurrentSimulationTime >= NextAgentNearTargetFitnessUpdate) {
            UpdateAgentNearTargetFitness(Agents);
            UpdateAgentNearTargetFitness(AgentsPredefinedBehaviour);
            NextAgentNearTargetFitnessUpdate += AgentNearTargetUpdateInterval;
        }

        // Time penalty
        AddTimePenaltyToAgents(Agents);
        AddTimePenaltyToAgents(AgentsPredefinedBehaviour);
    }

    void UpdateAgentsWithBTs(AgentComponent[] agents) {
        ActionBuffer actionBuffer;
        foreach (CollectorAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf) {
                actionBuffer = new ActionBuffer(null, new int[] { 0, 0, 0}); // Forward, Side, Rotate

                agent.BehaviourTree.UpdateTree(actionBuffer);
                MoveAgent(agent, actionBuffer);
            }
        }

    }

    void MoveAgent(CollectorAgentComponent agent, ActionBuffer actionBuffer) {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var forwardAxis = actionBuffer.DiscreteActions[0];
        var rightAxis = actionBuffer.DiscreteActions[1];
        var rotateAxis = actionBuffer.DiscreteActions[2];

        switch (forwardAxis) {
            case 1:
                dirToGo = agent.transform.forward * ForwardSpeed;
                break;
            case 2:
                dirToGo = agent.transform.forward * -ForwardSpeed;
                break;
        }

        switch (rotateAxis) {
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

    void MoveAgentsWithController2(AgentComponent[] agents) {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        foreach (CollectorAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf) {
                Vector3 moveDirection = agent.transform.forward * verticalInput * AgentMoveSpeed * Time.fixedDeltaTime;
                agent.Rigidbody.MovePosition(agent.Rigidbody.position + moveDirection);

                float rotation = horizontalInput * AgentRotationSpeed * Time.fixedDeltaTime;
                Quaternion turnRotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                agent.Rigidbody.MoveRotation(agent.Rigidbody.rotation * turnRotation);
            }
        }
    }

    void SpawnTarget() {
        Vector3 spawnPos;
        Quaternion rotation;

        bool isFarEnough;
        do {
            isFarEnough = true;
            spawnPos = GetRandomSpawnPointInRadius(ArenaRadius, ArenaOffset);
            if (SceneLoadMode == SceneLoadMode.GridMode)
                spawnPos += GridCell.GridCellPosition;

            rotation = GetAgentRandomRotation();

            if(!TargetSpawnPointSuitable(spawnPos, rotation)) {
                isFarEnough = false;
            }

            // Check if current spawn point is far enough from the agents
            foreach (CollectorAgentComponent agent in Agents) {
                if (Vector3.Distance(agent.transform.position, spawnPos) < TargetMinDistanceFromAgents) {
                    isFarEnough = false;
                    break;
                }
            }

            foreach (CollectorAgentComponent agent in AgentsPredefinedBehaviour) {
                if (Vector3.Distance(agent.transform.position, spawnPos) < TargetMinDistanceFromAgents) {
                    isFarEnough = false;
                    break;
                }
            }
        } while (!isFarEnough);


        GameObject obj = Instantiate(TargetPrefab, spawnPos, rotation, gameObject.transform);
        obj.layer = gameObject.layer;
        obj.GetComponent<TargetComponent>().CollectorEnvironmentController = this;
    }

    public void TargetAquired(TargetComponent target, AgentComponent agent) {
        // Update agent fitness
        agent.AgentFitness.Fitness.UpdateFitness(CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.AgentPickedTarget.ToString()], CollectorFitness.FitnessKeys.AgentPickedTarget.ToString());
        agent.GetComponent<CollectorAgentComponent>().TargetsAquired++;

        if(GameMode == CollectorGameMode.SingleTargetPickup) {
            FinishGame(); // Finish game when target is aquired
        }
        else if (GameMode == CollectorGameMode.InfiniteTargetPickup) {
            SpawnTarget();
        }
    }

    void UpdateAgentMoveFitness(AgentComponent[] agents) {
        foreach (CollectorAgentComponent agent in agents) {
            // Only update agents that are active
            if (agent.gameObject.activeSelf) {
                if (agent.LastKnownPosition != Vector3.zero) {
                    float distance = Vector3.Distance(agent.LastKnownPosition, agent.transform.position);
                    if (distance >= AgentMoveFitnessMinDistance) {
                        agent.AgentFitness.Fitness.UpdateFitness(CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.AgentMovedBonus.ToString()], CollectorFitness.FitnessKeys.AgentMovedBonus.ToString());
                    }
                }
                agent.LastKnownPosition = agent.transform.position;
            }
        }

    }

    public virtual bool TargetSpawnPointSuitable(Vector3 newSpawnPos, Quaternion newRotation) {
        Collider[] colliders = Physics.OverlapBox(newSpawnPos, Vector3.one * TargetExtends, newRotation, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)) + DefaultLayer);
        if (colliders.Length > 0) {
            return false;
        }

        return true;
    }

    void UpdateAgentNearWallFitness(AgentComponent[] agents) {
        foreach (CollectorAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf) {
                Collider agentCol = agent.GetComponent<Collider>();

                Collider[] colliders = Physics.OverlapBox(agent.transform.position, Vector3.one * AgentNearWallExtends, agent.transform.rotation, LayerMask.GetMask(LayerMask.LayerToName(agent.gameObject.layer)) + DefaultLayer);
                foreach (Collider col in colliders) {
                    if (col.gameObject.tag.Contains("Wall") || col.gameObject.tag.Contains("Obstacle")) {
                        agent.AgentFitness.Fitness.UpdateFitness(CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.AgentNearWall.ToString()], CollectorFitness.FitnessKeys.AgentNearWall.ToString());
                    }
                }
            }
        }
    }

    void UpdateAgentNearTargetFitness(AgentComponent[] agents) {
        foreach (CollectorAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf) {
                Collider agentCol = agent.GetComponent<Collider>();

                Collider[] colliders = Physics.OverlapBox(agent.transform.position, Vector3.one * AgentNearTargetExtends, agent.transform.rotation, LayerMask.GetMask(LayerMask.LayerToName(agent.gameObject.layer)) + DefaultLayer);
                foreach (Collider col in colliders) {
                    if (col.gameObject.tag.Contains("Object1")) {
                        agent.AgentFitness.Fitness.UpdateFitness(CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.AgentNearWall.ToString()], CollectorFitness.FitnessKeys.AgentNearWall.ToString());
                    }
                }
            }
        }
    }

    void AddTimePenaltyToAgents(AgentComponent[] agents) {
        foreach (CollectorAgentComponent agent in agents) {
            agent.AgentFitness.Fitness.UpdateFitness(CollectorFitness.FitnessValues[CollectorFitness.FitnessKeys.TimePassedPenalty.ToString()], CollectorFitness.FitnessKeys.TimePassedPenalty.ToString());
        }
    }

}
