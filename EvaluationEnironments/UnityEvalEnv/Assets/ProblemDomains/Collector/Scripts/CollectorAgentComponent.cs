
using Base;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.Collector
{
    public class CollectorAgentComponent : AgentComponent
    {

        public int TargetsAquired { get; set; }
        public int SectorsExplored { get; set; }
    }
}