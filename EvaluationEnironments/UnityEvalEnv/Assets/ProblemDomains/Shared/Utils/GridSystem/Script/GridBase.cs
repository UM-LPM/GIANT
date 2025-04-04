using Problems.Utils.PathFinding;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.Utils.GridSystem
{
    public abstract class GridBase : MonoBehaviour
    {
        [Header("Base Grid Settings")]
        [SerializeField, Range(3, 50)] public int X;
        [SerializeField, Range(3, 50)] public int Y;
        [SerializeField] public float CellSize = 1;
        [SerializeField] public Vector3 OriginPosition;

        [SerializeField] public GameObject GridPrefab;
        public GridNode[,] GridArray { get; set; }

        public GridNode LastHoveredCell { get; protected set; }

        public PathFindingAlgorithm PathFindingAlgorithm { get; set; }

        // Temp variables defined globally for optimization
        protected Vector3 lastRequrestedWorldPosition = Vector3.zero;
        protected GameObject go;

        void Awake()
        {
            CreateGrid();

            PathFindingAlgorithm = GetComponent<PathFindingAlgorithm>();
            if(PathFindingAlgorithm == null)
            {
                UnityEngine.Debug.LogWarning("PathFindingAlgorithm component is missing on the GridBase object.");
            }
        }

        public abstract void CreateGrid();
        public abstract Vector3 GetMouseWorldPosition();

        public bool IsMousePositionInGrid(out int gridX, out int gridY)
        {
            return IsPositionInGrid(GetMouseWorldPosition(), out gridX, out gridY);
        }

        public abstract bool IsPositionInGrid(Vector3 worldPosition, out int gridX, out int gridY);

        public abstract int GetDistance(GridNode nodeA, GridNode nodeB);

        public abstract List<GridNode> GetNeighbors(GridNode node);

        public virtual Vector3 GetCellWorldPosition(int x, int y)
        {
            return new Vector3(x, y) * CellSize + OriginPosition;
        }

        public virtual Vector3Int GetCellFromWorldPosition(float x, float y)
        {
            return new Vector3Int(Mathf.FloorToInt((x - OriginPosition.x) / CellSize), Mathf.FloorToInt((y - OriginPosition.y) / CellSize), 0);
        }

        public GameObject GetGridObject(int x, int y)
        {
            return GridArray[x, y].ChildGameObject;
        }

        public GameObject AddGridObject(int x, int y, GameObject prefabGO, bool isWalkable)
        {
            go = Instantiate(prefabGO, GetCellWorldPosition(x, y), Quaternion.identity);
            go.transform.SetParent(transform);
            go.layer = gameObject.layer;

            SetGridObject(x,y, go, isWalkable);

            return go;
        }

        public void SetGridObject(int x, int y, GameObject gameObject, bool isWalkable)
        {
            GridArray[x, y].ChildGameObject = gameObject;
            GridArray[x, y].IsWalkable = isWalkable;
        }

        public bool IsValidGridPosition(int x, int y)
        {
            return x >= 0 && x < X && y >= 0 && y < Y;
        }

        public void MoveGridObject(Vector3Int start, Vector3Int end)
        {
            // Check if start and end positions are the same. If so, return
            if (start.x== end.x && start.y == end.y) { return; }

            // Check if start and end positions are valid
            if (!IsValidGridPosition(start.x, start.y) || !IsValidGridPosition(end.x, end.y)) { return; }

            GameObject gameObject = GetGridObject(start.x, start.y);
            bool isWalkable = GridArray[end.x, end.y].IsWalkable;

            SetGridObject(start.x, start.y, null, true);
            SetGridObject(end.x, end.y, gameObject, isWalkable);

            gameObject.transform.position = GetCellWorldPosition(end.x, end.y);
        }

        public Vector3Int FindPath(Vector3Int start, Vector3Int end)
        {
            return PathFindingAlgorithm.FindPath(this, start, end);
        }
    }
}