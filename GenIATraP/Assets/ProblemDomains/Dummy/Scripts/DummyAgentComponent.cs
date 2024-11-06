
using System.Collections.Generic;
using UnityEngine;

namespace Problems.Dummy
{
    public class DummyAgentComponent : AgentComponent
    {

        public Rigidbody Rigidbody { get; set; }

        public int TargetsAquired { get; set; }

        protected override void DefineAdditionalDataOnAwake()
        {
            Rigidbody = GetComponent<Rigidbody>();
        }
    }
}