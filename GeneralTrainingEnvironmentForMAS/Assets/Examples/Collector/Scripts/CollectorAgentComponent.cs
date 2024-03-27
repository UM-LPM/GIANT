using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectorAgentComponent : AgentComponent {

    public Rigidbody Rigidbody { get; set; }
 
    public int TargetsAquired { get; set; }

    protected override void DefineAdditionalDataOnAwake() {
        Rigidbody = GetComponent<Rigidbody>();
    }
}