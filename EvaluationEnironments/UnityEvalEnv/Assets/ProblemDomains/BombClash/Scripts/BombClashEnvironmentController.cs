using Base;
using Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

namespace Problems.BombClash
{
    public class BombClashEnvironmentController : EnvironmentControllerBase
    {
        [Header("Bomberman Agent configuration")]
        [SerializeField] public int AgentStartMoveSpeed = 5;
        [SerializeField] public int StartAgentBombAmount = 1;
        [SerializeField] public int StartExplosionRadius = 1;
        [SerializeField] public int StartHealth = 1;
        [SerializeField] public int ExplosionDamageCooldown = 40; //0.8f;

        [SerializeField] public int AgentUpdateinterval = 10; //0.2f;
        [SerializeField] public float MaxSpeedIncrease = 0.5f;

        [Header("Bomberman Game Configuration")]
        [SerializeField] public int DestructibleDestructionTime = 50; //1f;
        [Range(0f, 1f)]
        [SerializeField] public float PowerUpSpawnChance = 0.4f;
        [SerializeField] public ItemPickupComponent[] SpawnableItems;

        [Header("Bomb")]
        [SerializeField] BombComponent BombPrefab;
        [SerializeField] public int BombFuseTime = 150; //3f;

        [Header("Explosion")]
        [SerializeField] ExplosionComponent ExplosionPrefab;
        [SerializeField] public int ExplosionDuration = 50; //1f;

        [Header("Tiles")]
        [SerializeField] DestructibleComponent DestructiblePrefab;
        [SerializeField] public Tilemap DestructibleTiles;
        [SerializeField] public Tilemap IndestructibleTiles;
        [SerializeField] public Tilemap IndestructibleWalkableTiles;
        [SerializeField] public Vector2Int WorldMin = new Vector2Int(-7, -7);

        public BombClashGrid Grid { get; private set; }

        private List<ActiveBomb> ActiveBombs;
        private List<ActiveExplosion> ActiveExplosions;
        private List<ActiveDestructible> ActiveDestructibles;

        bool allOpponentsDestroyed;

        // Fitness calculation
        int sectorCount;
        int destructibleTilesCount;
        int opponentPlacedBombs;
        float allPossibleBombs;

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

            destructibleTilesCount = CountTiles(DestructibleTiles);

            ActiveBombs = new List<ActiveBomb>();
            ActiveExplosions = new List<ActiveExplosion>();
            ActiveDestructibles = new List<ActiveDestructible>();

            Grid = GetComponent<BombClashGrid>();

            Grid.InitGrid(IndestructibleTiles.cellBounds.size.x, IndestructibleTiles.cellBounds.size.y, WorldMin);

            for (int x = 0; x < Grid.Width; x++)
            {
                for (int y = 0; y < Grid.Width; y++)
                {
                    Vector3Int cellPos = new Vector3Int(Grid.WorldMin.x + x, Grid.WorldMin.y + y, 0);

                    if (IndestructibleTiles.HasTile(cellPos))
                    {
                        Grid.SetTile(x, y, TileType.Wall, null);
                    }
                    else if (DestructibleTiles.HasTile(cellPos))
                    {
                        Grid.SetTile(x, y, TileType.Destructible, null);
                    }
                    else
                    {
                        Grid.SetTile(x, y, TileType.Empty, null);
                    }
                }
            }
        }

        protected override void DefineAdditionalDataOnPostStart()
        {
            SetAgentStartParams(Agents);
        }

        void SetAgentStartParams(AgentComponent[] agents)
        {
            foreach (BombClashAgentComponent agent in agents)
            {
                agent.SetStartParams(this, AgentStartMoveSpeed, StartHealth, StartExplosionRadius, StartAgentBombAmount);
            }
        }

