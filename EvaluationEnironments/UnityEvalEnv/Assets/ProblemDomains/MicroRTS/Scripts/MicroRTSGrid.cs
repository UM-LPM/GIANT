using UnityEngine;
using UnityEngine.Tilemaps;
using Problems.MicroRTS.Core;

namespace Problems.MicroRTS
{
    public class MicroRTSGrid : Grid
    {
        [HideInInspector] public int Width { get; set; }
        [HideInInspector] public int Height { get; set; }

        private Tilemap walkableTilemap;
        private MicroRTSEnvironmentController environmentController;

        public void InitGrid(int width, int height, Tilemap walkableTilemap, MicroRTSEnvironmentController controller)
        {
            this.Width = width;
            this.Height = height;
            this.walkableTilemap = walkableTilemap;
            this.environmentController = controller;
        }

        public override Component[] GetCellItem(int x, int y, int z)
        {
            if (environmentController == null) return new Component[] { };

            Unit unit = environmentController.GetUnitAt(x, y);
            if (unit != null)
            {
                GameObject unitObj = environmentController.GetUnitGameObject(unit.ID);
                if (unitObj != null)
                {
                    MicroRTSUnitComponent component = environmentController.GetUnitComponent(unitObj);
                    if (component != null)
                    {
                        return new Component[] { component };
                    }
                }
            }

            return new Component[] { };
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public bool IsWalkable(int x, int y)
        {
            if (!IsInBounds(x, y)) return false;
            if (walkableTilemap == null) return false;
            if (environmentController == null) return false;

            BoundsInt bounds = walkableTilemap.cellBounds;
            Vector3Int tilemapOffset = bounds.position;

            // Grid to tilemap cell coordinates
            // Y is inverted + offset
            Vector3Int cellPos = new Vector3Int(
                x + tilemapOffset.x,
                Height - 1 - y + tilemapOffset.y,
                0
            );

            // Tile exists on position
            return walkableTilemap.HasTile(cellPos);
        }

        public Vector2Int WorldToGrid(float worldX, float worldY)
        {
            if (walkableTilemap == null || environmentController == null)
            {
                return new Vector2Int((int)worldX, (int)worldY);
            }

            Vector3 worldPos = new Vector3(worldX, worldY, 0);
            if (environmentController.TryWorldToGrid(worldPos, out int gridX, out int gridY))
            {
                return new Vector2Int(gridX, gridY);
            }

            return new Vector2Int(-1, -1);
        }

        public Vector3 GridToWorld(int x, int y)
        {
            if (environmentController == null)
            {
                return new Vector3(x, y, 0);
            }

            return environmentController.GridToWorldPosition(x, y);
        }
    }
}