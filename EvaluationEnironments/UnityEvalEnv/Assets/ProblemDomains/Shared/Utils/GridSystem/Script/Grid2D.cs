using System.Collections.Generic;
using UnityEngine;

namespace Problems.Utils.GridSystem
{
    public abstract class Grid2D : GridBase
    {
        // Temp variables defined globally for optimization
        Vector3 mousePosition;

        private void FixedUpdate()
        {
            if (IsMousePositionInGrid(out int gridX, out int gridY))
            {
                GridNode currentCell = GridArray[gridX, gridY];

                if (currentCell != LastHoveredCell)
                {
                    if (LastHoveredCell != null)
                    {
                        LastHoveredCell.Hide();
                    }
                    currentCell.Show();
                    LastHoveredCell = currentCell;
                }
            }
            else
            {
                if (LastHoveredCell != null)
                {
                    LastHoveredCell.Hide();
                    LastHoveredCell = null;
                }
            }
        }

        public override void CreateGrid()
        {
            GridArray = new GridNode[X, Y];

            for (int x = 0; x < X; x++)
            {
                for (int y = 0; y < Y; y++)
                {
                    Vector3 worldPosition = GetCellWorldPosition(x, y);
                    GameObject nodeGameObject = Instantiate(GridPrefab, worldPosition, Quaternion.identity);

                    nodeGameObject.transform.SetParent(transform);
                    nodeGameObject.layer = gameObject.layer;
                    nodeGameObject.name = $"Grid ({x}, {y})";

                    GridArray[x, y] = new GridNode(new Vector3Int(x, y, 0), nodeGameObject);
                    GridArray[x, y].Hide();
                }
            }
        }

        public override Vector3 GetMouseWorldPosition()
        {
            mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.nearClipPlane; // Adjust for 2D
            return Camera.main.ScreenToWorldPoint(mousePosition);
        }
    }
}