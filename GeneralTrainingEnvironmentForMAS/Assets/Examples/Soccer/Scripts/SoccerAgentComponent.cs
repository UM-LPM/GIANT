using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoccerAgentComponent : AgentComponent
{
    public Rigidbody Rigidbody { get; set; }

    protected override void DefineAdditionalDataOnAwake() {
        Rigidbody = GetComponent<Rigidbody>();
    }
}
