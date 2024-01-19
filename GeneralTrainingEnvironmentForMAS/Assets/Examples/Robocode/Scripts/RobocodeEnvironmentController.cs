using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class RobocodeEnvironmentController : EnvironmentControllerBase {

    public static int MissileHealthDamage = 1;

    [SerializeField] float MissileShootCooldown = 1.0f;
    [SerializeField] float MissleLaunchSpeed = 30f;
    [SerializeField] float ForwardSpeed = 1f;
    [SerializeField] float LateralSpeed = 1f;
    [SerializeField] GameObject MissilePrefab;
    [SerializeField] MissileController MissileController;
    [SerializeField] int AgentStartHealth = 10;
    [SerializeField] GameScenarioType GameScenarioType = GameScenarioType.Normal;

    float AgentMoveSpeed = 5f;
    float AgentRotationSpeed = 80f;
    float AgentTurrentRotationSpeed = 90f;

    protected override void DefineAdditionalDataOnStart() {
        foreach (RobocodeAgentComponent agent in Agents) {
            agent.Health =  AgentStartHealth;
        }
    }

    public override void UpdateAgents() {
        //MoveAgentsWithController1();
        //MoveAgentsWithController2();
        //AgentsShoot();
        
        MoveAgentsWithBehaviourTrees();
    }

    void MoveAgentsWithBehaviourTrees() {
        ActionBuffers actionBuffers;
        for (int i = 0; i < Agents.Length; i++) {
            if (Agents[i].gameObject.activeSelf) {
                actionBuffers = new ActionBuffers(null, new int[] { 0, 0, 0, 0, 0 }); // Forward, Side, Rotate, TurrentRotation, Shoot

                Agents[i].BehaviourTree.UpdateTree(actionBuffers);
                MoveAgent(Agents[i], actionBuffers);
                ShootMissile(Agents[i], actionBuffers);
            }
        }

    }

    void MoveAgent(AgentComponent agent, ActionBuffers actionBuffer) {
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

        /*switch (rightAxis) {
            case 1:
                dirToGo = agent.transform.right * tanksController.LateralSpeed;
                break;
            case 2:
                dirToGo = agent.transform.right * -tanksController.LateralSpeed;
                break;
        }*/

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
        (agent as RobocodeAgentComponent).Turret.transform.Rotate(rotateTurrentDir, Time.fixedDeltaTime * AgentTurrentRotationSpeed);
    }

    void ShootMissile(AgentComponent agent, ActionBuffers actionBuffers) {
        GameObject obj;
        Rigidbody rb;
        MissileComponent mc;
        if (actionBuffers.DiscreteActions[4] == 1 && (agent as RobocodeAgentComponent).NextShootTime <= CurrentSimulationTime) {
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

    // Old controllers START
    void MoveAgentsWithController1() {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        horizontalInput = 1f;
        verticalInput = 1f;

        for (int i = 0; i < Agents.Length; i++) {
            Agents[i].transform.Translate(Vector3.forward * Time.fixedDeltaTime * verticalInput * AgentMoveSpeed);
            Agents[i].transform.Rotate(Vector3.up, horizontalInput * Time.fixedDeltaTime * AgentRotationSpeed);

        }
    }

    void MoveAgentsWithController2() {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        horizontalInput = 1f;
        verticalInput = 1f;

        for (int i = 0; i < Agents.Length; i++) { 
            Vector3 moveDirection = Agents[i].transform.forward * verticalInput * AgentMoveSpeed * Time.fixedDeltaTime;
            Agents[i].Rigidbody.MovePosition(Agents[i].Rigidbody.position + moveDirection);

            float rotation = horizontalInput * AgentRotationSpeed * Time.fixedDeltaTime;
            Quaternion turnRotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            Agents[i].Rigidbody.MoveRotation(Agents[i].Rigidbody.rotation * turnRotation);
        }
    }

    void AgentsShoot() {
        GameObject obj;
        Rigidbody rb;
        MissileComponent mc;
        for (int i = 0; i < Agents.Length; i++) {
            if (Util.rnd.NextDouble() > 0.5f && (Agents[i] as RobocodeAgentComponent).NextShootTime <= CurrentSimulationTime) {
                Vector3 spawnPosition = (Agents[i] as RobocodeAgentComponent).MissileSpawnPoint.transform.position;
                Quaternion spawnRotation = Agents[i].transform.rotation;

                Vector3 localXDir = (Agents[i] as RobocodeAgentComponent).MissileSpawnPoint.transform.TransformDirection(Vector3.forward);
                Vector3 velocity = localXDir * MissleLaunchSpeed;

                //Instantiate object
                obj = Instantiate(MissilePrefab, spawnPosition, spawnRotation, this.transform);
                obj.layer = gameObject.layer;
                rb = obj.GetComponent<Rigidbody>();
                rb.velocity = velocity;
                mc = obj.GetComponent<MissileComponent>();
                mc.Parent = Agents[i];
                mc.RobocodeEnvironmentController = this;
                (Agents[i] as RobocodeAgentComponent).NextShootTime = CurrentSimulationTime + MissileShootCooldown;
            }
        }
    }

    // Old controllers END

    int GetNumOfActiveAgents() {
        int counter = 0;

        foreach (var agent in Agents) {
            if(agent.gameObject.activeSelf) 
                counter++;
        }

        return counter;
    }

    public void AddSurvivalFitnessBonus() {
        bool lastSurvival = GetNumOfActiveAgents() > 1? false: true;
        // Survival bonus
        foreach (var agent in Agents) {
            if (agent.gameObject.activeSelf) {
                agent.AgentFitness.Fitness.UpdateFitness(RobocodeFitness.SURVIVAL_BONUS);

                // Last survival bonus
                if (lastSurvival) {
                    agent.AgentFitness.Fitness.UpdateFitness(RobocodeFitness.LAST_SURVIVAL_BONUS);
                }
            }
        }

    }

    public void TankHit(MissileComponent missile, AgentComponent hitAgent) {
        UpdateFitnesses(missile, hitAgent);
        UpdateAgentHealth(missile, hitAgent as RobocodeAgentComponent);
    }

    public void ObstacleHit(MissileComponent missile) {
        missile.Parent.AgentFitness.Fitness.UpdateFitness(RobocodeFitness.MISSILE_HIT_OBSTACLE);
    }


    void UpdateFitnesses(MissileComponent missile, AgentComponent hitAgent) {
        // Update Agent whose missile hit the other tank
        missile.Parent.AgentFitness.Fitness.UpdateFitness(RobocodeFitness.MISSILE_HIT_TANK);

        // Update Agent who got hit by a missile
        hitAgent.AgentFitness.Fitness.UpdateFitness(RobocodeFitness.TANK_HIT_BY_ROCKET);
    }

    void UpdateAgentHealth(MissileComponent missile, RobocodeAgentComponent hitAgent) {
        RobocodeAgentComponent rba = hitAgent as RobocodeAgentComponent;
        rba.Health -= MissileHealthDamage;

        if (rba.Health <= 0) {
            switch (GameScenarioType) {
                case GameScenarioType.Normal:
                    hitAgent.gameObject.SetActive(false);
                    AddSurvivalFitnessBonus();
                    break;
                case GameScenarioType.Deathmatch:
                    missile.Parent.AgentFitness.Fitness.UpdateFitness(RobocodeFitness.TANK_DESTROYED_BONUS);
                    hitAgent.AgentFitness.Fitness.UpdateFitness(RobocodeFitness.DEATH_PENALTY);
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


        } while (RespawnPointSuitable(respawnPos));

        agent.transform.position = respawnPos;
        agent.transform.rotation = rotation;

        if (Debug)
            UnityEngine.Debug.Log("Agent respawned!");
    }


}

public enum GameScenarioType {
    Normal,
    Deathmatch
}
