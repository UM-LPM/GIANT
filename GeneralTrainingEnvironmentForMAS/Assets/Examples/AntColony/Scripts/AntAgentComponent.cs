using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntAgentComponent : AgentComponent
{

    public Rigidbody Rigidbody { get; set; }
    public float Health { get; set; }
    public bool hasFood { get; set; }

    public PheromoneNodeComponent currentPheromoneNode { get; set; }
    public PheromoneTrailComponent pheromoneTrailComponent { get; set; }
    protected override void DefineAdditionalDataOnAwake()
    {
        Rigidbody = GetComponent<Rigidbody>();
      
    }
}
