using UnityEngine;

namespace Problems.Shooter
{
    public class  WeaponComponent : MonoBehaviour
    {
        [HideInInspector] public WeaponBulletSpawnPoint BulletSpawnPoint;

        private void Awake()
        {
            BulletSpawnPoint = GetComponentInChildren<WeaponBulletSpawnPoint>();
            if(BulletSpawnPoint == null)
            {
                throw new System.Exception("WeaponComponent requires a WeaponBulletSpawnPoint to be used as the BulletSpawnPoint.");
            }
        }
    }
}