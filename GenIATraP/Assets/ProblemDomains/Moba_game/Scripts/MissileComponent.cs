using Base;
using Problems.Moba_game;
using UnityEngine;

namespace Problems.Moba_game
{
    public class MissileComponent : MonoBehaviour
    {
        public AgentComponent Parent { get; set; }
        public Moba_gameEnvironmentController Moba_gameEnvironmentController { get; set; }

        public Vector3 MissileVelocity { get; set; }

        public bool MissileHitTarget { get; set; }

        private void Start()
        {
            MissileHitTarget = false;
            Destroy(gameObject, Moba_gameEnvironmentController.DestroyMissileAfter);
        }

        private void OnDestroy()
        {
            if (!MissileHitTarget)
            {
                Moba_gameEnvironmentController.MissileController.RemoveMissile(this);
            }
        }
    }
}