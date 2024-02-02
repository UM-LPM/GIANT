using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SoccerAgentComponent : AgentComponent
{
    public Rigidbody Rigidbody { get; set; }
    public Team Team { get; set; }
    public float KickPower { get; set; }
    public Vector3 StartPosition { get; set; }
    public Quaternion StartRotation { get; set; }
    SoccerEnvironmentController SoccerEnvironmentController { get; set; }

    protected override void DefineAdditionalDataOnAwake() {
        Rigidbody = GetComponent<Rigidbody>();
        StartPosition = transform.position;
        StartRotation = transform.rotation;
        SoccerEnvironmentController = GetComponentInParent<SoccerEnvironmentController>();
    }

    void OnCollisionEnter(Collision c) {
        SoccerBallComponent soccerBall = c.gameObject.GetComponent<SoccerBallComponent>();
        
        if(soccerBall != null) {
            soccerBall.LastTouchedAgent = this;

            var force = SoccerEnvironmentController.KickPower * KickPower;
            var dir = c.contacts[0].point - transform.position;
            dir = dir.normalized;
            soccerBall.Rigidbody.AddForce(dir * force);

            SoccerEnvironmentController.AgentTouchedSoccerBall(this, soccerBall);
        }
    }
}
