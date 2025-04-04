using UnityEngine;

namespace Problems.TowerDefense
{
    public class EnemyComponent: MonoBehaviour
    {
        [SerializeField] private EnemyType enemyType;
    }

    public enum EnemyType
    {
        Weak,
        Normal,
        Strong
    }
}