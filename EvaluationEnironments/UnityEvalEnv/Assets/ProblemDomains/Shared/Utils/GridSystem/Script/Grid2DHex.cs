using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Problems.Utils.GridSystem
{
    public class Grid2DHex :  Grid2D
    {
        // Temp variables defined globally for optimization
        List<GridNode> neighbors = new List<GridNode>();
        float xOffset;
        Vector3 localPosition;
        Vector3Int neighborPosition;

        int dx, dy, dz;

        Vector3Int roughPos;
        Vector3Int closestNeighbour;
        Vector3Int currentNeighbour;

        Vector3Int[] directionsEven = new Vector3Int[]
           {
                new Vector3Int(-1, 0, 0), // Left
                new Vector3Int(-1, 1, 0), // Up Left
                new Vector3Int(0, 1, 0), // Up
                new Vector3Int(1, 0, 0), // Right
                new Vector3Int(0, -1, 0), // Down
                new Vector3Int(-1, -1, 0), // Down Left
           };
        Vector3Int[] directionsOdd = new Vector3Int[]
           {
                new Vector3Int(-1, 0, 0), // Left
                new Vector3Int(0, 1, 0), // Up
                new Vector3Int(1, 1, 0), // Up Right
                new Vector3Int(1, 0, 0), // Right
                new Vector3Int(1, -1, 0), // Down Right
                new Vector3Int(0, -1, 0), // Down
           };
        Vector3Int[] directions;

        public override Vector3 GetCellWorldPosition(int x, int y)
        {
            xOffset = (y % 2 == 0) ? 0 : CellSize/ 2;

            lastRequrestedWorldPosition.x = x * CellSize + xOffset;
            lastRequrestedWorldPosition.y = y * CellSize * Mathf.Sqrt(3) / 2;
            lastRequrestedWorldPosition.z = 0;
            return lastRequrestedWorldPosition + OriginPosition;
        }

        public override Vector3Int GetCellFromWorldPosition(float x, float y)
        {
            // Convert world coordinates to the local grid coordinates
            localPosition = Vector3.zero;
           
            localPosition.y = y - OriginPosition.y;
            localPosition.y /= CellSize * Mathf.Sqrt(3) / 2;

            localPosition.x = x - OriginPosition.x;
            localPosition.x -= (localPosition.y % 2 == 0) ? 0 : CellSize / 2;
            localPosition.x /= CellSize;
            localPosition.z = 0;

            // Check if all values of localPosition are whole numbers
            if (Mathf.Approximately(localPosition.x, Mathf.Floor(localPosition.x)) && Mathf.Approximately(localPosition.y, Mathf.Floor(localPosition.y)))
            {
                // Return the local grid coordinates
                return new Vector3Int(Mathf.FloorToInt(localPosition.x), Mathf.FloorToInt(localPosition.y), 0);
            }
            else
            {
                throw new System.Exception("Invalid world position for hexagonal grid (World x: " + x +", " +
                    "World y: " + y + ", Cell x: " + localPosition.x + ", Cell y: " + localPosition.y + ").");
            }
        }

        public override bool IsPositionInGrid(Vector3 worldPosition, out int gridX, out int gridY)
        {
            // Check if the world position is inside any of the grid cells
            localPosition = worldPosition - OriginPosition;

            int roughX = Mathf.RoundToInt(localPosition.x / CellSize);
            int roughY = Mathf.RoundToInt(localPosition.y / (CellSize * Mathf.Sqrt(3) / 2));

            roughPos = new Vector3Int(roughX, roughY, 0);

            if (roughY % 2 == 0)
                directions = directionsEven;
            else
                directions = directionsOdd;

            closestNeighbour = roughPos;

            foreach (Vector3Int direction in directions)
            {
                currentNeighbour = roughPos + direction;
                if (Vector3.Distance(worldPosition, GetCellWorldPosition(currentNeighbour.x, currentNeighbour.y)) < Vector3.Distance(worldPosition, GetCellWorldPosition(closestNeighbour.x, closestNeighbour.y)))
                {
                    closestNeighbour = new Vector3Int(currentNeighbour.x, currentNeighbour.y, 0);
                }
            }

            gridX = closestNeighbour.x;
            gridY = closestNeighbour.y;

            // Check if the closest neighbour is inside the grid
            if (IsValidGridPosition(closestNeighbour.x, closestNeighbour.y))
                return true;
            else
                return false;
        }

        public override int GetDistance(GridNode nodeA, GridNode nodeB)
        {
            dx = nodeA.LocalPosition.x - nodeB.LocalPosition.x;
            dy = nodeA.LocalPosition.y - nodeB.LocalPosition.y;
            dz = -dx - dy;
            return Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy), Mathf.Abs(dz));
        }

        public override List<GridNode> GetNeighbors(GridNode node)
        {
            neighbors.Clear();

            if(node.LocalPosition.y % 2 == 0)
                directions = directionsEven;
            else
                directions = directionsOdd;

            foreach (Vector3Int direction in directions)
            {
                neighborPosition = node.LocalPosition + direction;

                if (IsValidGridPosition(neighborPosition.x, neighborPosition.y))
                {
                    neighbors.Add(GridArray[neighborPosition.x, neighborPosition.y]);
                }
            }

            return neighbors;
        }
    }
}