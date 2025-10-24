using UnityEngine;

namespace Problems
{
    public abstract class Grid : MonoBehaviour
    {
        public abstract Component GetCellItem(int x, int y, int z);
    }
}