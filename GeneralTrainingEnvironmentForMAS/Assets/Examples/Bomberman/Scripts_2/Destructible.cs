using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField] float DestructionTime = 1f;
    [Range(0f, 1f)]
    [SerializeField] float ItemSpawnChance = 0.2f;
    [SerializeField] GameObject[] SpawnableItems;

    private void Start() {
        Destroy(gameObject, DestructionTime);
    }

    private void OnDestroy() {
        // Spawn an item
        if(SpawnableItems.Length > 0 && Random.value < ItemSpawnChance) {
            int index = Random.Range(0, SpawnableItems.Length);
            Instantiate(SpawnableItems[index], transform.position, Quaternion.identity);
        }
    }
}
