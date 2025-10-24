using Base;
using UnityEngine;

namespace Problems.BombClash
{
    public class DestructibleComponent : MonoBehaviour
    {
        public Vector2Int Position { get; set; }
    }

    public class ActiveDestructible
    {
        public DestructibleComponent DestructibleComponent;
        public int TimeToDestruction;

        public ActiveDestructible(DestructibleComponent destructibleComponent, int timeToDestruction)
        {
            DestructibleComponent = destructibleComponent;
            TimeToDestruction = timeToDestruction;
        }

        public bool DecreaseTime()
        {
            TimeToDestruction--;
            return TimeToDestruction <= 0f;
        }
    }
}