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

        private Collider2D[] MissileCollisions = new Collider2D[PhysicsUtil.DefaultColliderArraySize];
        private AgentComponent otherAgent;

        void Start()
        {
            RobostrikeEnvironmentController = gameObject.GetComponent<RobostrikeEnvironmentController>();
            Missiles = new List<MissileComponent>();
        }

        public void UpdateMissilePosAndCheckForColls()
        {
            for (int i = 0; i < Missiles.Count; i++)
            {
                // Update missile position and check if it's colliding with anything
                //Vector3 missileNewPos = Missiles[i].transform.position += Missiles[i].MissileVelocity * Time.fixedDeltaTime;
                //Missiles[i].transform.position += Missiles[i].MissileVelocity * Time.fixedDeltaTime;
                Missiles[i].transform.position = Missiles[i].transform.position + Missiles[i].MissileVelocity * Time.fixedDeltaTime;
            }

            for (int i = 0; i < Missiles.Count; i++)
            {
                CheckMissileCollision(Missiles[i]);
            }
        }

        void CheckMissileCollision(MissileComponent missileComponent)
        {
            ResetMissileCollisions();
            RobostrikeEnvironmentController.PhysicsScene2D.OverlapCircle(missileComponent.transform.position, MissileRadius, MissileCollisions, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)));

            foreach (Collider2D collision in MissileCollisions)
            {
                if (collision == null || 
                    collision.gameObject == missileComponent.Parent.gameObject || 
                    collision.gameObject == missileComponent.gameObject ||
                    collision.isTrigger)
                    continue;

                collision.gameObject.TryGetComponent(out otherAgent);

                if (otherAgent != null)
                {
                    RobostrikeEnvironmentController.TankHit(missileComponent, otherAgent);
                }

                missileComponent.MissileHitTarget = true;
                RemoveMissile(missileComponent);
                Destroy(missileComponent.gameObject);
            }
        }

        void ResetMissileCollisions()
        {
            for (int i = 0; i < MissileCollisions.Length; i++)
            {
                MissileCollisions[i] = null;
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