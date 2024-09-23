using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MissileComponent : MonoBehaviour {
    
    public AgentComponent Parent { get; set; }
    public RobostrikeEnvironmentController RobostrikeEnvironmentController{ get; set; }

    public Vector3 MissileVelocity { get; set; }

    public bool MissileHitTarget { get; set; }

    private void Start() {
        MissileHitTarget = false;
        Destroy(this.gameObject, RobostrikeEnvironmentController.DestroyMissileAfter);
    }

    private void OnTriggerEnter(Collider other) {
        if (Parent == null) {
            Destroy(this.gameObject);
            return;
        }

        if (other.gameObject == Parent.gameObject && other.gameObject.layer == this.gameObject.layer || other.GetComponent<PowerUpComponent>() != null)
            return;

        AgentComponent otherAgent;
        other.gameObject.TryGetComponent<AgentComponent>(out otherAgent);
        
        if(otherAgent != null) {
            RobostrikeEnvironmentController.TankHit(this, otherAgent);
        }
        else {
            RobostrikeEnvironmentController.ObstacleMissedAgent(this);
        }

        MissileHitTarget = true;
        Destroy(this.gameObject);
    }

    private void OnDestroy() {
        if(!MissileHitTarget) {
            RobostrikeEnvironmentController.ObstacleMissedAgent(this);
            RobostrikeEnvironmentController.getMissileController().RemoveMissile(this);
        }
    }
}