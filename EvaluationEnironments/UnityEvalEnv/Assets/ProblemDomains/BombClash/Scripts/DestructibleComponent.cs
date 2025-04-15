using Base;
using UnityEngine;

namespace Problems.Bombclash
{
    public class DestructibleComponent : MonoBehaviour
    {
        private BombermanEnvironmentController BombermanEnvironmentController;

        private void Awake()
        {
            BombermanEnvironmentController = GetComponentInParent<BombermanEnvironmentController>();
        }

        private void Start()
        {
            Destroy(gameObject, BombermanEnvironmentController.DestructibleDestructionTime);
        }

        private void OnDestroy()
        {
            // Spawn an item (power up)
            if (BombermanEnvironmentController.SpawnableItems.Length > 0 && BombermanEnvironmentController.Util.NextDouble() < BombermanEnvironmentController.PowerUpSpawnChance)
            {
                int index = BombermanEnvironmentController.Util.NextInt(0, BombermanEnvironmentController.SpawnableItems.Length);
                GameObject item = Instantiate(BombermanEnvironmentController.SpawnableItems[index], transform.position, Quaternion.identity, transform.parent);
                EnvironmentControllerBase.SetLayerRecursively(item, gameObject.layer);
            }
        }
    }
}