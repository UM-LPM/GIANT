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

        void Start()
        {
            RobostrikeEnvironmentController = gameObject.GetComponent<RobostrikeEnvironmentController>();
            Missiles = new List<MissileComponent>();
        }

        public void UpdateMissilePosAndCheckForColls()
        {
            if(Time.timeScale == 0f) return; // Skip updates when the game is paused to avoid issues with missile movement and collision detection
            for (int i = 0; i < Missiles.Count; i++)
            {
                var newPos = Missiles[i].transform.position + Missiles[i].MissileVelocity * Time.fixedDeltaTime;
                dirrection.x = newPos.x - Missiles[i].transform.position.x;
                dirrection.y = newPos.y - Missiles[i].transform.position.y;

                // Check for collisions (with CircleCast) along the path of the bullet to avoid tunneling issues at high speeds
                var hits = PhysicsUtil.PhysicsCircleCast2D(
                    RobostrikeEnvironmentController.PhysicsScene2D,
                    Missiles[i].gameObject,
                    Missiles[i].transform.position,
                    MissileRadius,
                    dirrection.normalized,
                    Vector2.Distance(newPos, Missiles[i].transform.position),
                    true,
                    LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer))
                );

                if (hits.Length > 0 && hits[0].collider != null &&
                    hits[0].collider.gameObject != Missiles[i].gameObject &&
                    hits[0].collider.gameObject != Missiles[i].Parent.gameObject)
                {
                    Missiles[i].transform.position = hits[0].point;
                    MissileHitSomething(Missiles[i], hits);
                }
                else
                {
                    Missiles[i].transform.position = Missiles[i].transform.position + Missiles[i].MissileVelocity * Time.fixedDeltaTime;
                }
            }
        }

        void MissileHitSomething(MissileComponent missileComponent, RaycastHit2D[] hits)
        {
            foreach(RaycastHit2D hit in hits)
            {
                if (hit.collider == null ||
                    hit.collider.gameObject == missileComponent.Parent.gameObject ||
                    hit.collider.gameObject == missileComponent.gameObject)
                    continue;
                hit.collider.gameObject.TryGetComponent(out otherAgent);
                if (otherAgent != null)
                {
                    RobostrikeEnvironmentController.TankHit(missileComponent, otherAgent);
                }
                missileComponent.MissileHitTarget = true;
                RemoveMissile(missileComponent);
                Destroy(missileComponent.gameObject);
            }
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