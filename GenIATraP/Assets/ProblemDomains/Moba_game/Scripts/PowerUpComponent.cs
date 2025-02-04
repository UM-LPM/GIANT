using UnityEngine;

namespace Problems.Moba_game
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