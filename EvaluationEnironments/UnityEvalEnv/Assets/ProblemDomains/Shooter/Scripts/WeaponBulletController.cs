using Base;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Problems.Shooter
{
    public class WeaponBulletController : MonoBehaviour
    {
        [SerializeField] float BulletRadius = 0.1f;
        private List<WeaponBulletComponent> Bullets;
        private ShooterEnvironmentController ShooterEnvironmentController;

        private AgentComponent otherAgent;
        private Vector2 dirrection;

        void Start()
        {
            ShooterEnvironmentController = gameObject.GetComponent<ShooterEnvironmentController>();
            Bullets = new List<WeaponBulletComponent>();
        }

        public void UpdateBulletPosAndCheckForColls()
        {
            for (int i = 0; i < Bullets.Count; i++)
            {
                var newPos = Bullets[i].transform.position + Bullets[i].Velocity * Time.fixedDeltaTime;
                dirrection.x = newPos.x - Bullets[i].transform.position.x;
                dirrection.y = newPos.y - Bullets[i].transform.position.y;

                // Check for collisions (with CircleCast) along the path of the bullet to avoid tunneling issues at high speeds
                var hits = PhysicsUtil.PhysicsCircleCast2D(
                    ShooterEnvironmentController.PhysicsScene2D,
                    Bullets[i].gameObject,
                    Bullets[i].transform.position,
                    BulletRadius,
                    dirrection.normalized,
                    Vector2.Distance(newPos, Bullets[i].transform.position),
                    true,
                    LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer))
                );

                if (hits.Length > 0 && hits[0].collider != null &&
                    hits[0].collider.gameObject != Bullets[i].gameObject &&
                    hits[0].collider.gameObject != Bullets[i].Parent.gameObject)
                {
                    Bullets[i].transform.position = hits[0].point;
                    BulletHitSomething(Bullets[i], hits);
                }
                else
                {
                    Bullets[i].transform.position = Bullets[i].transform.position + Bullets[i].Velocity * Time.fixedDeltaTime;
                }
            }
        }

        void BulletHitSomething(WeaponBulletComponent bulletComponent, RaycastHit2D[] hits)
        {
            foreach(RaycastHit2D hit in hits)
            {
                if (hit.collider == null ||
                    hit.collider.gameObject == bulletComponent.Parent.gameObject ||
                    hit.collider.gameObject == bulletComponent.gameObject)
                    continue;

                hit.collider.gameObject.TryGetComponent(out otherAgent);
                if (otherAgent != null)
                {
                    ShooterEnvironmentController.AgentHit(bulletComponent, otherAgent);
                }
                //bulletComponent.BulletHitTarget = true;
                RemoveBullet(bulletComponent);
                Destroy(bulletComponent.gameObject);
            }
        }

        public void AddBullet(WeaponBulletComponent bullet)
        {
            Bullets.Add(bullet);
        }

        public void RemoveBullet(WeaponBulletComponent bullet)
        {
            Bullets.Remove(bullet);
        }
    }
}