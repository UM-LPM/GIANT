using Base;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Problems.MicroRTS.Core;
using Utils;

namespace Problems.MicroRTS
{
    public class MicroRTSEnvironmentController : EnvironmentControllerBase
    {
        [Header("MicroRTS Game State")]
        [SerializeField] private UnitTypeTable unitTypeTable;

        [Header("Grid Settings")]
        [SerializeField] private Tilemap walkableTilemap;
        [SerializeField] private int mapWidth = 8;
        [SerializeField] private int mapHeight = 8;

        public MicroRTSGrid Grid { get; private set; }

        private Vector3Int tilemapOffset;

        // Game state
        private List<Player> players = new List<Player>();
        private List<Unit> units = new List<Unit>();

        // Unit registry: maps Unit ID to GameObject
        private Dictionary<long, GameObject> unitGameObjects = new Dictionary<long, GameObject>();
        private Dictionary<GameObject, MicroRTSUnitComponent> unitComponents = new Dictionary<GameObject, MicroRTSUnitComponent>();

        public UnitTypeTable UnitTypeTable => unitTypeTable;
        public int MapWidth => mapWidth;
        public int MapHeight => mapHeight;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            unitTypeTable = new UnitTypeTable();

            // IndestructablesWalkable tilemap
            Tilemap[] allTilemaps = GetComponentsInChildren<Tilemap>();
            walkableTilemap = null;
            foreach (Tilemap tm in allTilemaps)
            {
                if (tm.name == "IndestructablesWalkable")
                {
                    walkableTilemap = tm;
                    break;
                }
            }

            if (walkableTilemap == null)
            {
                DebugSystem.LogError("IndestructablesWalkable tilemap not found");
            }
            else
            {
                BoundsInt bounds = walkableTilemap.cellBounds;
                tilemapOffset = bounds.position;
                mapWidth = bounds.size.x;
                mapHeight = bounds.size.y;
            }

            // Initialize grid
            Grid = GetComponent<MicroRTSGrid>();
            if (Grid == null)
            {
                DebugSystem.LogError("MicroRTSGrid component not found");
            }
            else
            {
                Grid.InitGrid(mapWidth, mapHeight, walkableTilemap, this);
            }

            // Initialize players
            players.Clear();
            players.Add(new Player(0, 0));
            players.Add(new Player(1, 0));
        }

        public bool TryWorldToGrid(Vector3 worldPosition, out int gridX, out int gridY)
        {
            gridX = 0;
            gridY = 0;

            if (walkableTilemap == null)
            {
                DebugSystem.LogWarning($"Can't convert world position {worldPosition} to grid - tilemap is null");
                return false;
            }

            Vector3Int cellPos = walkableTilemap.WorldToCell(worldPosition);
            gridX = cellPos.x - tilemapOffset.x;
            gridY = mapHeight - 1 - (cellPos.y - tilemapOffset.y);

            if (gridX < 0 || gridX >= mapWidth || gridY < 0 || gridY >= mapHeight)
            {
                DebugSystem.LogWarning($"Grid position ({gridX}, {gridY}) is out of bounds (map is {mapWidth}x{mapHeight})");
                return false;
            }

            return true;
        }

        public Vector3 GridToWorldPosition(int gridX, int gridY)
        {
            if (walkableTilemap == null)
            {
                DebugSystem.LogWarning("Tilemap is null, using fallback position calculation");
                return new Vector3(gridX, mapHeight - 1 - gridY, 0);
            }

            Vector3Int cellPos = new Vector3Int(
                gridX + tilemapOffset.x,
                mapHeight - 1 - gridY + tilemapOffset.y,
                0
            );
            Vector3 cellWorldPos = walkableTilemap.CellToWorld(cellPos);
            Vector3 centeredPos = cellWorldPos + walkableTilemap.cellSize * 0.5f;

            return centeredPos;
        }

        public bool IsWalkable(int gridX, int gridY)
        {
            if (Grid == null) return false;
            return Grid.IsWalkable(gridX, gridY);
        }

        public void RegisterUnit(GameObject unitGameObject, MicroRTSUnitComponent unitComponent, Unit unit)
        {
            if (unitGameObject == null || unitComponent == null || unit == null)
            {
                DebugSystem.LogError("Can't register unit - missing GameObject, component, or unit data");
                throw new System.ArgumentNullException("Cannot register null unit, component, or GameObject");
            }

            unitGameObjects[unit.ID] = unitGameObject;
            unitComponents[unitGameObject] = unitComponent;

            AddUnit(unit);
        }

