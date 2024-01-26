using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BombermanEnvironmentController: EnvironmentControllerBase {

    [Header("Bomberman Agent configuration")]
    [SerializeField] float AgentStartMoveSpeed = 5f;
    [SerializeField] int StartAgentBombAmout = 1;
    [SerializeField] int StartExplosionRadius = 1;
    [SerializeField] int StartHealth = 1;
    [SerializeField] public float ExplosionDamageCooldown = 0.8f;

    private BombExplosionController BombExplosionController { get; set; }

    protected override void DefineAdditionalDataOnAwake() {
        BombExplosionController = GetComponent<BombExplosionController>();
    }

    protected override void DefineAdditionalDataOnStart() {
        foreach (BombermanAgentComponent agent in Agents) {
            agent.BombermanEnvironmentController = this;
            agent.Rigidbody = agent.GetComponent<Rigidbody2D>();
            agent.MoveSpeed = AgentStartMoveSpeed;
            agent.Health = StartHealth;
            agent.ExplosionRadius = StartExplosionRadius;
            agent.SetBombs(StartAgentBombAmout);
        }
    }

    protected override void OnUpdate() {
        foreach (BombermanAgentComponent agent in Agents) {
            if (agent.gameObject.activeSelf) {
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

    public override void UpdateAgents() {
        MoveAgents();

        //MoveAgentsWithBehaviourTrees();
    }

    public void AddSurvivalFitnessBonus() {
        bool lastSurvival = GetNumOfActiveAgents() > 1 ? false : true;
        // Survival bonus
        foreach (var agent in Agents) {
            if (agent.gameObject.activeSelf) {
                agent.AgentFitness.Fitness.UpdateFitness(BombermanFitness.SURVIVAL_BONUS);

                // Last survival bonus
                if (lastSurvival) {
                    agent.AgentFitness.Fitness.UpdateFitness(BombermanFitness.LAST_SURVIVAL_BONUS);
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

    public void MoveAgents() {
        foreach(BombermanAgentComponent agent in Agents) {
            if (agent.gameObject.activeSelf) {
                Vector2 position = agent.Rigidbody.position;
                Vector2 translation = agent.MoveDirection * agent.MoveSpeed * Time.fixedDeltaTime;

                agent.Rigidbody.MovePosition(position + translation);
            }
        }
    }


}
