using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class BombermanEnvironmentController: EnvironmentControllerBase {

    [Header("Bomberman Agent configuration")]
    [SerializeField] float AgentStartMoveSpeed = 5f;
    [SerializeField] int StartAgentBombAmout = 1;
    [SerializeField] int StartExplosionRadius = 1;
    [SerializeField] int StartHealth = 1;
    [SerializeField] public float ExplosionDamageCooldown = 0.8f;

    // TODO Add support for continuous movement
    [Header("Descrete Agent movement configuration")]
    [SerializeField] public bool DiscreteAgentMovement = true;
    [SerializeField] public float AgentUpdateinterval = 0.1f;

    private BombExplosionController BombExplosionController { get; set; }

    protected override void DefineAdditionalDataOnAwake() {
        BombExplosionController = GetComponent<BombExplosionController>();
    }

    protected override void DefineAdditionalDataOnStart() {
        SetAgentDefaultParams(Agents);
        SetAgentDefaultParams(AgentsPredefinedBehaviour);
    }

    public override void UpdateAgents() {
        // Update Agents that are being evaluated
        if(ManualAgentControl)
            MoveAgents(Agents);
        else
            UpdateAgentsWithBTs(Agents);
        
        // Update Agents with fixed behaviour
        if (ManualAgentPredefinedBehaviourControl)
            MoveAgents(AgentsPredefinedBehaviour);
        else
            UpdateAgentsWithBTs(AgentsPredefinedBehaviour);

        AgentsOverExplosion();
    }

    protected override void OnUpdate() {
        if (ManualAgentControl) {
            OnGameInput(Agents);
        }
        if (ManualAgentPredefinedBehaviourControl) {
            OnGameInput(AgentsPredefinedBehaviour);
        }
    }

    void SetAgentDefaultParams(AgentComponent[] agents) {
        foreach (BombermanAgentComponent agent in agents) {
            agent.BombermanEnvironmentController = this;
            agent.MoveSpeed = AgentStartMoveSpeed;
            agent.Health = StartHealth;
            agent.ExplosionRadius = StartExplosionRadius;
            agent.SetBombs(StartAgentBombAmout);
        }
    }

    void OnGameInput(AgentComponent[] agents) {
        foreach (BombermanAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf && agent.enabled) {
                if (Input.GetKey(agent.InputUp)) {
                    agent.SetDirection(Vector2.up, agent.SpriteRendererUp);
                }
                else if (Input.GetKey(agent.InputDown)) {
                    agent.SetDirection(Vector2.down, agent.SpriteRendererDown);
                }
                else if (Input.GetKey(agent.InputLeft)) {
                    agent.SetDirection(Vector2.left, agent.SpriteRendererLeft);
                }
                else if (Input.GetKey(agent.InputRight)) {
                    agent.SetDirection(Vector2.right, agent.SpriteRendererRight);
                }
                else {
                    // Player is not moving
                    agent.SetDirection(Vector2.zero, agent.ActiveSpriteRenderer);
                }

                if (agent.BombsRemaining > 0 && Input.GetKeyDown(agent.BombKey)) {
                    StartCoroutine(BombExplosionController.PlaceBomb(agent));
                }
            }
        }
    }

    public void AddSurvivalFitnessBonus() {
        bool lastSurvival = GetNumOfActiveAgents() > 1 ? false : true;
        // Survival bonus
        foreach (var agent in Agents) {
            if (agent.gameObject.activeSelf && agent.enabled) {
                agent.AgentFitness.Fitness.UpdateFitness(BombermanFitness.SURVIVAL_BONUS);

                // Last survival bonus
                if (lastSurvival) {
                    agent.AgentFitness.Fitness.UpdateFitness(BombermanFitness.LAST_SURVIVAL_BONUS);
                }
            }
        }

    }

    void AgentsOverExplosion() {
        foreach (BombermanAgentComponent agent in Agents) {
            if (agent.gameObject.activeSelf && agent.enabled) {
                Collider2D[] hitColliders = Physics2D.OverlapBoxAll(agent.transform.position, new Vector2(1 / 1.15f, 1 / 1.15f), 0f);

                foreach (Collider2D collider in hitColliders) {
                    ExplosionComponent explosion = collider.GetComponent<ExplosionComponent>();
                    if (explosion != null) {
                        ExlosionHitAgent(agent, explosion);
                        return;
                    }
                }
            }
        }
    }

    public void ExlosionHitAgent(BombermanAgentComponent agent, ExplosionComponent explosion) {
        if (agent.NextDamageTime <= CurrentSimulationTime) {
            agent.NextDamageTime = CurrentSimulationTime + ExplosionDamageCooldown;

            agent.Health--;

            if (this == explosion.Parent) {
                agent.AgentFitness.Fitness.UpdateFitness(BombermanFitness.AGENT_HIT_BY_OWN_BOMB);
            }
            else {
                agent.AgentFitness.Fitness.UpdateFitness(BombermanFitness.AGENT_HIT_BY_BOMB);
                explosion.Parent.AgentFitness.Fitness.UpdateFitness(BombermanFitness.BOMB_HIT_AGENT);
            }

            if (agent.Health <= 0) {
                // Agent has died
                if (this == explosion.Parent) {
                    agent.AgentFitness.Fitness.UpdateFitness(BombermanFitness.AGENT_HIT_BY_OWN_BOMB);
                }
                else {
                    agent.AgentFitness.Fitness.UpdateFitness(BombermanFitness.AGENT_HEALTH_ZERO);
                    explosion.Parent.AgentFitness.Fitness.UpdateFitness(BombermanFitness.AGENT_KILLED);
                }

                agent.DeathSequence();

                // Add survival bonuns to the existing agents
                AddSurvivalFitnessBonus();
            }
        }
    }

    public void MoveAgents(AgentComponent[] agents) {
        foreach (BombermanAgentComponent agent in agents) {
            if (agent.gameObject.activeSelf && agent.enabled) {

                if (DiscreteAgentMovement) {
                    if (agent.NextAgentUpdateTime <= CurrentSimulationTime && agent.MoveDirection != Vector2.zero) {
                        if (AgentCanMove(agent)) {
                            agent.transform.Translate(new Vector3(agent.MoveDirection.x, agent.MoveDirection.y, 0));
                            agent.NextAgentUpdateTime = CurrentSimulationTime + AgentUpdateinterval;
                            CheckIfAgentOverPowerUp(agent);
                        }
                        else {
                            agent.SetDirection(Vector2.zero, agent.SpriteRendererDown);
                        }
                    }
                }
                else {
                    // TODO ?
                    /*Vector2 position = agent.Rigidbody.position;
                    Vector2 translation = agent.MoveDirection * agent.MoveSpeed * Time.fixedDeltaTime;

                    agent.Rigidbody.MovePosition(position + translation);*/
                }
            }
        }
    }

    bool AgentCanMove(BombermanAgentComponent agent) {
        Vector3 newPos = agent.transform.position + new Vector3(agent.MoveDirection.x, agent.MoveDirection.y, 0);
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(newPos, new Vector2(1 / 1.15f, 1 / 1.15f), 0f);

        foreach(Collider2D collider in hitColliders) {
            BombComponent bombComponent = collider.GetComponent<BombComponent>();
            if(bombComponent != null) {
                return BombMove(bombComponent, bombComponent.transform.position, agent.MoveDirection, 0) ? true : false;
            }
            else if(!collider.isTrigger)
                return false;
        }

        return true;
    }

    bool BombMove(BombComponent bomb, Vector3 pos, Vector2 moveDirection, int deep) {
        Vector3 newPos = pos + new Vector3(moveDirection.x, moveDirection.y, 0);
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(newPos, new Vector2(1 / 1.15f, 1 / 1.15f), 0f);
        if(hitColliders.Length > 0) {
            return deep > 0? true : false;
        }
        else {
            bomb.transform.Translate(moveDirection);
            return BombMove(bomb, newPos, moveDirection, deep + 1);
        }
    }

    void CheckIfAgentOverPowerUp(BombermanAgentComponent agent) {
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(agent.transform.position, new Vector2(1 / 1.15f, 1 / 1.15f), 0f);

        foreach (Collider2D collider in hitColliders) {
            ItemPickupComponent itemPickup = collider.GetComponent<ItemPickupComponent>();
            if(itemPickup != null) {
                itemPickup.OnItemPickup(agent);
                return;
            }
        }
    }

    public void UpdateAgentsWithBTs(AgentComponent[] agents) {
        ActionBuffers actionBuffers;
        foreach(BombermanAgentComponent agent in agents) { 
            if (agent.gameObject.activeSelf && agent.enabled) {
                actionBuffers = new ActionBuffers(null, new int[] { 0, 0, 0 }); // Forward, Side, Place bomb

                agent.BehaviourTree.UpdateTree(actionBuffers);

                MoveAgent(agent, actionBuffers);
                PlaceBomb(agent, actionBuffers);
            }
        }
    }

    public void MoveAgent(BombermanAgentComponent agent, ActionBuffers actionBuffers) {

        // Set direction
        var verticalAxis = actionBuffers.DiscreteActions[0];
        var horizontalAxis = actionBuffers.DiscreteActions[1];

        switch (verticalAxis) {
            case 1:
                agent.SetDirection(Vector2.up, agent.SpriteRendererUp);
                break;
            case 2:
                agent.SetDirection(Vector2.down, agent.SpriteRendererDown);
                break;
        }
        switch (horizontalAxis) {
            case 1:
                agent.SetDirection(Vector2.left, agent.SpriteRendererLeft);
                break;
            case 2:
                agent.SetDirection(Vector2.right, agent.SpriteRendererRight);
                break;
        }

        if (DiscreteAgentMovement) {
            if (agent.NextAgentUpdateTime <= CurrentSimulationTime && agent.MoveDirection != Vector2.zero) {
                if (AgentCanMove(agent)) {
                    agent.transform.Translate(new Vector3(agent.MoveDirection.x, agent.MoveDirection.y, 0));
                    agent.NextAgentUpdateTime = CurrentSimulationTime + AgentUpdateinterval;
                    CheckIfAgentOverPowerUp(agent);
                }
                else {
                    agent.SetDirection(Vector2.zero, agent.SpriteRendererDown);
                }
            }
        }
        else {
            // Move agent TODO ?
            /*Vector2 position = agent.Rigidbody.position;
            Vector2 translation = agent.MoveDirection * agent.MoveSpeed * Time.fixedDeltaTime;

            agent.Rigidbody.MovePosition(position + translation);*/
        }
    }

    public void PlaceBomb(BombermanAgentComponent agent, ActionBuffers actionBuffers) {
        if (agent.BombsRemaining > 0 && actionBuffers.DiscreteActions[2] == 1) {
            StartCoroutine(BombExplosionController.PlaceBomb(agent));
        }
    }


}
