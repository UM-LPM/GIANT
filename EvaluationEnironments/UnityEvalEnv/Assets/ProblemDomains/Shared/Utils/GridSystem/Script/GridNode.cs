using UnityEngine;

namespace Problems.Utils.GridSystem
{
    public class GridNode
    {
        public Vector3Int LocalPosition { get; set; }
        public GameObject NodeGameObject { get; set; }
        public GameObject ChildGameObject { get; set; }
        public bool IsWalkable { get; set; }

        public GridNode Parent { get; set; }
        public int G { get; set; } // Cost from start
        public int H { get; set; } // Heuristic cost to end
        public int F => G + H; // Total cost

        public GridNode(Vector3Int localPosition, GameObject nodeGameObject)
        {
            LocalPosition = localPosition;
            NodeGameObject = nodeGameObject;
            ChildGameObject = null;
            IsWalkable = true;
        }

        public void Show()
        {
            NodeGameObject.transform.Find("Selected").gameObject.SetActive(true);
        }

        public void Hide()
        {
            NodeGameObject.transform.Find("Selected").gameObject.SetActive(false);
        }

        public void ResetPathfindingValues()
        {
            Parent = null;
            G = 0;
            H = 0;
        }
    }
}