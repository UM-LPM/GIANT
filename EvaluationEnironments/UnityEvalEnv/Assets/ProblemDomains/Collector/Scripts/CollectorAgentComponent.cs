
using Base;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.Collector
{
    public class CollectorAgentComponent : AgentComponent
    {

        public Rigidbody Rigidbody { get; set; }

        public int TargetsAquired { get; set; }

        protected override void DefineAdditionalDataOnAwake()
        {
            Rigidbody = GetComponent<Rigidbody>();
        }
    }
}