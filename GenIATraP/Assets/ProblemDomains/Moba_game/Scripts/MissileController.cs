using Base;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Problems.Moba_game
{
    public class MissileController : MonoBehaviour
    {
        [SerializeField] float MissileRadius = 0.225f;
        private List<MissileComponent> Missiles;
        private Moba_gameEnvironmentController Moba_gameEnvironmentController;

        private Collider2D[] MissileCollisions;
        private AgentComponent otherAgent;
        private BaseComponent otherBase;
        private GoldComponent otherGold;

        void Start()
        {
            Moba_gameEnvironmentController = gameObject.GetComponent<Moba_gameEnvironmentController>();
            Missiles = new List<MissileComponent>();
        }

        public void UpdateMissilePosAndCheckForColls()
        {
            for (int i = 0; i < Missiles.Count; i++)
            {
                // Update missile position and check if it's colliding with anything
                //Vector3 missileNewPos = Missiles[i].transform.position += Missiles[i].MissileVelocity * Time.fixedDeltaTime;
                //Missiles[i].transform.position += Missiles[i].MissileVelocity * Time.fixedDeltaTime;
                Missiles[i].transform.position = UnityUtils.RoundToDecimals(Missiles[i].transform.position + Missiles[i].MissileVelocity * Time.fixedDeltaTime, 2);

                CheckMissileCollision(Missiles[i]);
            }
        }

        void CheckMissileCollision(MissileComponent missileComponent)
        {
            MissileCollisions = Physics2D.OverlapCircleAll(missileComponent.transform.position, MissileRadius, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)) + Moba_gameEnvironmentController.DefaultLayer);
            if (MissileCollisions.Length > 0)
            {
                foreach (Collider2D collision in MissileCollisions)
                {
                    if (collision.gameObject == missileComponent.Parent.gameObject || collision.gameObject == missileComponent.gameObject || collision.isTrigger)
                        continue;

                    collision.gameObject.TryGetComponent(out otherAgent);
                    collision.gameObject.TryGetComponent(out otherBase);
                    collision.gameObject.TryGetComponent(out otherGold);


                    if (otherAgent != null)
                    {
                        Moba_gameEnvironmentController.TankHit(missileComponent, otherAgent);
                    }
                    
                    if (otherBase != null)
                    {
                        Moba_gameEnvironmentController.BaseHit(missileComponent, otherBase);
                    }

                    missileComponent.MissileHitTarget = true;
                    RemoveMissile(missileComponent);
                    Destroy(missileComponent.gameObject);
                }
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