using Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.Shooter
{
    public class ShooterAgentComponent : AgentComponent
    {
        public float NextShootTime { get; set; }

        public HealthComponent HealthComponent { get; set; }
        public ShieldComponent ShieldComponent { get; set; }
        public WeaponComponent WeaponComponent { get; set; }

        [HideInInspector] public Transform WeaponPlaceholder { get; private set; }

        private SpriteRenderer Renderer;

        // Agent fitness variables
        public bool HealthItemCollected { get; set; }
        public bool ShieldItemCollected { get; set; }
        public int SectorsExplored { get; set; }
        public int BulletsFired { get; set; }
        public int BulletsHitOpponent { get; set; }
        public int BulletsHitTeamMate { get; set; }
        public int HitByOpponentBullets { get; set; }
        public int OpponentsDefeated { get; set; }
        public int SurvivedSimulationSteps { get; set; }

        protected override void DefineAdditionalDataOnAwake()
        {
            HealthComponent = GetComponent<HealthComponent>();
            if(HealthComponent == null)
                throw new System.Exception("HealthComponent is required on ShooterAgentComponent");

            ShieldComponent = GetComponent<ShieldComponent>();
            if(ShieldComponent == null)
                throw new System.Exception("ShieldComponent is required on ShooterAgentComponent");

            WeaponPlaceholder = GetComponentInChildren<ShooterAgentWeaponPlaceholder>().transform;
            Renderer = GetComponent<SpriteRenderer>();

            SurvivedSimulationSteps = -1;
        }

        public void SetTeamColor(Color color)
        {
            if (Renderer != null)
            {
                Renderer.color = color;
            }
        }

        public bool HasWeapon()
        {
            return WeaponComponent != null;
        }

        public void EquipWeapon(WeaponComponent weapon)
        {
            WeaponComponent = weapon;
            WeaponComponent.transform.SetParent(WeaponPlaceholder);
            WeaponComponent.transform.localPosition = Vector3.zero;
            WeaponComponent.transform.localRotation = Quaternion.identity;
        }

        public void BulletFired()
        {
            BulletsFired++;
        }

        public void TakeDamage(int value)
        {
            if(ShieldComponent.Shield > 0)
                ShieldComponent.Shield -= value;
            else
                HealthComponent.Health -= value;
        }

        public void BulletHitOpponent()
        {
            BulletsHitOpponent++;
        }

        public void HitByOpponentBullet()
        {
            HitByOpponentBullets++;
        }

        public void BulletHitTeamMate()
        {
            BulletsHitTeamMate++;
        }
    }
}
