using Collector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheKiwiCoder;
using UnityEngine;
using UnityEngine.EventSystems;

public class RobostrikeEnvironmentController : EnvironmentControllerBase {

    [Header("Robostrike configuration Movement")]
    [SerializeField] float AgentMoveSpeed = 5f;
    [SerializeField] float AgentRotationSpeed = 80f;
    [SerializeField] float AgentTurrentRotationSpeed = 90f;

    [Header("Robostrike configuration Missile")]
    [SerializeField, Tooltip("Destroy Missile After X seconds")] public float DestroyMissileAfter = 3.0f;
    [SerializeField] float MissileShootCooldown = 1.0f;
    [SerializeField] float MissleLaunchSpeed = 30f;
    [SerializeField] public static int MissileDamage = 2;

    [Header("Robostrike configuration Agent Move Fitness")]
    [SerializeField] float AgentMoveFitnessUpdateInterval = 5f;
    [SerializeField] float AgentMoveFitnessMinDistance = 3f;
    [Header("Robostrike configuration Agent Aim Fitness")]
    [SerializeField] float AgentAimFitnessUpdateInterval = 5f;
    [Header("Robostrike configuration Agent Near Wall Fitness")]
    [SerializeField] float AgentNearWallUpdateInterval = 5f;

    [Header("Robostrike configuration General")]
    [SerializeField] GameObject MissilePrefab;
    [SerializeField] MissileController MissileController;
    [SerializeField] int AgentStartHealth = 10;
    [SerializeField] int AgentStartShield = 0;
    [SerializeField] int AgentStartAmmo = 0;
    [SerializeField] RobostrikeGameScenarioType GameScenarioType = RobostrikeGameScenarioType.Normal;
    [SerializeField] RobostrikeAgentRespawnType AgentRespawnType = RobostrikeAgentRespawnType.StartPos;

    [Header("Robostrike configuration PowerUps")]
    [SerializeField] int HealthPowerUpValue = 5;
    [SerializeField] int ShieldPowerUpValue = 5;
    [SerializeField] int AmmoPowerUpValue = 10;

    [SerializeField] public static int MAX_HEALTH = 10;
    [SerializeField] public static int MAX_SHIELD = 10;
    [SerializeField] public static int MAX_AMMO = 20;

    [Header("Robostrike Power Up Prefabs")]
    [SerializeField] public float MinPowerUpDistance = 8f;
    [SerializeField] public Vector3 PowerUpColliderExtendsMultiplier = new Vector3(0.505f, 0.495f, 0.505f);
    [SerializeField] GameObject HealthBoxPrefab;
    [SerializeField] int HealthBoxSpawnAmount = 2;
    [SerializeField] GameObject ShieldBoxPrefab;
    [SerializeField] int ShieldBoxSpawnAmount = 2;
    [SerializeField] GameObject AmmoBoxPrefab;
    [SerializeField] int AmmoBoxSpawnAmount = 2;

    [Header("User Input")]
    [SerializeField] KeyCode ShootKey = KeyCode.Space;
    [SerializeField] KeyCode TurretMovementLeft = KeyCode.Q;
    [SerializeField] KeyCode TurretMovementRight = KeyCode.E;

    [Header("Game Type")]
    [SerializeField] RobostrikeGameMode GameMode = RobostrikeGameMode.OneVsOne;

    [Header("1 vs 1 Agent Configuration")]
    [SerializeField] RobostrikeAgentConfig[] RobostrikeAgentConfigs;


    float ForwardSpeed = 1f;

    bool IsPressingShoot;
    float TurretMoveDir;
    float NextAgentMoveFitnessUpdate = 0;
    float NextAgentAimFitnessUpdate = 0;
    float NextAgentNearWallFitnessUpdate = 0;

    ActionBuffer actionBuffer;

    // Move Agent variables
    Vector3 dirToGo = Vector3.zero;
    Vector3 rotateDir = Vector3.zero;
    Vector3 rotateTurrentDir = Vector3.zero;

    int forwardAxis = 0;
    int rightAxis = 0;
    int rotateAxis = 0;
    int rotateTurrentAxis = 0;
    Quaternion turnRotation;

