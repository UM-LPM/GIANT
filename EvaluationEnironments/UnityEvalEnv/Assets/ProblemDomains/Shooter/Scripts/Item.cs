using UnityEngine;

namespace Problems.Shooter {
    public abstract class Item : MonoBehaviour
    {
        public abstract ItemType getType();
    }

    public enum ItemType
    {
        Weapon,
        Health,
        Shield,
    }
}