using UnityEngine;

namespace Problems.BombClash
{
    public class ItemPickupComponent : MonoBehaviour
    {
        public enum ItemType
        {
            ExtraBomb,
            BlastRadius,
            SpeedIncrease
        }

        [SerializeField] ItemType Type;

        public void OnItemPickup(BombClashAgentComponent agent)
        {
            switch (Type)
            {
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

            agent.PowerUpsCollected++;

            Destroy(gameObject);
        }
    }
}