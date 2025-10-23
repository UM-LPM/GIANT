using Base;
using Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Utils;

namespace Problems.BombClash
{
    public class BombermanEnvironmentController : EnvironmentControllerBase
    {
        [Header("Bomberman Agent configuration")]
        [SerializeField] int AgentStartMoveSpeed = 5;
        [SerializeField] int StartAgentBombAmount = 1;
        [SerializeField] int StartExplosionRadius = 1;
        [SerializeField] int StartHealth = 1;
        [SerializeField] public int ExplosionDamageCooldown = 40; //0.8f;

        [Header("Descrete Agent Movement Configuration")]
        [SerializeField] public int AgentUpdateinterval = 10; //0.2f;

        [Header("Bomberman Game Configuration")]
        [SerializeField] public int DestructibleDestructionTime = 50; //1f;
        [Range(0f, 1f)]
        [SerializeField] public float PowerUpSpawnChance = 0.4f;
        [SerializeField] public GameObject[] SpawnableItems;

        [Header("Bomb")]
        [SerializeField] BombComponent BombPrefab;
        [SerializeField] public int BombFuseTime = 150; //3f;

        [Header("Explosion")]
        [SerializeField] ExplosionComponent ExplosionPrefab;
        [SerializeField] public int ExplosionDuration = 50; //1f;

        [Header("Destructible")]
        [SerializeField] public Tilemap DestructibleTiles;
        [SerializeField] DestructibleComponent DestructiblePrefab;

        [HideInInspector] public Tilemap IndestructibleWalkableTiles { get; set; }

        private List<ActiveBomb> ActiveBombs;
        private List<ActiveExplosion> ActiveExplosions;
        private List<ActiveDestructible> ActiveDestructibles; 

        Collider2D[] hitColliders;
        Vector3 newBombPos;
        Vector3 newAgentPos;
        ItemPickupComponent itemPickup;
        int sectorCount;
        int destructibleTilesCount;
        int opponentPlacedBombs;
        float allPossibleBombs;

        ContactFilter2D contactFilter;

        bool allOpponentsDestroyed;

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

        private string agentFitnessLog;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();
            IndestructibleWalkableTiles = GetComponentInChildren<SectorComponent>().GetComponent<Tilemap>();

            destructibleTilesCount = CountTiles(DestructibleTiles);

            ActiveBombs = new List<ActiveBomb>();
            ActiveExplosions = new List<ActiveExplosion>();
            ActiveDestructibles = new List<ActiveDestructible>();
        }