        public bool AgentCanMove(BombClashAgentComponent agent)
        {
            Vector2Int newAgentPos = agent.Position + agent.MoveDirection;
            Vector2Int gridPos = Grid.WorldToGrid(newAgentPos.x, newAgentPos.y);
            GridTile tile = Grid.GetTile(gridPos.x, gridPos.y);

            if (!tile.IsWalkable())
            {
                return false;
            }
            
            if(tile.TileType == TileType.Bomb && tile.Component is BombComponent)
            {
                var bombComponent = tile.Component as BombComponent;
                return MoveBomb(bombComponent, bombComponent.Position, agent.MoveDirection, 0) ? true : false;
            }

            return true;
        }

        bool MoveBomb(BombComponent bomb, Vector2Int pos, Vector2Int moveDirection, int deep)
        {
            Vector2Int newBombPos = pos + moveDirection;

            if (AnyAgentInPosition(newBombPos))
            {
                return deep > 0 ? true : false;
            }

            Vector2Int gridPosNew = Grid.WorldToGrid(newBombPos.x, newBombPos.y);
            GridTile tile = Grid.GetTile(gridPosNew.x, gridPosNew.y);

            if (tile.TileType != TileType.Empty)
            {
                return deep > 0 ? true : false;
            }

            Vector2Int gridPosOld = Grid.WorldToGrid(pos.x, pos.y);
            Grid.SetTile(gridPosOld.x, gridPosOld.y, TileType.Empty, null);
            
            bomb.Move(moveDirection.x, moveDirection.y);
            Grid.SetTile(gridPosNew.x, gridPosNew.y, TileType.Bomb, bomb);

            return MoveBomb(bomb, newBombPos, moveDirection, deep + 1);
        }

        public bool AnyAgentInPosition(Vector2Int position)
        {
            foreach(BombClashAgentComponent agent in Agents)
            {
                if (agent.Position == position)
                    return true;
            }

            return false;
        }

        public void CheckIfAgentOverPowerUp(BombClashAgentComponent agent)
        {
            Vector2Int gridPos = Grid.WorldToGrid(agent.Position.x, agent.Position.y);
            GridTile tile = Grid.GetTile(gridPos.x, gridPos.y);

            if(tile.TileType == TileType.PowerUp && tile.Component != null && tile.Component is ItemPickupComponent)
            {
                (tile.Component as ItemPickupComponent).OnItemPickup(agent);
                Grid.SetTile(gridPos.x, gridPos.y, TileType.Empty, null);
            }
        }

        public void ExlosionHitAgent(BombClashAgentComponent agent, ExplosionComponent explosion)
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
                    CheckEndingState();

