using UnityEngine;

namespace Problems.Shooter
{
    public class WeaponBulletComponent: MonoBehaviour
    {
        public ShooterAgentComponent Parent { get; set; }
        public Vector3 Velocity { get; set; }
    }
}