        protected override void DefineAdditionalDataOnPostStart()
        {
            SetAgentStartParams(Agents);

            contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)));

            hitColliders = new Collider2D[32];
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
                    AgentStartMoveSpeed = int.Parse(conf.ProblemConfiguration["AgentStartMoveSpeed"]);
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
                    ExplosionDamageCooldown = int.Parse(conf.ProblemConfiguration["ExplosionDamageCooldown"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("DestructibleDestructionTime"))
                {
                    DestructibleDestructionTime = int.Parse(conf.ProblemConfiguration["DestructibleDestructionTime"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("PowerUpSpawnChance"))
                {
                    PowerUpSpawnChance = float.Parse(conf.ProblemConfiguration["PowerUpSpawnChance"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("BombFuseTime"))
                {
                    BombFuseTime = int.Parse(conf.ProblemConfiguration["BombFuseTime"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("ExplosionDuration"))
                {
                    ExplosionDuration = int.Parse(conf.ProblemConfiguration["ExplosionDuration"]);
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
            newAgentPos = agent.transform.position + new Vector3Int(agent.MoveDirection.x, agent.MoveDirection.y, 0);
            hitColliders = PhysicsUtil.PhysicsOverlapBox2D(PhysicsScene2D, agent.gameObject, newAgentPos, agent.transform.rotation, new Vector2(0.87f, 0.87f), false, gameObject.layer);

            foreach (Collider2D collider in hitColliders)
            {
                if(collider == null) continue;

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

        bool MoveBomb(BombComponent bomb, Vector3 pos, Vector2Int moveDirection, int deep)
        {
            newBombPos = pos + new Vector3Int(moveDirection.x, moveDirection.y, 0);
            hitColliders = PhysicsUtil.PhysicsOverlapBox2D(PhysicsScene2D, bomb.gameObject, newBombPos, bomb.transform.rotation, new Vector2(0.87f, 0.87f), false, gameObject.layer);
            
            if (hitColliders.Length > 0)
            {
                return deep > 0 ? true : false;
            }
            else
            {
                bomb.transform.position = newBombPos;

                return MoveBomb(bomb, newBombPos, moveDirection, deep + 1);
            }
        }

        public void CheckIfAgentOverPowerUp(BombermanAgentComponent agent)
        {
            hitColliders = PhysicsUtil.PhysicsOverlapBox2D(PhysicsScene2D, agent.gameObject, agent.transform.position, agent.transform.rotation, new Vector2(0.87f, 0.87f), false, gameObject.layer);
            
            foreach (Collider2D collider in hitColliders)
            {
                if(collider == null) continue;

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
            if (agent.NextDamageTime <= CurrentSimulationSteps)
            {
                agent.NextDamageTime = CurrentSimulationSteps + ExplosionDamageCooldown;

                agent.Health--;

                if (agent == explosion.Owener)
                {
                    agent.AgentHitByOwnBombs++;
                }
                else
                {
                    agent.AgentHitByBombs++;
                    explosion.Owener.BombsHitAgent++;
                }

                if (agent.Health <= 0)
                {
                    // Agent has died
                    if (agent != explosion.Owener)
                    {
                        agent.AgentDied = true;
                        explosion.Owener.AgentsKilled++;
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

            // Handle active bombs
            for (int i = ActiveBombs.Count - 1; i >= 0; i--)
            {
                if (ActiveBombs[i].DecreaseTime())
                {
                    InitExplosion(ActiveBombs[i]);

                }
            }
            // Handle active explosions
            for (int i = ActiveExplosions.Count - 1; i >= 0; i--)
            {
                if (ActiveExplosions[i].DecreaseDuration())
                {
                    Destroy(ActiveExplosions[i].Explosion.gameObject);
                    ActiveExplosions.RemoveAt(i);
                }
            }

            // Handle active destructibles
            for (int i = ActiveDestructibles.Count - 1; i >= 0; i--)
            {
                if (ActiveDestructibles[i].DecreaseTime())
                {
                    SpawnPowerUpItem(ActiveDestructibles[i]);
                    Destroy(ActiveDestructibles[i].DestructibleComponent.gameObject);
                    ActiveDestructibles.RemoveAt(i);
                }
            }
        }

        void AgentsOverExplosion()
        {
            foreach (BombermanAgentComponent agent in Agents)
            {
                if (agent.gameObject.activeSelf && agent.enabled)
                {
                    hitColliders = PhysicsUtil.PhysicsOverlapBox2D(PhysicsScene2D, agent.gameObject, agent.transform.position, agent.transform.rotation, new Vector2(0.87f, 0.87f), false, gameObject.layer);

                    foreach (Collider2D collider in hitColliders)
                    {
                        if(collider == null) continue;

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
            if (GetNumOfActiveAgents() <= 1)
            {
                allOpponentsDestroyed = true;
            }
        }

        public override bool IsSimulationFinished()
        {
            return base.IsSimulationFinished() || allOpponentsDestroyed;
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
                else
                {
                    bombsHitAgentFitness = 0f;
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
                else
                {
                    agentHitByBombsFitness = 0f;
                }

                // Agent hit by own bombs
                if (agent.AgentHitByOwnBombs > 0 && agent.BombsPlaced > 0)
                {
                    agentHitByOwnBombsFitness = agent.AgentHitByOwnBombs / (float)agent.BombsPlaced;
                    agentHitByOwnBombsFitness = (float)Math.Round(BombermanFitness.FitnessValues[BombermanFitness.FitnessKeys.AgentHitByOwnBombs.ToString()] * agentHitByOwnBombsFitness, 4);
                    agent.AgentFitness.UpdateFitness(agentHitByOwnBombsFitness, BombermanFitness.FitnessKeys.AgentHitByOwnBombs.ToString());
                }
                else
                {
                    agentHitByOwnBombsFitness = 0f;
                }

                // Agent death
                if (agent.AgentDied)
                {
                    agentDeathFitness = BombermanFitness.FitnessValues[BombermanFitness.FitnessKeys.AgentDeath.ToString()];
                    agent.AgentFitness.UpdateFitness(agentDeathFitness, BombermanFitness.FitnessKeys.AgentDeath.ToString());
                }
                else
                {
                    agentDeathFitness = 0f;
                }

                // Survival bonus
                if (agent.SurvivalBonuses > 0)
                {
                    survivalBonusFitness = agent.SurvivalBonuses / (float)(Agents.Length - 1);
                    survivalBonusFitness = (float)Math.Round(BombermanFitness.FitnessValues[BombermanFitness.FitnessKeys.SurvivalBonus.ToString()] * survivalBonusFitness, 4);
                    agent.AgentFitness.UpdateFitness(survivalBonusFitness, BombermanFitness.FitnessKeys.SurvivalBonus.ToString());
                }
                else
                {
                    survivalBonusFitness = 0;
                }

                // Last survival bonus
                if (agent.LastSurvivalBonus)
                {
                    lastSurvivalBonusFitness = BombermanFitness.FitnessValues[BombermanFitness.FitnessKeys.LastSurvivalBonus.ToString()];
                    agent.AgentFitness.UpdateFitness(lastSurvivalBonusFitness, BombermanFitness.FitnessKeys.LastSurvivalBonus.ToString());
                }
                else
                {
                    lastSurvivalBonusFitness = 0f;
                }

                    agentFitnessLog = "========================================\n" +
                                      $"[Agent]: Team ID {agent.TeamIdentifier.TeamID}, ID: {agent.IndividualID}\n" +
                                      $"Sectors explored: {agent.SectorsExplored} / {sectorCount} = {sectorExplorationFitness}\n" +
                                      $"Bombs placed: {agent.BombsPlaced} / {allPossibleBombs} = {bombsPlacedFitness}\n" +
                                      $"Blocks destroyed: {agent.BlocksDestroyed} / {destructibleTilesCount} = {blocksDestroyedFitness}\n" +
                                      $"Power ups collected: {agent.PowerUpsCollected} / {(destructibleTilesCount * PowerUpSpawnChance)} = {powerUpsCollectedFitness}\n" +
                                      $"Bombs hit agent: {agent.BombsHitAgent} / {opponentPlacedBombs} = {bombsHitAgentFitness}\n" +
                                      $"Agents killed: {agent.AgentsKilled} = {agentsKilledFitness}\n" +
                                      $"Agent hit by bombs: {agent.AgentHitByBombs} / {opponentPlacedBombs} = {agentHitByBombsFitness}\n" +
                                      $"Agent hit by own bombs: {agent.AgentHitByOwnBombs} / {agent.BombsPlaced} = {agentHitByOwnBombsFitness}\n" +
                                      $"Agent death: {agent.AgentDied} = {agentDeathFitness}\n" +
                                      $"Survival bonus: {agent.SurvivalBonuses} / {(Agents.Length - 1)} = {survivalBonusFitness}\n" +
                                      $"Last survival bonus: {agent.LastSurvivalBonus} = {lastSurvivalBonusFitness}";

                DebugSystem.LogVerbose(agentFitnessLog);
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

        public void PlaceBomb(BombermanAgentComponent agent)
        {
            Vector2 position = agent.transform.position;

            // Check if the agent can place a bomb (No bomb is already present at this position
            var existingBomb = PhysicsUtil.PhysicsOverlapBoxTargetObject<BombComponent>(
                PhysicsScene,
                PhysicsScene2D,
                GameType,
                agent.gameObject,
                position,
                Quaternion.identity,
                new Vector2(0.5f, 0.5f),
                false,
                gameObject.layer
                );

            if (existingBomb != null || agent.BombsRemaining <= 0)
                return;

            agent.BombsPlaced++;

            BombComponent bomb = Instantiate(BombPrefab, position, Quaternion.identity, transform);

            SetLayerRecursively(bomb.gameObject, gameObject.layer);
            agent.BombsRemaining--;

            ActiveBombs.Add(new ActiveBomb(bomb, agent, BombFuseTime));
        }

        public void InitExplosion(ActiveBomb activeBomb)
        {
            Vector2 position = activeBomb.Bomb.transform.position;

            // Handle explosion logic
            ExplosionComponent explosion = Instantiate(ExplosionPrefab, position, Quaternion.identity, transform);
            explosion.Owener = activeBomb.Owner;
            SetLayerRecursively(explosion.gameObject, gameObject.layer);

            explosion.SetActiveRenderer(explosion.Start);
            ActiveExplosions.Add(new ActiveExplosion(explosion, ExplosionDuration));

            Explode(position, Vector2.up, activeBomb.Owner.ExplosionRadius, activeBomb.Owner);
            Explode(position, Vector2.down, activeBomb.Owner.ExplosionRadius, activeBomb.Owner);
            Explode(position, Vector2.left, activeBomb.Owner.ExplosionRadius, activeBomb.Owner);
            Explode(position, Vector2.right, activeBomb.Owner.ExplosionRadius, activeBomb.Owner);

            Destroy(activeBomb.Bomb.gameObject);
            activeBomb.Owner.BombsRemaining++;

            ActiveBombs.Remove(activeBomb);
        }

        public void Explode(Vector2 position, Vector2 direction, int length, BombermanAgentComponent agent)
        {
            if (length <= 0)
                return;

            position += direction;

            Collider2D[] colliders = PhysicsUtil.PhysicsOverlapBox2D(PhysicsScene2D, null, position, Quaternion.identity, new Vector2(0.5f, 0.5f), true, gameObject.layer);

            foreach (var collider in colliders)
            {
                if (collider != null && !collider.isTrigger)
                {
                    BombermanAgentComponent hitAgent;
                    collider.TryGetComponent<BombermanAgentComponent>(out hitAgent);
                    if (!hitAgent)
                    {
                        ClearDestructible(position, agent);
                        return;
                    }
                }
            }

            ExplosionComponent explosion = Instantiate(ExplosionPrefab, position, Quaternion.identity, transform);
            explosion.Owener = agent;
            SetLayerRecursively(explosion.gameObject, gameObject.layer);

            explosion.SetActiveRenderer(length > 1 ? explosion.Middle : explosion.End);
            explosion.SetDirection(direction);

            ActiveExplosions.Add(new ActiveExplosion(explosion, ExplosionDuration));

            Explode(position, direction, length - 1, agent);
        }

        public void ClearDestructible(Vector2 position, BombermanAgentComponent agent)
        {
            Vector3Int cell = DestructibleTiles.WorldToCell(position);
            TileBase tile = DestructibleTiles.GetTile(cell);

            if (tile != null)
            {
                DestructibleComponent destructible = Instantiate(DestructiblePrefab, position, Quaternion.identity, transform);
                SetLayerRecursively(destructible.gameObject, gameObject.layer);

                DestructibleTiles.SetTile(cell, null);

                ActiveDestructibles.Add(new ActiveDestructible(destructible, DestructibleDestructionTime));

                agent.BlocksDestroyed++;
            }
        }

        public void SpawnPowerUpItem(ActiveDestructible activeDestructible)
        {
            // Spawn an item (power up)
            if (SpawnableItems.Length > 0 && Util.NextDouble() < PowerUpSpawnChance)
            {
                int index = Util.NextInt(0, SpawnableItems.Length);
                GameObject item = Instantiate(SpawnableItems[index], activeDestructible.DestructibleComponent.transform.position, Quaternion.identity, activeDestructible.DestructibleComponent.transform.parent);
                SetLayerRecursively(item, gameObject.layer);
            }
        }
    }
}
