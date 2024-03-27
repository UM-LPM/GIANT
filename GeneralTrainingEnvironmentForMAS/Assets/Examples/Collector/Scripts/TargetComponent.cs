using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetComponent : MonoBehaviour
{
    public CollectorEnvironmentController CollectorEnvironmentController { get; set; }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer != this.gameObject.layer)
            return;

        AgentComponent otherAgent;
        other.gameObject.TryGetComponent<AgentComponent>(out otherAgent);

        if (otherAgent != null) {
            CollectorEnvironmentController.TargetAquired(this, otherAgent);
            Destroy(this.gameObject);
        }

    }
}
