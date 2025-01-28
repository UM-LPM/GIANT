using AgentControllers;
using Base;
using Configuration;
using UnityEngine;

namespace Problems.Bomberman
{
    public class BombermanEnvironmentController : EnvironmentControllerBase
    {
        [Header("Bomberman Agent configuration")]
        [SerializeField] float AgentStartMoveSpeed = 5f;
        [SerializeField] int StartAgentBombAmount = 1;
        [SerializeField] int StartExplosionRadius = 1;
        [SerializeField] int StartHealth = 1;
        [SerializeField] public float ExplosionDamageCooldown = 0.8f;

        [Header("Descrete Agent Movement Configuration")]
        [SerializeField] public float AgentUpdateinterval = 0.1f;

        [Header("Bomberman Game Configuration")]
        [SerializeField] public float DestructibleDestructionTime = 1f;
        [Range(0f, 1f)]
        [SerializeField] public float PowerUpSpawnChance = 0.4f;
        [SerializeField] public GameObject[] SpawnableItems;

        [HideInInspector] public BombExplosionController BombExplosionController { get; set; }

        Collider2D[] hitColliders;
        Vector3 newBombPos;
        Vector3 newAgentPos;
        ItemPickupComponent itemPickup;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();
            BombExplosionController = GetComponent<BombExplosionController>();
        }

        protected override void DefineAdditionalDataOnPostStart()
        {
            SetAgentStartParams(Agents);
        }

        void ReadParamsFromMainConfiguration()
        {
            if (MenuManager.Instance != null && MenuManager.Instance.MainConfiguration != null)
            {
                MainConfiguration conf = MenuManager.Instance.MainConfiguration;

                BombermanFitness.FitnessValues = conf.FitnessValues;

                if (conf.ProblemConfiguration.ContainsKey("DecisionRequestInterval"))
                {
                    DecisionRequestInterval = int.Parse(conf.ProblemConfiguration["DecisionRequestInterval"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentStartMoveSpeed"))
                {
                    AgentStartMoveSpeed = float.Parse(conf.ProblemConfiguration["AgentStartMoveSpeed"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("StartAgentBombAmount"))
                {
                    StartAgentBombAmount = int.Parse(conf.ProblemConfiguration["StartAgentBombAmount"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("StartExplosionRadius"))
                {
                    StartExplosionRadius = int.Parse(conf.ProblemConfiguration["StartExplosionRadius"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("StartHealth"))
                {
                    StartHealth = int.Parse(conf.ProblemConfiguration["StartHealth"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("ExplosionDamageCooldown"))
                {
                    ExplosionDamageCooldown = float.Parse(conf.ProblemConfiguration["ExplosionDamageCooldown"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("DestructibleDestructionTime"))
                {
                    DestructibleDestructionTime = float.Parse(conf.ProblemConfiguration["DestructibleDestructionTime"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("PowerUpSpawnChance"))
                {
                    PowerUpSpawnChance = float.Parse(conf.ProblemConfiguration["PowerUpSpawnChance"]);
                }
            }
        }

        void SetAgentStartParams(AgentComponent[] agents)
        {
            foreach (BombermanAgentComponent agent in agents)
            {
                agent.SetStartParams(this, AgentStartMoveSpeed, StartHealth, StartExplosionRadius, StartAgentBombAmount);
            }
        }
        public bool AgentCanMove(BombermanAgentComponent agent)
        {
            newAgentPos = agent.transform.position + new Vector3(agent.MoveDirection.x, agent.MoveDirection.y, 0);
            hitColliders = Physics2D.OverlapBoxAll(newAgentPos, new Vector2(1 / 1.15f, 1 / 1.15f), 0f, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)) + DefaultLayer);

            foreach (Collider2D collider in hitColliders)
            {
                BombComponent bombComponent = collider.GetComponent<BombComponent>();
                if (bombComponent != null)
                {
                    return MoveBomb(bombComponent, bombComponent.transform.position, agent.MoveDirection, 0) ? true : false;
                }
                else if (!collider.isTrigger)
                    return false;
            }

            return true;
        }

        bool MoveBomb(BombComponent bomb, Vector3 pos, Vector2 moveDirection, int deep)
        {
            newBombPos = pos + new Vector3(moveDirection.x, moveDirection.y, 0);
            hitColliders = Physics2D.OverlapBoxAll(newBombPos, new Vector2(1 / 1.15f, 1 / 1.15f), 0f, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)) + DefaultLayer);
            if (hitColliders.Length > 0)
            {
                return deep > 0 ? true : false;
            }
            else
            {
                bomb.transform.Translate(moveDirection);
                return MoveBomb(bomb, newBombPos, moveDirection, deep + 1);
            }
        }

        public void CheckIfAgentOverPowerUp(BombermanAgentComponent agent)
        {
            hitColliders = Physics2D.OverlapBoxAll(agent.transform.position, new Vector2(1 / 1.15f, 1 / 1.15f), 0f, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)) + DefaultLayer);

            foreach (Collider2D collider in hitColliders)
            {
                itemPickup = collider.GetComponent<ItemPickupComponent>();
                if (itemPickup != null)
                {
                    itemPickup.OnItemPickup(agent);
                    return;
                }
            }
        }

        public void ExlosionHitAgent(BombermanAgentComponent agent, ExplosionComponent explosion)
        {
            if (agent.NextDamageTime <= CurrentSimulationTime)
            {
                agent.NextDamageTime = CurrentSimulationTime + ExplosionDamageCooldown;

                agent.Health--;

                if (this == explosion.Parent)
                {
                    agent.AgentHitByOwnBombs++;
                }
                else
                {
                    agent.AgentHitByBombs++;
                    explosion.Parent.BombsHitAgent++;
                }

                if (agent.Health <= 0)
                {
                    // Agent has died
                    if (this == explosion.Parent)
                    {
                        agent.AgentHitByOwnBombs++;
                    }
                    else
                    {
                        agent.AgentDied = true;
                        explosion.Parent.AgentsKilled++;
                    }

                    agent.DeathSequence();

                    // Add survival bonuns to the existing agents
                    AddSurvivalFitnessBonus();
                }
            }
        }

        public void AddSurvivalFitnessBonus()
        {
            bool lastSurvival = GetNumOfActiveAgents() > 1 ? false : true;
            // Survival bonus
            foreach (BombermanAgentComponent agent in Agents)
            {
                if (agent.gameObject.activeSelf && agent.enabled)
                {
                    agent.SurvivalBonuses++;

                    // Last survival bonus
                    if (lastSurvival)
                    {
                        agent.LastSurvivalBonus = true;
                    }
                }
            }

        }

        protected override void OnPostFixedUpdate()
        {
            AgentsOverExplosion();
        }

        void AgentsOverExplosion()
        {
            foreach (BombermanAgentComponent agent in Agents)
            {
                if (agent.gameObject.activeSelf && agent.enabled)
                {
                    Collider2D[] hitColliders = Physics2D.OverlapBoxAll(agent.transform.position, new Vector2(1 / 1.15f, 1 / 1.15f), 0f, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)) + DefaultLayer);

                    foreach (Collider2D collider in hitColliders)
                    {
                        ExplosionComponent explosion = collider.GetComponent<ExplosionComponent>();
                        if (explosion != null)
                        {
                            ExlosionHitAgent(agent, explosion);
                            break;
                        }
                    }
                }
            }
        }
    }
}
