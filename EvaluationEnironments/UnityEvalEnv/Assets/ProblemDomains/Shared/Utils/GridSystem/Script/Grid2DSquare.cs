using static UnityEngine.RuleTile.TilingRuleOutput;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.Utils.GridSystem
{
    public class Grid2DSquare :  Grid2D
    {
        // Temp variables defined globally for optimization
        Vector3 localPosition;
        Vector3 closestCellCenter;

        int gridXMin, gridXMax, gridYMin, gridYMax;

        Vector3 cellCenter;

        List<GridNode> neighbors;

        Vector3Int[] directions = new Vector3Int[]
            {
                new Vector3Int(0, 1, 0), // Up
                new Vector3Int(0, -1, 0), // Down
                new Vector3Int(-1, 0, 0), // Left
                new Vector3Int(1, 0, 0) // Right
            };

        int dstX, dstY;

        public override bool IsPositionInGrid(Vector3 worldPosition, out int gridX, out int gridY)
        {
            localPosition = worldPosition - OriginPosition;
            gridX = Mathf.FloorToInt(localPosition.x / CellSize);
            gridY = Mathf.FloorToInt(localPosition.y / CellSize);

            gridXMin = gridX - 1;
            gridXMax = gridX + 1;
            gridYMin = gridY - 1;
            gridYMax = gridY + 1;

            if (gridXMin < 0) gridXMin = 0;
            if (gridXMax >= X) gridXMax = X - 1;
            if (gridYMin < 0) gridYMin = 0;
            if (gridYMax >= Y) gridYMax = Y - 1;

            closestCellCenter = GetCellWorldPosition(gridX, gridY);

            for (int x = gridXMin; x <= gridXMax; x++)
            {
                for (int y = gridYMin; y <= gridYMax; y++)
                {
                    cellCenter = GetCellWorldPosition(x, y);
                    if (Vector3.Distance(cellCenter, worldPosition) < Vector3.Distance(closestCellCenter, worldPosition))
                    {
                        gridX = x;
                        gridY = y;
                        closestCellCenter = cellCenter;
                    }
                }
            }

            return gridX >= 0 && gridY >= 0 && gridX < X && gridY < Y;
        }

        public override int GetDistance(GridNode nodeA, GridNode nodeB)
        {
            dstX = Mathf.Abs(nodeA.LocalPosition.x - nodeB.LocalPosition.x);
            dstY = Mathf.Abs(nodeA.LocalPosition.y - nodeB.LocalPosition.y);

            if (dstX > dstY)
            {
                return 14 * dstY + 10 * (dstX - dstY);
            }
            return 14 * dstX + 10 * (dstY - dstX);
        }

        public override List<GridNode> GetNeighbors(GridNode node)
        {
            neighbors = new List<GridNode>();

            foreach (Vector3Int direction in directions)
            {
                Vector3Int neighborPosition = node.LocalPosition + direction;

                if (IsValidGridPosition(neighborPosition.x, neighborPosition.y))
                {
                    neighbors.Add(GridArray[neighborPosition.x, neighborPosition.y]);
                }
            }

            return neighbors;
        }
    }
}