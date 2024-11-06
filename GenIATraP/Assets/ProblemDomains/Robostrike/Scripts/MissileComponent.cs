using Problems.Robostrike;
using UnityEngine;

namespace Problems.Robostrike
{
    public class MissileComponent : MonoBehaviour
    {
        public AgentComponent Parent { get; set; }
        public RobostrikeEnvironmentController RobostrikeEnvironmentController { get; set; }

        public Vector3 MissileVelocity { get; set; }

        public bool MissileHitTarget { get; set; }

        private void Start()
        {
            MissileHitTarget = false;
            Destroy(this.gameObject, RobostrikeEnvironmentController.DestroyMissileAfter);
        }

        private void OnDestroy()
        {
            if (!MissileHitTarget)
            {
                // TODO Implement (Trigger instance level event)
                //RobostrikeEnvironmentController.ObstacleMissedAgent(this);
                //RobostrikeEnvironmentController.getMissileController().RemoveMissile(this);
            }
        }
    }
}