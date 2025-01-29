using Base;
using Configuration;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

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

        [SerializeField] public float BombFuseTime = 3f;
        [SerializeField] public float ExplosionDuration = 1f;

        [HideInInspector] public BombExplosionController BombExplosionController { get; set; }

        [HideInInspector] public Tilemap IndestructibleWalkableTiles { get; set; }

        Collider2D[] hitColliders;
        Vector3 newBombPos;
        Vector3 newAgentPos;
        ItemPickupComponent itemPickup;
        int sectorCount;
        int destructibleTilesCount;
        int opponentPlacedBombs;
        float allPossibleBombs;

        // Fitness calculation
        private float sectorExplorationFitness;
        private float bombsPlacedFitness;
        private float blocksDestroyedFitness;
        private float powerUpsCollectedFitness;
        private float bombsHitAgentFitness;
        private float agentsKilledFitness;
        private float agentHitByBombsFitness;
        private float agentHitByOwnBombsFitness;
        private float agentDeathFitness;
        private float survivalBonusFitness;
        private float lastSurvivalBonusFitness;


        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();
            BombExplosionController = GetComponent<BombExplosionController>();

            if (SceneLoadMode == SceneLoadMode.LayerMode)
            {
                // Only one problem environment exists
                IndestructibleWalkableTiles = FindObjectOfType<SectorComponent>().GetComponent<Tilemap>();
            }
            else
            {
                // Each EnvironmentController contains its own problem environment
                IndestructibleWalkableTiles = GetComponentInChildren<SectorComponent>().GetComponent<Tilemap>();
            }

            destructibleTilesCount = CountTiles(BombExplosionController.DestructibleTiles);
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

                if (conf.ProblemConfiguration.ContainsKey("BombFuseTime"))
                {
                    BombFuseTime = float.Parse(conf.ProblemConfiguration["BombFuseTime"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("ExplosionDuration"))
                {
                    ExplosionDuration = float.Parse(conf.ProblemConfiguration["ExplosionDuration"]);
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

                if (agent == explosion.Parent)
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
                    if (agent != explosion.Parent)
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

        public override void CheckEndingState()
        {
            if (GetNumOfActiveAgents() == 1)
            {
                FinishGame();
            }
        }

        protected override void OnPreFinishGame()
        {
            SetAgentsFitness();
        }

        private void SetAgentsFitness()
        {
            sectorCount = CountTiles(IndestructibleWalkableTiles);

            foreach (BombermanAgentComponent agent in Agents)
            {
                // Sector exploration
                sectorExplorationFitness = agent.SectorsExplored / (float)sectorCount;
                sectorExplorationFitness = (float)Math.Round(BombermanFitness.FitnessValues[BombermanFitness.FitnessKeys.SectorExploration.ToString()] * sectorExplorationFitness, 4);
                agent.AgentFitness.UpdateFitness(sectorExplorationFitness, BombermanFitness.FitnessKeys.SectorExploration.ToString());

                // Bombs placed
                allPossibleBombs = (CurrentSimulationSteps * Time.fixedDeltaTime) / BombFuseTime;
                bombsPlacedFitness = agent.BombsPlaced / allPossibleBombs;
                if (bombsPlacedFitness > 1)
                    bombsPlacedFitness = 1;
                bombsPlacedFitness = (float)Math.Round(BombermanFitness.FitnessValues[BombermanFitness.FitnessKeys.BombsPlaced.ToString()] * bombsPlacedFitness, 4);
                agent.AgentFitness.UpdateFitness(bombsPlacedFitness, BombermanFitness.FitnessKeys.BombsPlaced.ToString());

                // Blocks destroyed
                blocksDestroyedFitness = agent.BlocksDestroyed / (float)destructibleTilesCount;
                blocksDestroyedFitness = (float)Math.Round(BombermanFitness.FitnessValues[BombermanFitness.FitnessKeys.BlockDestroyed.ToString()] * blocksDestroyedFitness, 4);
                agent.AgentFitness.UpdateFitness(blocksDestroyedFitness, BombermanFitness.FitnessKeys.BlockDestroyed.ToString());

                // Power ups collected
                powerUpsCollectedFitness = agent.PowerUpsCollected / (float)(destructibleTilesCount * PowerUpSpawnChance);
                if(powerUpsCollectedFitness > 1) powerUpsCollectedFitness = 1;
                powerUpsCollectedFitness = (float)Math.Round(BombermanFitness.FitnessValues[BombermanFitness.FitnessKeys.PowerUpsCollected.ToString()] * powerUpsCollectedFitness, 4);
                agent.AgentFitness.UpdateFitness(powerUpsCollectedFitness, BombermanFitness.FitnessKeys.PowerUpsCollected.ToString());

                // Bombs hit agent
                opponentPlacedBombs = Agents.Select(x => (x as BombermanAgentComponent).BombsPlaced).Sum() - agent.BombsPlaced;
                if (opponentPlacedBombs > 0)
                {
                    bombsHitAgentFitness = agent.BombsHitAgent / (float)opponentPlacedBombs;
                    bombsHitAgentFitness = (float)Math.Round(BombermanFitness.FitnessValues[BombermanFitness.FitnessKeys.BombsHitAgent.ToString()] * bombsHitAgentFitness, 4);
                    agent.AgentFitness.UpdateFitness(bombsHitAgentFitness, BombermanFitness.FitnessKeys.BombsHitAgent.ToString());
                }

                // Agents killed
                agentsKilledFitness = agent.AgentsKilled / (float)(Agents.Length -1);
                agentsKilledFitness = (float)Math.Round(BombermanFitness.FitnessValues[BombermanFitness.FitnessKeys.AgentsKilled.ToString()] * agentsKilledFitness, 4);
                agent.AgentFitness.UpdateFitness(agentsKilledFitness, BombermanFitness.FitnessKeys.AgentsKilled.ToString());

                // Agent hit by bombs
                if (opponentPlacedBombs > 0)
                {
                    agentHitByBombsFitness = agent.AgentHitByBombs / (float)opponentPlacedBombs;
                    agentHitByBombsFitness = (float)Math.Round(BombermanFitness.FitnessValues[BombermanFitness.FitnessKeys.AgentHitByBombs.ToString()] * agentHitByBombsFitness, 4);
                    agent.AgentFitness.UpdateFitness(agentHitByBombsFitness, BombermanFitness.FitnessKeys.AgentHitByBombs.ToString());
                }

                // Agent hit by own bombs
                if (agent.AgentHitByOwnBombs > 0 && agent.BombsPlaced > 0)
                {
                    agentHitByOwnBombsFitness = agent.AgentHitByOwnBombs / (float)agent.BombsPlaced;
                    agentHitByOwnBombsFitness = (float)Math.Round(BombermanFitness.FitnessValues[BombermanFitness.FitnessKeys.AgentHitByOwnBombs.ToString()] * agentHitByOwnBombsFitness, 4);
                    agent.AgentFitness.UpdateFitness(agentHitByOwnBombsFitness, BombermanFitness.FitnessKeys.AgentHitByOwnBombs.ToString());
                }

                // Agent death
                if (agent.AgentDied)
                {
                    agentDeathFitness = BombermanFitness.FitnessValues[BombermanFitness.FitnessKeys.AgentDeath.ToString()];
                    agent.AgentFitness.UpdateFitness(agentDeathFitness, BombermanFitness.FitnessKeys.AgentDeath.ToString());
                }

                // Survival bonus
                if(agent.SurvivalBonuses > 0)
                {
                    survivalBonusFitness = agent.SurvivalBonuses / (float)(Agents.Length - 1);
                    survivalBonusFitness = (float)Math.Round(BombermanFitness.FitnessValues[BombermanFitness.FitnessKeys.SurvivalBonus.ToString()] * survivalBonusFitness, 4);
                    agent.AgentFitness.UpdateFitness(survivalBonusFitness, BombermanFitness.FitnessKeys.SurvivalBonus.ToString());
                }

                // Last survival bonus
                if (agent.LastSurvivalBonus)
                {
                    lastSurvivalBonusFitness = BombermanFitness.FitnessValues[BombermanFitness.FitnessKeys.LastSurvivalBonus.ToString()];
                    agent.AgentFitness.UpdateFitness(lastSurvivalBonusFitness, BombermanFitness.FitnessKeys.LastSurvivalBonus.ToString());
                }

                Debug.Log("========================================");
                Debug.Log("Agent: Team ID" + agent.TeamID + ", ID: " + agent.IndividualID);
                Debug.Log("Sectors explored: " + agent.SectorsExplored + " / " + sectorCount + "= " + sectorExplorationFitness);
                Debug.Log("Bombs placed: " + agent.BombsPlaced + " / " + allPossibleBombs + "= " + bombsPlacedFitness);
                Debug.Log("Blocks destroyed: " + agent.BlocksDestroyed + " / " + destructibleTilesCount + "= " + blocksDestroyedFitness);
                Debug.Log("Power ups collected: " + agent.PowerUpsCollected + " / " + (destructibleTilesCount * PowerUpSpawnChance) + "= " + powerUpsCollectedFitness);
                Debug.Log("Bombs hit agent: " + agent.BombsHitAgent + " / " + opponentPlacedBombs + "= " + bombsHitAgentFitness);
                Debug.Log("Agents killed: " + agent.AgentsKilled + "= " + agentsKilledFitness);
                Debug.Log("Agent hit by bombs: " + agent.AgentHitByBombs + " / " + opponentPlacedBombs + "= " + agentHitByBombsFitness);
                Debug.Log("Agent hit by own bombs: " + agent.AgentHitByOwnBombs + " / " + agent.BombsPlaced + "= " + agentHitByOwnBombsFitness);
                Debug.Log("Agent death: " + agent.AgentDied + "= " + agentDeathFitness);
                Debug.Log("Survival bonus: " + agent.SurvivalBonuses + " / " + (Agents.Length - 1) + "= " + survivalBonusFitness);
                Debug.Log("Last survival bonus: " + agent.LastSurvivalBonus + "= " + lastSurvivalBonusFitness);
            }
        }

        int CountTiles(Tilemap tilemap)
        {
            if (tilemap == null) return 0;

            int count = 0;
            BoundsInt bounds = tilemap.cellBounds;

            foreach (Vector3Int pos in bounds.allPositionsWithin)
            {
                if (tilemap.HasTile(pos)) // Check if there's a tile at this position
                {
                    count++;
                }
            }
            return count;
        }
    }
}
