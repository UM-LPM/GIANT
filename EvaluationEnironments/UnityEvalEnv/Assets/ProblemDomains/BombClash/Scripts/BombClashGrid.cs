using UnityEngine;

namespace Problems.BombClash
{
    public class BombClashGrid : Grid
    {
        [HideInInspector] public int Width { get; set; }
        [HideInInspector] public int Height { get; set; }

        public Vector2Int WorldMin { get; private set; }

        private GridTile[,] Grid {  get; set; }

        public void InitGrid(int width, int height, Vector2Int worldMin)
        {
            this.Width = width;
            this.Height = height;
            this.WorldMin = worldMin;

            Grid = new GridTile[Width, Height];

            for (int i = 0; i < Grid.GetLength(0); i++)
            {
                for (int j = 0; j < Grid.GetLength(1); j++)
                {
                    Grid[i,j] = new GridTile();
                }
            }
        }

        public override Component[] GetCellItem(int x, int y, int z)
        {
            var tile = GetTile(x, y);
            if (tile == null) throw new System.Exception("Grid position is invalid");

            if (tile.Component == null) return new Component[] { };
            else return new Component[] { tile.Component };
        }

        public GridTile GetTile(int x, int y)
        {
            return IsInBounds(x, y) ? Grid[x, y] : null;
        }

        public void SetTile(int x, int y, TileType tileType, Component component)
        {
            if (!IsInBounds(x, y)) return;

            Grid[x, y].TileType = tileType;
            Grid[x, y].Component = component;
        }

        public void MoveTile(int oldX, int oldY, int newX, int newY, GridTile tile)
        {
            if (!IsInBounds(oldX, oldY)) return;
            if (!IsInBounds(newX, newY)) return;

            SetTile(oldX, oldY, TileType.Empty, null);
            SetTile(newX, newY, tile.TileType, tile.Component);
        }

        public bool IsWalkable(int x, int y)
        {
            if(IsInBounds(x, y)) return true;
            return Grid[x, y].IsWalkable();
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public Vector2Int WorldToGrid(int worldX, int worldY)
        {
            int gridX = worldX - WorldMin.x;
            int gridY = -worldY - WorldMin.y;
            return new Vector2Int(gridX, gridY);
        }

        public Vector3 GridToWorld(int x, int y)
        {
            float worldX = WorldMin.x + x;
            float worldY = WorldMin.y + y;
            return new Vector3(worldX, worldY, 0f);
        }
    }

    public class GridTile
    {
        public TileType TileType { get; set; }
        public Component Component { get; set; }

        public GridTile()
        {
            TileType = TileType.Empty;
            Component = null;
        }

        public bool IsWalkable()
        {
            return TileType == TileType.Empty || TileType == TileType.PowerUp || TileType == TileType.Bomb || TileType == TileType.Explosion;
        }
    }


    public enum TileType
    {
        Empty,          // Walkable ground
        Wall,           // Indestructible
        Destructible,   // Box or crate
        ActiveDestructible,   // Destroying Box or crate
        Bomb,           // Placed bomb
        Explosion,      // Temporary explosion
        PowerUp         // PowerUp
    }
}