using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class RobocodeEnvironmentController : EnvironmentControllerBase {

    public static int MissileHealthDamage = 1;
    public Rigidbody Rigidbody { get; set; }

    [Header("Robocode configuration Missile")]
    [SerializeField, Tooltip("Destroy Missile After X seconds")] public float DestroyMissileAfter = 3.0f;
    [SerializeField] float MissileShootCooldown = 1.0f;
    [SerializeField] float MissleLaunchSpeed = 30f;

    [Header("Robocode configuration Movement")]
    [SerializeField] float ForwardSpeed = 1f;

    [Header("Robocode configuration Agent Move Fitness")]
    [SerializeField] float AgentMoveFitnessUpdateInterval = 5f;
    [SerializeField] float AgentMoveFitnessMinDistance = 2f;
    [Header("Robocode configuration Agent Aim Fitness")]
    [SerializeField] float AgentAimFitnessUpdateInterval = 2f;

    [Header("Robocode configuration General")]
    [SerializeField] GameObject MissilePrefab;
    [SerializeField] MissileController MissileController;
    [SerializeField] int AgentStartHealth = 10;
    [SerializeField] RobocodeGameScenarioType GameScenarioType = RobocodeGameScenarioType.Normal;

    [Header("User Input")]
    [SerializeField] KeyCode ShootKey = KeyCode.Space;
    [SerializeField] KeyCode TurretMovementLeft = KeyCode.Q;
    [SerializeField] KeyCode TurretMovementRight = KeyCode.E;

    float AgentMoveSpeed = 5f;
    float AgentRotationSpeed = 80f;
    float AgentTurrentRotationSpeed = 90f;

    bool IsPressingShoot;
    float TurretMoveDir;
    float NextAgentMoveFitnessUpdate = 0;
    float NextAgentAimFitnessUpdate = 0;

    protected override void DefineAdditionalDataOnStart() {
        foreach (RobocodeAgentComponent agent in Agents) {
            agent.Health =  AgentStartHealth;
            agent.Rigidbody = agent.GetComponent<Rigidbody>();
        }

        foreach (RobocodeAgentComponent agent in AgentsPredefinedBehaviour) {
            agent.Health = AgentStartHealth;
            agent.Rigidbody = agent.GetComponent<Rigidbody>();
        }
    }

    protected override void OnUpdate() {
        if(Input.GetKey(ShootKey))
            IsPressingShoot = true;
        else
            IsPressingShoot = false;

        if (Input.GetKey(TurretMovementLeft))
            TurretMoveDir = -1f;
        else if (Input.GetKey(TurretMovementRight))
            TurretMoveDir = 1f;
        else
            TurretMoveDir = 0f;
    }

    public override void UpdateAgents() {
        //MoveAgentsWithController1(Agents);

        if (ManualAgentControl) {
            MoveAgentsWithController2(Agents);
            if (IsPressingShoot)
                AgentsShoot(Agents);
            if (TurretMoveDir != 0) {
                AgentsMoveTurret(Agents);
            }
        }
        else {
            UpdateAgentsWithBTs(Agents);
        }

        if (ManualAgentPredefinedBehaviourControl) {
            MoveAgentsWithController2(AgentsPredefinedBehaviour);
            if (IsPressingShoot)
                AgentsShoot(AgentsPredefinedBehaviour);
            if (TurretMoveDir != 0) {
                AgentsMoveTurret(AgentsPredefinedBehaviour);
            }
        }
        else {
            UpdateAgentsWithBTs(AgentsPredefinedBehaviour);
        }

        // Update agent move fitness
        if(CurrentSimulationTime >= NextAgentMoveFitnessUpdate) {
            UpdateAgentMoveFitness(Agents);
            UpdateAgentMoveFitness(AgentsPredefinedBehaviour);
            NextAgentMoveFitnessUpdate += AgentMoveFitnessUpdateInterval;
        }

        // Update agent aim fitness
        if (CurrentSimulationTime >= NextAgentAimFitnessUpdate) {
            UpdateAgentAimFitness(Agents);
            UpdateAgentAimFitness(AgentsPredefinedBehaviour);
            NextAgentAimFitnessUpdate += AgentAimFitnessUpdateInterval;
        }
    }

    void UpdateAgentsWithBTs(AgentComponent[] agents) {
        ActionBuffer actionBuffer;
        foreach(RobocodeAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf) {
                actionBuffer = new ActionBuffer(null, new int[] { 0, 0, 0, 0, 0 }); // Forward, Side, Rotate, TurrentRotation, Shoot

                agent.BehaviourTree.UpdateTree(actionBuffer);
                MoveAgent(agent, actionBuffer);
                ShootMissile(agent, actionBuffer);
            }
        }

    }

    void MoveAgent(RobocodeAgentComponent agent, ActionBuffer actionBuffer) {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;
        var rotateTurrentDir = Vector3.zero;

        var forwardAxis = actionBuffer.DiscreteActions[0];
        var rightAxis = actionBuffer.DiscreteActions[1];
        var rotateAxis = actionBuffer.DiscreteActions[2];
        var rotateTurrentAxis = actionBuffer.DiscreteActions[3];

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

        switch (rotateTurrentAxis) {
            case 1:
                rotateTurrentDir = agent.transform.up * -1f;
                break;
            case 2:
                rotateTurrentDir = agent.transform.up * 1f;
                break;
        }

        // Movement Version 1 
        /*agent.transform.Translate(dirToGo * Time.fixedDeltaTime * AgentMoveSpeed);
        agent.transform.Rotate(rotateDir, Time.fixedDeltaTime * AgentRotationSpeed);*/

        // Movement Version 2
        agent.Rigidbody.MovePosition(agent.Rigidbody.position + (dirToGo * AgentMoveSpeed * Time.fixedDeltaTime));
        Quaternion turnRotation = Quaternion.Euler(0.0f, rotateDir.y * Time.fixedDeltaTime * AgentRotationSpeed, 0.0f);
        agent.Rigidbody.MoveRotation(agent.Rigidbody.rotation * turnRotation);
        
        // Turrent rotation
        agent.Turret.transform.Rotate(rotateTurrentDir, Time.fixedDeltaTime * AgentTurrentRotationSpeed);
    }

    void ShootMissile(AgentComponent agent, ActionBuffer actionBuffer) {
        GameObject obj;
        Rigidbody rb;
        MissileComponent mc;
        if (actionBuffer.DiscreteActions[4] == 1 && (agent as RobocodeAgentComponent).NextShootTime <= CurrentSimulationTime) {
            Vector3 spawnPosition = (agent as RobocodeAgentComponent).MissileSpawnPoint.transform.position;
            Quaternion spawnRotation = (agent as RobocodeAgentComponent).Turret.transform.rotation;

            Vector3 localXDir = (agent as RobocodeAgentComponent).MissileSpawnPoint.transform.TransformDirection(Vector3.forward);
            Vector3 velocity = localXDir * MissleLaunchSpeed;

            //Instantiate object
            obj = Instantiate(MissilePrefab, spawnPosition, spawnRotation, this.transform);
            obj.layer = gameObject.layer;
            rb = obj.GetComponent<Rigidbody>();
            rb.velocity = velocity;
            mc = obj.GetComponent<MissileComponent>();
            mc.Parent = agent;
            mc.RobocodeEnvironmentController = this;
            (agent as RobocodeAgentComponent).NextShootTime = CurrentSimulationTime + MissileShootCooldown;
        }
    }


    void MoveAgentsWithController1(AgentComponent[] agents) {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        foreach (RobocodeAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf) {
                agent.transform.Translate(Vector3.forward * Time.fixedDeltaTime * verticalInput * AgentMoveSpeed);
                agent.transform.Rotate(Vector3.up, horizontalInput * Time.fixedDeltaTime * AgentRotationSpeed);
            }
        }
    }

    void MoveAgentsWithController2(AgentComponent[] agents) {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        foreach(RobocodeAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf) {
                Vector3 moveDirection = agent.transform.forward * verticalInput * AgentMoveSpeed * Time.fixedDeltaTime;
                agent.Rigidbody.MovePosition(agent.Rigidbody.position + moveDirection);

                float rotation = horizontalInput * AgentRotationSpeed * Time.fixedDeltaTime;
                Quaternion turnRotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                agent.Rigidbody.MoveRotation(agent.Rigidbody.rotation * turnRotation);
            }
        }
    }

    void AgentsShoot(AgentComponent[] agents) {
        GameObject obj;
        Rigidbody rb;
        MissileComponent mc;
        
        foreach (RobocodeAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf) {
                if (Util.rnd.NextDouble() > 0.5f && agent.NextShootTime <= CurrentSimulationTime) {
                    Vector3 spawnPosition = agent.MissileSpawnPoint.transform.position;
                    Quaternion spawnRotation = agent.Turret.transform.rotation;

                    Vector3 localXDir = agent.MissileSpawnPoint.transform.TransformDirection(Vector3.forward);
                    Vector3 velocity = localXDir * MissleLaunchSpeed;

                    //Instantiate object
                    obj = Instantiate(MissilePrefab, spawnPosition, spawnRotation, this.transform);
                    obj.layer = gameObject.layer;
                    rb = obj.GetComponent<Rigidbody>();
                    rb.velocity = velocity;
                    mc = obj.GetComponent<MissileComponent>();
                    mc.Parent = agent;
                    mc.RobocodeEnvironmentController = this;
                    agent.NextShootTime = CurrentSimulationTime + MissileShootCooldown;
                }
            }
        }
    }

    void AgentsMoveTurret(AgentComponent[] agents) {
        foreach(RobocodeAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf) {
                Vector3 rotateTurrentDir = agent.transform.up * TurretMoveDir;
                agent.Turret.transform.Rotate(rotateTurrentDir, Time.fixedDeltaTime * AgentTurrentRotationSpeed);
            }
        }
    }

    public void AddSurvivalFitnessBonus(AgentComponent[] agents) {
        bool lastSurvival = GetNumOfActiveAgents() > 1? false: true;
        // Survival bonus
        foreach (RobocodeAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf) {
                agent.AgentFitness.Fitness.UpdateFitness(RobocodeFitness.FitnessValues[RobocodeFitness.FitnessKeys.SurvivalBonus.ToString()], RobocodeFitness.FitnessKeys.SurvivalBonus.ToString());

                // Last survival bonus
                if (lastSurvival) {
                    agent.AgentFitness.Fitness.UpdateFitness(RobocodeFitness.FitnessValues[RobocodeFitness.FitnessKeys.LastSurvivalBonus.ToString()], RobocodeFitness.FitnessKeys.LastSurvivalBonus.ToString());
                }
            }
        }

    }

    public void TankHit(MissileComponent missile, AgentComponent hitAgent) {
        UpdateFitnesses(missile, hitAgent);
        UpdateAgentHealth(missile, hitAgent as RobocodeAgentComponent);
    }

    public void ObstacleMissedAgent(MissileComponent missile) {
        missile.Parent.AgentFitness.Fitness.UpdateFitness(RobocodeFitness.FitnessValues[RobocodeFitness.FitnessKeys.MissileMissedAgent.ToString()], RobocodeFitness.FitnessKeys.MissileMissedAgent.ToString());
    }


    void UpdateFitnesses(MissileComponent missile, AgentComponent hitAgent) {
        // Update Agent whose missile hit the other tank
        missile.Parent.AgentFitness.Fitness.UpdateFitness(RobocodeFitness.FitnessValues[RobocodeFitness.FitnessKeys.MissileHitAgent.ToString()], RobocodeFitness.FitnessKeys.MissileHitAgent.ToString());

        // Update Agent who got hit by a missile
        hitAgent.AgentFitness.Fitness.UpdateFitness(RobocodeFitness.FitnessValues[RobocodeFitness.FitnessKeys.AgentHitByRocket.ToString()], RobocodeFitness.FitnessKeys.AgentHitByRocket.ToString());
    }

    void UpdateAgentHealth(MissileComponent missile, RobocodeAgentComponent hitAgent) {
        RobocodeAgentComponent rba = hitAgent as RobocodeAgentComponent;
        rba.Health -= MissileHealthDamage;

        if (rba.Health <= 0) {
            switch (GameScenarioType) {
                case RobocodeGameScenarioType.Normal:
                    hitAgent.gameObject.SetActive(false);
                    AddSurvivalFitnessBonus(Agents);
                    AddSurvivalFitnessBonus(AgentsPredefinedBehaviour);
                    CheckEndingState();
                    break;
                case RobocodeGameScenarioType.Deathmatch:
                    missile.Parent.AgentFitness.Fitness.UpdateFitness(RobocodeFitness.FitnessValues[RobocodeFitness.FitnessKeys.AgentDestroyedBonus.ToString()], RobocodeFitness.FitnessKeys.AgentDestroyedBonus.ToString());
                    hitAgent.AgentFitness.Fitness.UpdateFitness(RobocodeFitness.FitnessValues[RobocodeFitness.FitnessKeys.DeathPenalty.ToString()], RobocodeFitness.FitnessKeys.DeathPenalty.ToString());
                    RespawnAgent(hitAgent);
                    break;
            }
        }

    }

    void RespawnAgent(RobocodeAgentComponent agent) {
        // Restore health
        agent.Health = AgentStartHealth;

        // Set to new position
        Vector3 respawnPos;
        Quaternion rotation;

        do {
            respawnPos = GetAgentRandomSpawnPoint();

            rotation = GetAgentRandomRotation();


        } while (!RespawnPointSuitable(respawnPos, rotation));

        agent.transform.position = respawnPos;
        agent.transform.rotation = rotation;

        if (Debug)
            UnityEngine.Debug.Log("Agent respawned!");
    }

    void UpdateAgentMoveFitness(AgentComponent[] agents) {
        foreach(RobocodeAgentComponent agent in agents) {
            // Only update agents that are active
            if (agent.gameObject.activeSelf) {
                if (agent.LastKnownPosition != Vector3.zero) {
                    float distance = Vector3.Distance(agent.LastKnownPosition, agent.transform.position);
                    if (distance >= AgentMoveFitnessMinDistance) {
                        agent.AgentFitness.Fitness.UpdateFitness(RobocodeFitness.FitnessValues[RobocodeFitness.FitnessKeys.AgentMovedBonus.ToString()], RobocodeFitness.FitnessKeys.AgentMovedBonus.ToString());
                    }
                }
                agent.LastKnownPosition = agent.transform.position;
            }
        }

    }

    void UpdateAgentAimFitness(AgentComponent[] agents) {
        foreach (RobocodeAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf) {
                RaySensorBase raySensor = agent.gameObject.GetComponentInChildren<RaySensorBase>();
                raySensor.LayerMask = (1 << gameObject.layer) + 1; // base layer + default

                SensorPerceiveOutput[] sensorPerceiveOutputs = raySensor.Perceive();

                if (sensorPerceiveOutputs[0].HasHit) {
                    AgentComponent otherAgent = sensorPerceiveOutputs[0].HitGameObjects[0].GetComponent<AgentComponent>();
                    if(otherAgent != null) {
                        agent.AgentFitness.Fitness.UpdateFitness(RobocodeFitness.FitnessValues[RobocodeFitness.FitnessKeys.AgentAimingOpponent.ToString()], RobocodeFitness.FitnessKeys.AgentAimingOpponent.ToString());
                    }
                }
            }
        }
    }

}

public enum RobocodeGameScenarioType {
    Normal,
    Deathmatch
}