        public GameObject GetUnitGameObject(long unitID)
        {
            unitGameObjects.TryGetValue(unitID, out GameObject obj);
            return obj;
        }

        public MicroRTSUnitComponent GetUnitComponent(GameObject unitGameObject)
        {
            unitComponents.TryGetValue(unitGameObject, out MicroRTSUnitComponent component);
            return component;
        }

        public IEnumerable<GameObject> GetAllUnitGameObjects()
        {
            return unitGameObjects.Values;
        }

        protected override void OnPostFixedUpdate()
        {
            CheckEndingState();
        }

        public override void CheckEndingState()
        {
            // Count alive units per player
            int[] unitCounts = new int[players.Count];
            int totalAliveUnits = 0;

            foreach (Unit u in units)
            {
                if (u.HitPoints > 0 && u.Player >= 0 && u.Player < unitCounts.Length)
                {
                    unitCounts[u.Player]++;
                    totalAliveUnits++;
                }
            }

            // Game is over if no units remain or only one player has units
            if (totalAliveUnits == 0)
            {
                FinishGame();
                return;
            }

            int playersWithUnits = 0;
            for (int i = 0; i < unitCounts.Length; i++)
            {
                if (unitCounts[i] > 0)
                {
                    playersWithUnits++;
                }
            }

            if (playersWithUnits <= 1)
            {
                FinishGame();
            }
        }

        // PhysicalGameState methods
        public void AddPlayer(Player player)
        {
            players.Add(player);
        }

        public void AddUnit(Unit unit)
        {
            if (unit.X < 0 || unit.X >= mapWidth || unit.Y < 0 || unit.Y >= mapHeight)
            {
                DebugSystem.LogError($"Unit {unit.ID} at ({unit.X}, {unit.Y}) is outside map bounds ({mapWidth}x{mapHeight})");
            }
            Debug.Assert(unit.X >= 0 && unit.X < mapWidth && unit.Y >= 0 && unit.Y < mapHeight, "Unit coordinates must be within map bounds");

            if (GetUnit(unit.ID) != null)
            {
                DebugSystem.LogError($"Unit with ID {unit.ID} already exists");
                throw new ArgumentException($"Unit with ID {unit.ID} already exists!");
            }

            units.Add(unit);
        }

        public void RemoveUnit(Unit unit)
        {
            if (unit == null) return;

            units.Remove(unit);

            if (unitGameObjects.TryGetValue(unit.ID, out GameObject unitObj))
            {
                unitGameObjects.Remove(unit.ID);
                unitComponents.Remove(unitObj);
                if (unitObj != null)
                {
                    Destroy(unitObj);
                }
            }
        }

        public Unit GetUnit(long id)
        {
            return units.FirstOrDefault(u => u.ID == id);
        }

        public Unit GetUnitAt(int x, int y)
        {
            // Only return alive units
            return units.FirstOrDefault(u => u.X == x && u.Y == y && u.HitPoints > 0);
        }

        public Player GetPlayer(int playerId)
        {
            return players.FirstOrDefault(p => p.ID == playerId);
        }

        public List<Unit> GetUnitsAround(int x, int y, int squareRange)
        {
            List<Unit> result = new List<Unit>();
            foreach (Unit unit in units)
            {
                if (Math.Abs(unit.X - x) <= squareRange && Math.Abs(unit.Y - y) <= squareRange)
                {
                    result.Add(unit);
                }
            }
            return result;
        }

        public List<Unit> GetUnitsAround(int x, int y, int width, int height)
        {
            List<Unit> result = new List<Unit>();
            int halfWidth = width / 2;
            int halfHeight = height / 2;

            foreach (Unit unit in units)
            {
                if (unit.X >= x - halfWidth && unit.X <= x + halfWidth &&
                    unit.Y >= y - halfHeight && unit.Y <= y + halfHeight)
                {
                    result.Add(unit);
                }
            }
            return result;
        }

        public List<Unit> GetUnitsInRectangle(int x, int y, int width, int height)
        {
            List<Unit> result = new List<Unit>();
            foreach (Unit unit in units)
            {
                if (unit.X >= x && unit.X < x + width && unit.Y >= y && unit.Y < y + height)
                {
                    result.Add(unit);
                }
            }
            return result;
        }

        public List<Unit> GetAllUnits()
        {
            return units;
        }

        public void ResetAllUnitsHP()
        {
            foreach (Unit unit in units)
            {
                unit.SetHitPoints(unit.Type.HP);
            }
        }
    }
}