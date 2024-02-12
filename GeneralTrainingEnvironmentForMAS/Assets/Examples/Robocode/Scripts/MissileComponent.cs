using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MissileComponent : MonoBehaviour {
    
    public AgentComponent Parent { get; set; }
    public RobocodeEnvironmentController RobocodeEnvironmentController{ get; set; }

    private bool MissileHitTarget { get; set; }

    private void Start() {
        MissileHitTarget = false;
        Destroy(this.gameObject, RobocodeEnvironmentController.DestroyMissileAfter);
    }

    private void OnTriggerEnter(Collider other) {
        if (Parent == null) {
            Destroy(this.gameObject);
            return;
        }

        if (other.gameObject == Parent.gameObject && other.gameObject.layer == this.gameObject.layer)
            return;

        AgentComponent otherAgent;
        other.gameObject.TryGetComponent<AgentComponent>(out otherAgent);
        
        if(otherAgent != null) {
            RobocodeEnvironmentController.TankHit(this, otherAgent);
        }
        else {
            RobocodeEnvironmentController.ObstacleMissedAgent(this);
        }

        MissileHitTarget = true;
        Destroy(this.gameObject);
    }

    private void OnDestroy() {
        if(!MissileHitTarget) {
            RobocodeEnvironmentController.ObstacleMissedAgent(this);
        }
    }
}