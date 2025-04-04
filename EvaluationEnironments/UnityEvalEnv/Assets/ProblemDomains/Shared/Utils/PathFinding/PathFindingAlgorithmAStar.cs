using System.Collections.Generic;
using UnityEngine;
using Problems.Utils.GridSystem;

namespace Problems.Utils.PathFinding
{
    public class PathFindingAlgorithmAStar : PathFindingAlgorithm
    {
        // Temp variables defined globally for optimization
        List<GridNode> openSet = new List<GridNode>();
        List<GridNode> closedSet = new List<GridNode>();
        List<GridNode> neighbors = new List<GridNode>();

        GridNode currentNode;
        GridNode pathNode;

        int newMovementCostToNeighbor;

        public override Vector3Int FindPath(GridBase grid, Vector3Int start, Vector3Int end)
        {
            // Validate start and end positions
            if (!grid.IsValidGridPosition(start.x, start.y) || !grid.IsValidGridPosition(end.x, end.y))
            {
                return start; // Return start if invalid
            }

            if (start == end)
            {
                return start; // No movement needed
            }

            // Reset all nodes Parent, G, H, F values
            foreach (GridNode node in grid.GridArray)
            {
                node.ResetPathfindingValues();
            }

            openSet.Clear();
            openSet.Add(grid.GridArray[start.x, start.y]);

            closedSet.Clear();

            while (openSet.Count > 0)
            {
                currentNode = openSet[0];

                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].F < currentNode.F || openSet[i].F == currentNode.F && openSet[i].H < currentNode.H)
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode.LocalPosition == end)
                {
                    pathNode = currentNode;
                    while (pathNode.Parent != null)
                    {
                        if (pathNode.Parent.Parent == null)
                            break;
                        pathNode = pathNode.Parent;
                    }
                    return pathNode.LocalPosition;
                }

                neighbors = grid.GetNeighbors(currentNode);

                foreach (GridNode neighbor in neighbors)
                {
                    if ((neighbor.ChildGameObject != null && neighbor.LocalPosition != end) || closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    newMovementCostToNeighbor = currentNode.G + grid.GetDistance(currentNode, neighbor);

                    if (newMovementCostToNeighbor < neighbor.G || !openSet.Contains(neighbor))
                    {
                        neighbor.G = newMovementCostToNeighbor;
                        neighbor.H = grid.GetDistance(neighbor, grid.GridArray[end.x, end.y]);
                        neighbor.Parent = currentNode;

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            // No path found, return the start position
            return start;
        }
    }
}