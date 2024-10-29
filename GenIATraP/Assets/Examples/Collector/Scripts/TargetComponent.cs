using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Collector
{
    /// <summary>
    /// Tag Component for Target
    /// </summary>
    public class TargetComponent : MonoBehaviour
    {
        public CollectorEnvironmentController CollectorEnvironmentController { get; set; }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != this.gameObject.layer)
                return;

            AgentComponent otherAgent;
            other.gameObject.TryGetComponent<AgentComponent>(out otherAgent);

            if (otherAgent != null)
            {
                Destroy(this.gameObject);
                CollectorEnvironmentController.TargetAquired(otherAgent);
            }

        }
    }
}
