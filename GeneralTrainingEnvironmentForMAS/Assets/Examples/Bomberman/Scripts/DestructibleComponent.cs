using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DestructibleComponent : MonoBehaviour {
    [SerializeField] float DestructionTime = 1f;
    [Range(0f, 1f)]
    [SerializeField] float ItemSpawnChance = 0.2f;
    [SerializeField] GameObject[] SpawnableItems;

    public Util Util { get; set; }

    private void Start() {
        Destroy(gameObject, DestructionTime);
    }

    private void OnDestroy() {
        // Spawn an item
        if (SpawnableItems.Length > 0 && Util.rnd.NextDouble() < ItemSpawnChance) {
            int index = Util.rnd.Next(0, SpawnableItems.Length);
            GameObject item = Instantiate(SpawnableItems[index], transform.position, Quaternion.identity, transform.parent);
            EnvironmentControllerBase.SetLayerRecursively(item, gameObject.layer);
        }
    }
}
