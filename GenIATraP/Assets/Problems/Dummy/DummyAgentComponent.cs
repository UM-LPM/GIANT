
using System.Collections.Generic;
using UnityEngine;

namespace Problems.Dummy
{
    public class DummyAgentComponent : AgentComponent
    {

        public Rigidbody Rigidbody { get; set; }

        public bool NearTarget { get; set; }

        public int TargetsAquired { get; set; }

        public List<GameObject> HitObjects { get; set; }

        protected override void DefineAdditionalDataOnAwake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            HitObjects = new List<GameObject>();
        }
    }
}