using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum ItemType {
        ExtraBomb,
        BlastRadius,
        SpeedIncrease
    }

    [SerializeField] ItemType Type;


    void OnItemPickup(GameObject agent) {
        switch(Type) {
            case ItemType.ExtraBomb:
                agent.GetComponent<BombController>().AddBomb(); 
                break;
            case ItemType.BlastRadius:
                agent.GetComponent<BombController>().ExplosionRadius++;
                break;
            case ItemType.SpeedIncrease:
                agent.GetComponent<MovementController>().Speed++;
                break;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Agent")) {
            OnItemPickup(collision.gameObject);
        }
    }
}
