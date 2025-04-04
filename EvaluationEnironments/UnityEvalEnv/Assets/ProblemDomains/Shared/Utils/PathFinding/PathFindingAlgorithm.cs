using UnityEngine;
using Problems.Utils.GridSystem;

namespace Problems.Utils.PathFinding
{
    public abstract class PathFindingAlgorithm : MonoBehaviour
    {
        public abstract Vector3Int FindPath(GridBase grid, Vector3Int start, Vector3Int end);
    }
}