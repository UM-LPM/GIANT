using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntAgentComponent : AgentComponent
{

    public Rigidbody Rigidbody { get; set; }
    public float Health { get; set; }

    protected override void DefineAdditionalDataOnAwake()
    {
        Rigidbody = GetComponent<Rigidbody>();
      
    }
}