                    // Add survival bonuns to the existing agents
                    AddSurvivalFitnessBonus();
                }
            }
        }

        public void AddSurvivalFitnessBonus()
        {
            bool lastSurvival = GetNumOfActiveAgents() > 1 ? false : true;
            // Survival bonus
            foreach (BombClashAgentComponent agent in Agents)
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
                    Vector2Int cell = Grid.WorldToGrid(ActiveExplosions[i].Explosion.Position.x, (int)ActiveExplosions[i].Explosion.Position.y);
                    Grid.SetTile(cell.x, cell.y, TileType.Empty, null);

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
            foreach (BombClashAgentComponent agent in Agents)
            {
                if (agent.gameObject.activeSelf && agent.enabled)
                {
                    Vector2Int gridPos = Grid.WorldToGrid(agent.Position.x, agent.Position.y);
                    GridTile tile = Grid.GetTile(gridPos.x, gridPos.y);

                    if (tile.TileType == TileType.Explosion && tile.Component != null && tile.Component is ExplosionComponent)
                    {
                        ExlosionHitAgent(agent, tile.Component as ExplosionComponent);
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

        public void PlaceBomb(BombClashAgentComponent agent)
        {
            Vector2Int position = agent.Position;

            Vector2Int gridPos = Grid.WorldToGrid(agent.Position.x, agent.Position.y);
            GridTile tile = Grid.GetTile(gridPos.x, gridPos.y);

            if(tile.TileType != TileType.Empty) return;

            agent.BombsPlaced++;

            BombComponent bomb = Instantiate(BombPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity, transform);

            SetLayerRecursively(bomb.gameObject, gameObject.layer);
            agent.BombsRemaining--;

            ActiveBombs.Add(new ActiveBomb(bomb, agent, BombFuseTime));
            Grid.SetTile(gridPos.x, gridPos.y, TileType.Bomb, bomb);
        }

        public void InitExplosion(ActiveBomb activeBomb)
        {
            Vector2Int position = activeBomb.Bomb.Position;

            // Handle explosion logic
            ExplosionComponent explosion = Instantiate(ExplosionPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity, transform);
            explosion.Position = position;
            explosion.Owener = activeBomb.Owner;
            SetLayerRecursively(explosion.gameObject, gameObject.layer);

            explosion.SetActiveRenderer(explosion.Start);
            ActiveExplosions.Add(new ActiveExplosion(explosion, ExplosionDuration));

            Vector2Int gridPos = Grid.WorldToGrid(position.x, position.y);
            Grid.SetTile(gridPos.x, gridPos.y, TileType.Explosion, explosion);

            Explode(position, Vector2Int.up, activeBomb.Owner.ExplosionRadius, activeBomb.Owner);
            Explode(position, Vector2Int.down, activeBomb.Owner.ExplosionRadius, activeBomb.Owner);
            Explode(position, Vector2Int.left, activeBomb.Owner.ExplosionRadius, activeBomb.Owner);
            Explode(position, Vector2Int.right, activeBomb.Owner.ExplosionRadius, activeBomb.Owner);

            Destroy(activeBomb.Bomb.gameObject);
            activeBomb.Owner.BombsRemaining++;

            ActiveBombs.Remove(activeBomb);
        }

        public void Explode(Vector2Int position, Vector2Int direction, int length, BombClashAgentComponent agent)
        {
            if (length <= 0)
                return;

            position += direction;

            Vector2Int gridPos = Grid.WorldToGrid(position.x, position.y);
            GridTile tile = Grid.GetTile(gridPos.x, gridPos.y);

            if(tile.TileType == TileType.Destructible)
            {
                ClearDestructible(position, agent);
                return;
            }

            if (tile.TileType != TileType.Empty) return;

            ExplosionComponent explosion = Instantiate(ExplosionPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity, transform);
            explosion.Position = position;
            explosion.Owener = agent;
            SetLayerRecursively(explosion.gameObject, gameObject.layer);

            explosion.SetActiveRenderer(length > 1 ? explosion.Middle : explosion.End);
            explosion.SetDirection(direction);

            ActiveExplosions.Add(new ActiveExplosion(explosion, ExplosionDuration));
            Grid.SetTile(gridPos.x, gridPos.y, TileType.Explosion, explosion);

            Explode(position, direction, length - 1, agent);
        }

        public void ClearDestructible(Vector2Int position, BombClashAgentComponent agent)
        {
            Vector3 pos = new Vector3(position.x, position.y, 0);
            Vector3Int cell = DestructibleTiles.WorldToCell(pos);
            TileBase tile = DestructibleTiles.GetTile(cell);

            if (tile != null)
            {
                DestructibleComponent destructible = Instantiate(DestructiblePrefab, pos, Quaternion.identity, transform);
                destructible.Position = position;
                SetLayerRecursively(destructible.gameObject, gameObject.layer);

                DestructibleTiles.SetTile(cell, null);

                ActiveDestructibles.Add(new ActiveDestructible(destructible, DestructibleDestructionTime));
                Vector2Int gridPos = Grid.WorldToGrid(position.x, position.y);
                Grid.SetTile(gridPos.x, gridPos.y, TileType.ActiveDestructible, destructible);

                agent.BlocksDestroyed++;
            }
        }

        public void SpawnPowerUpItem(ActiveDestructible activeDestructible)
        {
            // Spawn an item (power up)
            if (SpawnableItems.Length > 0 && Util.NextDouble() < PowerUpSpawnChance)
            {
                int index = Util.NextInt(0, SpawnableItems.Length);
                ItemPickupComponent powerup = Instantiate(SpawnableItems[index], new Vector3(activeDestructible.DestructibleComponent.Position.x, activeDestructible.DestructibleComponent.Position.y, 0), Quaternion.identity, activeDestructible.DestructibleComponent.transform.parent);

                Vector3Int cell = DestructibleTiles.WorldToCell(activeDestructible.DestructibleComponent.transform.position);
                Vector2Int gridPos = Grid.WorldToGrid(activeDestructible.DestructibleComponent.Position.x, activeDestructible.DestructibleComponent.Position.y);
                Grid.SetTile(gridPos.x, gridPos.y, TileType.PowerUp, powerup);

                SetLayerRecursively(powerup.gameObject, gameObject.layer);
            }
            else
            {
                Vector2Int gridPos = Grid.WorldToGrid(activeDestructible.DestructibleComponent.Position.x, activeDestructible.DestructibleComponent.Position.y);
                Grid.SetTile(gridPos.x, gridPos.y, TileType.Empty, null);
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

        public void SetAgentsFitness()
        {
            sectorCount = CountTiles(IndestructibleWalkableTiles);

            foreach (BombClashAgentComponent agent in Agents)
            {
                // Sector exploration
                sectorExplorationFitness = agent.SectorsExplored / (float)sectorCount;
                sectorExplorationFitness = (float)Math.Round(BombClashFitness.FitnessValues[BombClashFitness.FitnessKeys.SectorExploration.ToString()] * sectorExplorationFitness, 4);
                agent.AgentFitness.UpdateFitness(sectorExplorationFitness, BombClashFitness.FitnessKeys.SectorExploration.ToString());

                // Bombs placed
                allPossibleBombs = (CurrentSimulationSteps) / BombFuseTime;
                bombsPlacedFitness = agent.BombsPlaced / allPossibleBombs;
                if (bombsPlacedFitness > 1)
                    bombsPlacedFitness = 1;
                bombsPlacedFitness = (float)Math.Round(BombClashFitness.FitnessValues[BombClashFitness.FitnessKeys.BombsPlaced.ToString()] * bombsPlacedFitness, 4);
                agent.AgentFitness.UpdateFitness(bombsPlacedFitness, BombClashFitness.FitnessKeys.BombsPlaced.ToString());

                // Blocks destroyed
                blocksDestroyedFitness = agent.BlocksDestroyed / (float)destructibleTilesCount;
                blocksDestroyedFitness = (float)Math.Round(BombClashFitness.FitnessValues[BombClashFitness.FitnessKeys.BlockDestroyed.ToString()] * blocksDestroyedFitness, 4);
                agent.AgentFitness.UpdateFitness(blocksDestroyedFitness, BombClashFitness.FitnessKeys.BlockDestroyed.ToString());

                // Power ups collected
                powerUpsCollectedFitness = agent.PowerUpsCollected / (float)(destructibleTilesCount * PowerUpSpawnChance);
                if (powerUpsCollectedFitness > 1) powerUpsCollectedFitness = 1;
                powerUpsCollectedFitness = (float)Math.Round(BombClashFitness.FitnessValues[BombClashFitness.FitnessKeys.PowerUpsCollected.ToString()] * powerUpsCollectedFitness, 4);
                agent.AgentFitness.UpdateFitness(powerUpsCollectedFitness, BombClashFitness.FitnessKeys.PowerUpsCollected.ToString());

                // Bombs hit agent
                opponentPlacedBombs = Agents.Select(x => (x as BombClashAgentComponent).BombsPlaced).Sum() - agent.BombsPlaced;
                if (opponentPlacedBombs > 0)
                {
                    bombsHitAgentFitness = agent.BombsHitAgent / (float)opponentPlacedBombs;
                    bombsHitAgentFitness = (float)Math.Round(BombClashFitness.FitnessValues[BombClashFitness.FitnessKeys.BombsHitAgent.ToString()] * bombsHitAgentFitness, 4);
                    agent.AgentFitness.UpdateFitness(bombsHitAgentFitness, BombClashFitness.FitnessKeys.BombsHitAgent.ToString());
                }
                else
                {
                    bombsHitAgentFitness = 0f;
                }

                // Agents killed
                agentsKilledFitness = agent.AgentsKilled / (float)(Agents.Length - 1);
                agentsKilledFitness = (float)Math.Round(BombClashFitness.FitnessValues[BombClashFitness.FitnessKeys.AgentsKilled.ToString()] * agentsKilledFitness, 4);
                agent.AgentFitness.UpdateFitness(agentsKilledFitness, BombClashFitness.FitnessKeys.AgentsKilled.ToString());

                // Agent hit by bombs
                if (opponentPlacedBombs > 0)
                {
                    agentHitByBombsFitness = agent.AgentHitByBombs / (float)opponentPlacedBombs;
                    agentHitByBombsFitness = (float)Math.Round(BombClashFitness.FitnessValues[BombClashFitness.FitnessKeys.AgentHitByBombs.ToString()] * agentHitByBombsFitness, 4);
                    agent.AgentFitness.UpdateFitness(agentHitByBombsFitness, BombClashFitness.FitnessKeys.AgentHitByBombs.ToString());
                }
                else
                {
                    agentHitByBombsFitness = 0f;
                }

                // Agent hit by own bombs
                if (agent.AgentHitByOwnBombs > 0 && agent.BombsPlaced > 0)
                {
                    agentHitByOwnBombsFitness = agent.AgentHitByOwnBombs / (float)agent.BombsPlaced;
                    agentHitByOwnBombsFitness = (float)Math.Round(BombClashFitness.FitnessValues[BombClashFitness.FitnessKeys.AgentHitByOwnBombs.ToString()] * agentHitByOwnBombsFitness, 4);
                    agent.AgentFitness.UpdateFitness(agentHitByOwnBombsFitness, BombClashFitness.FitnessKeys.AgentHitByOwnBombs.ToString());
                }
                else
                {
                    agentHitByOwnBombsFitness = 0f;
                }

                // Agent death
                if (agent.AgentDied)
                {
                    agentDeathFitness = BombClashFitness.FitnessValues[BombClashFitness.FitnessKeys.AgentDeath.ToString()];
                    agent.AgentFitness.UpdateFitness(agentDeathFitness, BombClashFitness.FitnessKeys.AgentDeath.ToString());
                }
                else
                {
                    agentDeathFitness = 0f;
                }

                // Survival bonus
                if (agent.SurvivalBonuses > 0)
                {
                    survivalBonusFitness = agent.SurvivalBonuses / (float)(Agents.Length - 1);
                    survivalBonusFitness = (float)Math.Round(BombClashFitness.FitnessValues[BombClashFitness.FitnessKeys.SurvivalBonus.ToString()] * survivalBonusFitness, 4);
                    agent.AgentFitness.UpdateFitness(survivalBonusFitness, BombClashFitness.FitnessKeys.SurvivalBonus.ToString());
                }
                else
                {
                    survivalBonusFitness = 0;
                }

                // Last survival bonus
                if (agent.LastSurvivalBonus)
                {
                    lastSurvivalBonusFitness = BombClashFitness.FitnessValues[BombClashFitness.FitnessKeys.LastSurvivalBonus.ToString()];
                    agent.AgentFitness.UpdateFitness(lastSurvivalBonusFitness, BombClashFitness.FitnessKeys.LastSurvivalBonus.ToString());
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

        void ReadParamsFromMainConfiguration()
        {
            if (MenuManager.Instance != null && MenuManager.Instance.MainConfiguration != null)
            {
                MainConfiguration conf = MenuManager.Instance.MainConfiguration;

                BombClashFitness.FitnessValues = conf.FitnessValues;

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

                if (conf.ProblemConfiguration.ContainsKey("MaxSpeedIncrease"))
                {
                    MaxSpeedIncrease = float.Parse(conf.ProblemConfiguration["MaxSpeedIncrease"]);
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
    }
}