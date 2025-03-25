using UnityEngine;

namespace Problems.Robostrike
{
    public class PowerUpComponent : MonoBehaviour
    {
        public PowerUpType PowerUpType;
    }

    public enum PowerUpType
    {
        Health,
        Ammo,
        Shield
    }
}