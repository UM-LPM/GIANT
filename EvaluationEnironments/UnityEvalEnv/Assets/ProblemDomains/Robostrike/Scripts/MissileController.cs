using Base;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Problems.Robostrike
{
    public class MissileController : MonoBehaviour
    {
        [SerializeField] float MissileRadius = 0.225f;
        private List<MissileComponent> Missiles;
        private RobostrikeEnvironmentController RobostrikeEnvironmentController;

        private AgentComponent otherAgent;
        private Vector2 dirrection;

        RaycastHit2D[] hits = new RaycastHit2D[32]; // Reusable array for storing raycast hits to avoid allocations

        ContactFilter2D filter;

        void Start()
        {
            RobostrikeEnvironmentController = gameObject.GetComponent<RobostrikeEnvironmentController>();
            Missiles = new List<MissileComponent>();

            filter = new ContactFilter2D()
            {
                layerMask = 1 << gameObject.layer,
                useTriggers = false
            };
        }

        public void UpdateMissilePosAndCheckForColls()
        {
            if (Time.timeScale == 0f) return; // Skip updates when the game is paused to avoid issues with missile movement and collision detection
            for (int i = 0; i < Missiles.Count; i++)
            {
                var newPos = Missiles[i].transform.position + Missiles[i].MissileVelocity * Time.fixedDeltaTime;
                dirrection.x = newPos.x - Missiles[i].transform.position.x;
                dirrection.y = newPos.y - Missiles[i].transform.position.y;

                // Check for collisions (with CircleCast) along the path of the bullet to avoid tunneling issues at high speeds
                int count = PhysicsUtil.PhysicsCircleCast2D(
                    RobostrikeEnvironmentController.PhysicsScene2D,
                    Missiles[i].gameObject,
                    Missiles[i].transform.position,
                    MissileRadius,
                    dirrection.normalized,
                    Vector2.Distance(newPos, Missiles[i].transform.position),
                    filter,
                    hits
                );

                var hasHit = false;
                for (int j = 0; j < count; j++)
                {
                    var hit = hits[j];

                    if (hit.collider != null &&
                        hit.collider.gameObject != Missiles[i].gameObject &&
                        hit.collider.gameObject != Missiles[i].Parent.gameObject)
                    {
                        hasHit = true;
                        Missiles[i].transform.position = hit.point;
                        MissileHitSomething(Missiles[i], hit);
                        break;
                    }
                }

                if (!hasHit)
                {
                    Missiles[i].transform.position = Missiles[i].transform.position + Missiles[i].MissileVelocity * Time.fixedDeltaTime;
                }
            }
        }

        void MissileHitSomething(MissileComponent missileComponent, RaycastHit2D hit)
        {
            hit.collider.gameObject.TryGetComponent(out otherAgent);
            if (otherAgent != null)
            {
                RobostrikeEnvironmentController.TankHit(missileComponent, otherAgent);
            }
            missileComponent.MissileHitTarget = true;

            RemoveMissile(missileComponent);
            Destroy(missileComponent.gameObject);
        }

        public void AddMissile(MissileComponent missile)
        {
            Missiles.Add(missile);
        }

        public void RemoveMissile(MissileComponent missile)
        {
            Missiles.Remove(missile);
        }
    }
}