    // Shoot missile variables
    GameObject obj;
    Rigidbody rb;
    MissileComponent mc;
    Vector3 spawnPosition;
    Quaternion spawnRotation;
    Vector3 localXDir;
    Vector3 velocity;

    protected override void DefineAdditionalDataOnPostAwake()
    {
        // Read params from configuration if it exists
        ReadParamsFromMainConfiguration();

        actionBuffer = new ActionBuffer(null, new int[] { 0, 0, 0, 0, 0 }); // Forward, Side, Rotate, TurrentRotation, Shoot
    }

    protected override void DefineAdditionalDataOnPreStart()
    {
        // Based on the game type spawn agents
        switch (GameMode)
        {
            case RobostrikeGameMode.OneVsOne:
                SpawnAgentsForOneVsOne();
                break;
            case RobostrikeGameMode.AllVsAll:
                // TODO : Implement
                break;
        }
    }

    protected override void DefineAdditionalDataOnPostStart() {
        foreach (RobostrikeAgentComponent agent in Agents) {
            agent.HealthComponent.Health =  AgentStartHealth;
            agent.ShieldComponent.Shield = AgentStartShield;
            agent.AmmoComponent.Ammo = AgentStartAmmo;
            agent.Rigidbody = agent.GetComponent<Rigidbody>();
        }

        foreach (RobostrikeAgentComponent agent in AgentsPredefinedBehaviour) {
            agent.HealthComponent.Health = AgentStartHealth;
            agent.ShieldComponent.Shield = AgentStartShield;
            agent.AmmoComponent.Ammo = AgentStartAmmo;
            agent.Rigidbody = agent.GetComponent<Rigidbody>();
        }

        // Spawn powerUps
        SpawnPowerUps();

        // Register event for Ray sensor
        RayHitObject.OnTargetHit += RayHitObject_OnTargetHit;
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

    void ReadParamsFromMainConfiguration()
    {
        if (MenuManager.Instance != null && MenuManager.Instance.MainConfiguration != null)
        {
            MainConfiguration conf = MenuManager.Instance.MainConfiguration;

            RobostrikeFitness.FitnessValues = conf.FitnessValues;

            if (conf.ProblemConfiguration.ContainsKey("ArenaSizeX"))
            {
                ArenaSize.x = float.Parse(conf.ProblemConfiguration["ArenaSizeX"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("ArenaSizeZ"))
            {
                ArenaSize.z = float.Parse(conf.ProblemConfiguration["ArenaSizeZ"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("ArenaOffset"))
            {
                ArenaOffset = float.Parse(conf.ProblemConfiguration["ArenaOffset"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("GameMode"))
            {
                GameMode = (RobostrikeGameMode)int.Parse(conf.ProblemConfiguration["GameMode"]);
            }

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

            if (conf.ProblemConfiguration.ContainsKey("DestroyMissileAfter"))
            {
                DestroyMissileAfter = float.Parse(conf.ProblemConfiguration["DestroyMissileAfter"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("MissileShootCooldown"))
            {
                MissileShootCooldown = float.Parse(conf.ProblemConfiguration["MissileShootCooldown"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("MissleLaunchSpeed"))
            {
                MissleLaunchSpeed = float.Parse(conf.ProblemConfiguration["MissleLaunchSpeed"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("MissileDamage"))
            {
                MissileDamage = int.Parse(conf.ProblemConfiguration["MissileDamage"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("AgentStartHealth"))
            {
                AgentStartHealth = int.Parse(conf.ProblemConfiguration["AgentStartHealth"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("AgentStartShield"))
            {
                AgentStartShield = int.Parse(conf.ProblemConfiguration["AgentStartShield"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("AgentStartAmmo"))
            {
                AgentStartAmmo = int.Parse(conf.ProblemConfiguration["AgentStartAmmo"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("GameScenarioType"))
            {
                GameScenarioType = (RobostrikeGameScenarioType) int.Parse(conf.ProblemConfiguration["GameScenarioType"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("AgentRespawnType"))
            {
                AgentRespawnType = (RobostrikeAgentRespawnType)int.Parse(conf.ProblemConfiguration["AgentRespawnType"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("HealthPowerUpValue"))
            {
                HealthPowerUpValue = int.Parse(conf.ProblemConfiguration["HealthPowerUpValue"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("ShieldPowerUpValue"))
            {
                ShieldPowerUpValue = int.Parse(conf.ProblemConfiguration["ShieldPowerUpValue"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("AmmoPowerUpValue"))
            {
                AmmoPowerUpValue = int.Parse(conf.ProblemConfiguration["AmmoPowerUpValue"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("MaxHealth"))
            {
                MAX_HEALTH = int.Parse(conf.ProblemConfiguration["MaxHealth"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("MaxShield"))
            {
                MAX_SHIELD = int.Parse(conf.ProblemConfiguration["MaxShield"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("MaxAmmo"))
            {
                MAX_AMMO = int.Parse(conf.ProblemConfiguration["MaxAmmo"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("MinPowerUpDistance"))
            {
                MinPowerUpDistance = float.Parse(conf.ProblemConfiguration["MinPowerUpDistance"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("HealthBoxSpawnAmount"))
            {
                HealthBoxSpawnAmount = int.Parse(conf.ProblemConfiguration["HealthBoxSpawnAmount"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("ShieldBoxSpawnAmount"))
            {
                ShieldBoxSpawnAmount = int.Parse(conf.ProblemConfiguration["ShieldBoxSpawnAmount"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("AmmoBoxSpawnAmount"))
            {
                AmmoBoxSpawnAmount = int.Parse(conf.ProblemConfiguration["AmmoBoxSpawnAmount"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("AgentMoveFitnessUpdateInterval"))
            {
                AgentMoveFitnessUpdateInterval = float.Parse(conf.ProblemConfiguration["AgentMoveFitnessUpdateInterval"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("AgentMoveFitnessMinDistance"))
            {
                AgentMoveFitnessMinDistance = float.Parse(conf.ProblemConfiguration["AgentMoveFitnessMinDistance"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("AgentAimFitnessUpdateInterval"))
            {
                AgentAimFitnessUpdateInterval = float.Parse(conf.ProblemConfiguration["AgentAimFitnessUpdateInterval"]);
            }

            if (conf.ProblemConfiguration.ContainsKey("AgentNearWallUpdateInterval"))
            {
                AgentNearWallUpdateInterval = float.Parse(conf.ProblemConfiguration["AgentNearWallUpdateInterval"]);
            }

        }
    }

    public override void UpdateAgents(bool updateBTs) {
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
            UpdateAgentsWithBTs(Agents, updateBTs);
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
            UpdateAgentsWithBTs(AgentsPredefinedBehaviour, updateBTs);
        }

        // Update agent move fitness
        /*if(CurrentSimulationTime >= NextAgentMoveFitnessUpdate) {
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

        // Update agent near wall fitness
        if (CurrentSimulationTime >= NextAgentNearWallFitnessUpdate) {
            UpdateAgentNearWallFitness(Agents);
            UpdateAgentNearWallFitness(AgentsPredefinedBehaviour);
            NextAgentNearWallFitnessUpdate += AgentNearWallUpdateInterval;
        }*/
    }

    void UpdateAgentsWithBTs(AgentComponent[] agents, bool updateBTs) {
        foreach (RobostrikeAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf && agent.BehaviourTree != null) {
                //actionBuffer = new ActionBuffer(null, new int[] { 0, 0, 0, 0, 0 }); // Forward, Side, Rotate, TurrentRotation, Shoot
                actionBuffer.ResetDiscreteActions();

                if (updateBTs)
                {
                    agent.BehaviourTree.UpdateTree(actionBuffer);
                }
                MoveAgent(agent, actionBuffer);
                ShootMissile(agent, actionBuffer);
            }
        }

    }

    void MoveAgent(RobostrikeAgentComponent agent, ActionBuffer actionBuffer) {
        dirToGo = Vector3.zero;
        rotateDir = Vector3.zero;
        rotateTurrentDir = Vector3.zero;

        forwardAxis = actionBuffer.DiscreteActions[0];
        rightAxis = actionBuffer.DiscreteActions[1];
        rotateAxis = actionBuffer.DiscreteActions[2];
        rotateTurrentAxis = actionBuffer.DiscreteActions[3];

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
        turnRotation = Quaternion.Euler(0.0f, rotateDir.y * Time.fixedDeltaTime * AgentRotationSpeed, 0.0f);

        agent.Rigidbody.MovePosition(agent.Rigidbody.position + (dirToGo * AgentMoveSpeed * Time.fixedDeltaTime));
        agent.Rigidbody.MoveRotation(agent.Rigidbody.rotation * turnRotation);

        // Turrent rotation
        agent.Turret.transform.Rotate(rotateTurrentDir, Time.fixedDeltaTime * AgentTurrentRotationSpeed);
    }

    void ShootMissile(AgentComponent agent, ActionBuffer actionBuffer) {
        obj = null;
        rb = null;
        mc = null;
        if (actionBuffer.DiscreteActions[4] == 1 && (agent as RobostrikeAgentComponent).NextShootTime <= CurrentSimulationTime && (agent as RobostrikeAgentComponent).AmmoComponent.Ammo > 0) {
            spawnPosition = (agent as RobostrikeAgentComponent).MissileSpawnPoint.transform.position;
            spawnRotation = (agent as RobostrikeAgentComponent).Turret.transform.rotation;

            localXDir = (agent as RobostrikeAgentComponent).MissileSpawnPoint.transform.TransformDirection(Vector3.forward);
            velocity = localXDir * MissleLaunchSpeed;

            //Instantiate object
            obj = Instantiate(MissilePrefab, spawnPosition, spawnRotation, this.transform);
            obj.layer = gameObject.layer;
            rb = obj.GetComponent<Rigidbody>();
            rb.velocity = velocity;
            mc = obj.GetComponent<MissileComponent>();
            mc.Parent = agent;
            mc.RobostrikeEnvironmentController = this;
            (agent as RobostrikeAgentComponent).NextShootTime = CurrentSimulationTime + MissileShootCooldown;

            (agent as RobostrikeAgentComponent).MissileFired();

            // Update fitness
            agent.AgentFitness.Fitness.UpdateFitness(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.AgentFiredMissile.ToString()], RobostrikeFitness.FitnessKeys.AgentFiredMissile.ToString());
        }
    }


    void MoveAgentsWithController1(AgentComponent[] agents) {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        foreach (RobostrikeAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf) {
                agent.transform.Translate(Vector3.forward * Time.fixedDeltaTime * verticalInput * AgentMoveSpeed);
                agent.transform.Rotate(Vector3.up, horizontalInput * Time.fixedDeltaTime * AgentRotationSpeed);
            }
        }
    }

    void MoveAgentsWithController2(AgentComponent[] agents) {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        foreach(RobostrikeAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf) {
                Vector3 moveDirection = agent.transform.forward * verticalInput * AgentMoveSpeed * Time.fixedDeltaTime;
                agent.Rigidbody.MovePosition(agent.Rigidbody.position + moveDirection);

                float rotation = horizontalInput * AgentRotationSpeed * Time.fixedDeltaTime;
                Quaternion turnRotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                agent.Rigidbody.MoveRotation(agent.Rigidbody.rotation * turnRotation);
            }
        }
    }

    public void AgentsShoot(AgentComponent[] agents) {
        GameObject obj;
        Rigidbody rb;
        MissileComponent mc;
        
        foreach (RobostrikeAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf) {
                if (agent.NextShootTime <= CurrentSimulationTime && agent.AmmoComponent.Ammo > 0) {
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
                    mc.RobostrikeEnvironmentController = this;
                    agent.NextShootTime = CurrentSimulationTime + MissileShootCooldown;
                    agent.MissileFired();

                    // Update fitness
                    agent.AgentFitness.Fitness.UpdateFitness(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.AgentFiredMissile.ToString()], RobostrikeFitness.FitnessKeys.AgentFiredMissile.ToString());
                }
            }
        }
    }

    void AgentsMoveTurret(AgentComponent[] agents) {
        foreach(RobostrikeAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf) {
                Vector3 rotateTurrentDir = agent.transform.up * TurretMoveDir;
                agent.Turret.transform.Rotate(rotateTurrentDir, Time.fixedDeltaTime * AgentTurrentRotationSpeed);
            }
        }
    }

    public void AddSurvivalFitnessBonus(AgentComponent[] agents) {
        bool lastSurvival = GetNumOfActiveAgents() > 1? false: true;
        // Survival bonus
        foreach (RobostrikeAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf) {
                agent.AgentFitness.Fitness.UpdateFitness(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.SurvivalBonus.ToString()], RobostrikeFitness.FitnessKeys.SurvivalBonus.ToString());

                // Last survival bonus
                if (lastSurvival) {
                    agent.AgentFitness.Fitness.UpdateFitness(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.LastSurvivalBonus.ToString()], RobostrikeFitness.FitnessKeys.LastSurvivalBonus.ToString());
                }
            }
        }

    }

    public void TankHit(MissileComponent missile, AgentComponent hitAgent) {
        UpdateFitnesses(missile, hitAgent);
        UpdateAgentHealth(missile, hitAgent as RobostrikeAgentComponent);
    }

    public void ObstacleMissedAgent(MissileComponent missile) {
        missile.Parent.AgentFitness.Fitness.UpdateFitness(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.MissileMissedAgent.ToString()], RobostrikeFitness.FitnessKeys.MissileMissedAgent.ToString());
    }


    void UpdateFitnesses(MissileComponent missile, AgentComponent hitAgent) {
        // Update Agent whose missile hit the other tank
        missile.Parent.AgentFitness.Fitness.UpdateFitness(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.MissileHitAgent.ToString()], RobostrikeFitness.FitnessKeys.MissileHitAgent.ToString());

        // Update Agent who got hit by a missile
        hitAgent.AgentFitness.Fitness.UpdateFitness(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.AgentHitByRocket.ToString()], RobostrikeFitness.FitnessKeys.AgentHitByRocket.ToString());
    }

    void UpdateAgentHealth(MissileComponent missile, RobostrikeAgentComponent hitAgent) {
        RobostrikeAgentComponent rba = hitAgent as RobostrikeAgentComponent;
        rba.TakeDamage(MissileDamage);

        if (rba.HealthComponent.Health <= 0) {
            switch (GameScenarioType) {
                case RobostrikeGameScenarioType.Normal:
                    hitAgent.gameObject.SetActive(false);
                    AddSurvivalFitnessBonus(Agents);
                    AddSurvivalFitnessBonus(AgentsPredefinedBehaviour);
                    CheckEndingState();
                    break;
                case RobostrikeGameScenarioType.Deathmatch:
                    missile.Parent.AgentFitness.Fitness.UpdateFitness(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.AgentDestroyedBonus.ToString()], RobostrikeFitness.FitnessKeys.AgentDestroyedBonus.ToString());
                    hitAgent.AgentFitness.Fitness.UpdateFitness(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.DeathPenalty.ToString()], RobostrikeFitness.FitnessKeys.DeathPenalty.ToString());
                    //hitAgent.LastKnownPositions.Clear(); // TODO: Check if this is needed
                    RespawnAgent(hitAgent);
                    break;
            }
        }

    }

    void RespawnAgent(RobostrikeAgentComponent agent) {
        // Restore health
        agent.HealthComponent.Health = AgentStartHealth;
        // Update Healthbar
        agent.UpdatetStatBars();

        // Set to new position
        Vector3 respawnPos = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        if (AgentRespawnType == RobostrikeAgentRespawnType.StartPos)
        {
            respawnPos = agent.StartPosition;
            rotation = agent.StartRotation;
        }
        else if (AgentRespawnType == RobostrikeAgentRespawnType.Random)
        {
            do
            {
                respawnPos = GetRandomSpawnPoint();
                rotation = GetRandomRotation();

            } while (!RespawnPointSuitable(respawnPos, rotation, AgentColliderExtendsMultiplier, MinAgentDistance));
        }

        agent.transform.position = respawnPos;
        agent.transform.rotation = rotation;

        if (Debug)
            UnityEngine.Debug.Log("Agent respawned!");
    }

    /*void UpdateAgentMoveFitness(AgentComponent[] agents) {
        foreach(RobostrikeAgentComponent agent in agents) {
            // Only update agents that are active
            if (agent.gameObject.activeSelf) {
                if (agent.LastKnownPositions != null && agent.LastKnownPositions.Count == 0) {
                    float distance = Vector3.Distance(agent.LastKnownPositions[0], agent.transform.position);
                    if (distance <= AgentMoveFitnessMinDistance) {
                        agent.AgentFitness.Fitness.UpdateFitness(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.AgentMovedBonus.ToString()], RobostrikeFitness.FitnessKeys.AgentMovedBonus.ToString());
                    }
                }
                agent.LastKnownPositions[0] = agent.transform.position;
            }
        }

    }

    void UpdateAgentNearWallFitness(AgentComponent[] agents) {
        foreach (RobostrikeAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf) {
                Collider agentCol = agent.GetComponent<Collider>();

                Collider[] colliders = Physics.OverlapBox(agent.transform.position, Vector3.one * 0.6f, agent.transform.rotation, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)) + DefaultLayer);
                foreach (Collider col in colliders) {
                    if (col.gameObject.tag.Contains("Wall") || col.gameObject.tag.Contains("Obstacle")) {
                        agent.AgentFitness.Fitness.UpdateFitness(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.AgentNearWall.ToString()], RobostrikeFitness.FitnessKeys.AgentNearWall.ToString());
                    }
                }
            }
        }
    }*/

    public bool PowerUpPickedUp(PowerUpType powerUpType, AgentComponent agent)
    {
        switch (powerUpType)
        {
            case PowerUpType.Health:
                return HealthBoxPickedUp(agent);
            case PowerUpType.Shield:
                return ShieldBoxPickedUp(agent);
            case PowerUpType.Ammo:
                return AmmoBoxPickedUp(agent);
        }

        return false;
    }

    public bool HealthBoxPickedUp(AgentComponent agent)
    {
        bool operationSuccess = ((RobostrikeAgentComponent)agent).SetHealth(HealthPowerUpValue);
        if (operationSuccess)
        {
            //Update agent fitness
            agent.AgentFitness.Fitness.UpdateFitness(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.AgentPickedUpHealthBoxPowerUp.ToString()], RobostrikeFitness.FitnessKeys.AgentPickedUpHealthBoxPowerUp.ToString());

            // Spawn new health box
            if(HealthBoxPrefab != null)
            {
                SpawnPowerUp(HealthBoxPrefab);
            }
            (agent as RobostrikeAgentComponent).UpdatetStatBars();
        }
        return operationSuccess;
    }

    public bool ShieldBoxPickedUp(AgentComponent agent)
    {
        bool operationSuccess = ((RobostrikeAgentComponent)agent).SetShield(ShieldPowerUpValue);
        if (operationSuccess)
        {
            //Update agent fitness
            agent.AgentFitness.Fitness.UpdateFitness(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.AgentPickedUpShieldBoxPowerUp.ToString()], RobostrikeFitness.FitnessKeys.AgentPickedUpShieldBoxPowerUp.ToString());

            // Spawn new shield box
            if (ShieldBoxPrefab != null)
            {
                SpawnPowerUp(ShieldBoxPrefab);
            }
            (agent as RobostrikeAgentComponent).UpdatetStatBars();
        }

        return operationSuccess;
    }

    public bool AmmoBoxPickedUp(AgentComponent agent)
    {
        bool operationSuccess = ((RobostrikeAgentComponent)agent).SetAmmo(AmmoPowerUpValue);
        if (operationSuccess)
        {
            //Update agent fitness
            agent.AgentFitness.Fitness.UpdateFitness(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.AgentPickedUpAmmoBoxPowerUp.ToString()], RobostrikeFitness.FitnessKeys.AgentPickedUpAmmoBoxPowerUp.ToString());

            // Spawn new ammo box
            if (AmmoBoxPrefab != null)
            {
                SpawnPowerUp(AmmoBoxPrefab);
            }
            (agent as RobostrikeAgentComponent).UpdatetStatBars();
        }

        return operationSuccess;
    }

    private void SpawnPowerUps()
    {
        for (int i = 0; i < HealthBoxSpawnAmount; i++)
        {
            SpawnPowerUp(HealthBoxPrefab);
        }

        for (int i = 0; i < ShieldBoxSpawnAmount; i++)
        {
            SpawnPowerUp(ShieldBoxPrefab);
        }

        for (int i = 0; i < AmmoBoxSpawnAmount; i++)
        {
            SpawnPowerUp(AmmoBoxPrefab);
        }
    }

    private void SpawnPowerUp(GameObject powerUpPrefab)
    {
        if (powerUpPrefab != null)
        {
            List<Vector3> powerUpPositions = this.gameObject.GetComponentsInChildren<PowerUpComponent>().Select(x => x.transform.position).ToList();
            Vector3 spawnPos = GetRandomSpawnPoint();
            while (!SpawnPointSuitable(spawnPos, Quaternion.identity, powerUpPositions != null && powerUpPositions.Count > 0? powerUpPositions : null, PowerUpColliderExtendsMultiplier, MinPowerUpDistance))
            {
                spawnPos = GetRandomSpawnPoint();
            }

            GameObject obj = Instantiate(powerUpPrefab, spawnPos, Quaternion.identity, this.transform);
            obj.layer = gameObject.layer;
        }
    }

    private void SpawnAgentsForOneVsOne()
    {
        if(RobostrikeAgentConfigs.Length < 2)
        {
               throw new Exception("Game config not specified");
        }
        else
        {
            SpawnAgent(RobostrikeAgentConfigs[0]);
            SpawnAgent(RobostrikeAgentConfigs[1]);
        }
    }

    private void SpawnAgent(RobostrikeAgentConfig robostrikeGameConfig)
    {
        if (robostrikeGameConfig.AgentPrefab != null)
        {
            Vector3 spawnPos = Vector3.zero;
            Quaternion spawnRotation = Quaternion.identity;

            if (robostrikeGameConfig.AgentStartSpawnPosition== null)
            {
                List<Vector3> agentPositions = this.gameObject.GetComponentsInChildren<AgentComponent>().Select(x => x.transform.position).ToList();
                spawnPos = GetRandomSpawnPoint();
                spawnRotation = GetRandomRotation();

                while (!SpawnPointSuitable(spawnPos, spawnRotation, agentPositions != null && agentPositions.Count > 0 ? agentPositions : null, AgentColliderExtendsMultiplier, MinAgentDistance))
                {
                    spawnPos = GetRandomSpawnPoint();
                    spawnRotation = GetRandomRotation();
                }
            }
            else
            {
                spawnPos = robostrikeGameConfig.AgentStartSpawnPosition.position;
                spawnRotation = robostrikeGameConfig.AgentStartSpawnPosition.rotation;
            }

            GameObject obj = Instantiate(robostrikeGameConfig.AgentPrefab, spawnPos, spawnRotation, this.transform);
            obj.layer = gameObject.layer;

            if (robostrikeGameConfig.BehaviourTree != null)
            {
                AgentComponent agent = obj.GetComponent<AgentComponent>();
                agent.BehaviourTree = robostrikeGameConfig.BehaviourTree.Clone();
                agent.BehaviourTree.Bind(BehaviourTree.CreateBehaviourTreeContext(agent.gameObject));
                agent.HasPredefinedBehaviour = true;
            }

            // Set agent material
            if (robostrikeGameConfig.AgentMaterial != null)
            {
                RobostrikeAgentComponent agent = obj.GetComponent<RobostrikeAgentComponent>();

                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    renderer.material = robostrikeGameConfig.AgentMaterial;
                }
            }

            // Set control script
            if (robostrikeGameConfig.CustomControlScript)
            {
                MonoBehaviour controlScript = obj.AddComponent<RobostrikeAgentEnemyContoller>();
                controlScript.enabled = true;

                AgentComponent agent = obj.GetComponent<AgentComponent>();
                agent.HasPredefinedBehaviour = true;
            }
        }
        else
        {
            throw new Exception("Agent prefab is null");
        }
    }

    private void RayHitObject_OnTargetHit(object sender, OnTargetHitEventargs e)
    {
        /*// TODO: How to handle only aimimg at opponent??? -> Add Teams concept
        if (e.TargetGameObject.GetComponent<AgentComponent>() != null)
        {
            e.Agent.AgentFitness.Fitness.UpdateFitness(RobostrikeFitness.FitnessValues[RobostrikeFitness.FitnessKeys.AgentAimingOpponent.ToString()], RobostrikeFitness.FitnessKeys.AgentAimingOpponent.ToString());
        }*/
    }
}

public enum RobostrikeGameScenarioType {
    Normal,
    Deathmatch
}

public enum RobostrikeAgentRespawnType
{
    StartPos,
    Random
}

public enum RobostrikeGameMode
{
    OneVsOne,
    AllVsAll,
}