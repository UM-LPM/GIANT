using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ItemPickupComponent : MonoBehaviour {
    public enum ItemType {
        ExtraBomb,
        BlastRadius,
        SpeedIncrease
    }

    [SerializeField] ItemType Type;

    public void OnItemPickup(BombermanAgentComponent agent) {
        switch (Type) {
            case ItemType.ExtraBomb:
                agent.AddBomb();
                break;
            case ItemType.BlastRadius:
                agent.ExplosionRadius++;
                break;
            case ItemType.SpeedIncrease:
                agent.MoveSpeed++;
                break;
        }

        agent.AgentFitness.Fitness.UpdateFitness(BombermanFitness.POWER_UP_COLECTED);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        BombermanAgentComponent agent;
        if(collision.gameObject.TryGetComponent<BombermanAgentComponent>(out agent)) {
            OnItemPickup(agent);
        }
    }
}
