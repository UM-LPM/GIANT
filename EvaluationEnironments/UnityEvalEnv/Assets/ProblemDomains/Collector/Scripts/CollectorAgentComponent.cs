
using Base;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.Collector
{
    public class CollectorAgentComponent : AgentComponent
    {

        public Rigidbody Rigidbody { get; set; }

        public CollectorEnvironmentController CollectorEnvironmentController { get; set; }

        List<SectorComponent> ExploredSectors;

        // Temp variables
        SectorComponent sector;
        TargetComponent target;

        // Agent fitness variables
        public int SectorsExplored { get; set; }

        public int TargetsAcquired { get; set; }

        protected override void DefineAdditionalDataOnAwake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            ExploredSectors = new List<SectorComponent>();
            CollectorEnvironmentController = GetComponentInParent<CollectorEnvironmentController>();
        }

        void OnTriggerEnter(Collider c)
        {
            if(c.gameObject.TryGetComponent(out sector))
            {
                if (!ExploredSectors.Contains(sector))
                {
                    ExploredSectors.Add(sector);
                    SectorsExplored++;
                }
            }

            else if(c.gameObject.TryGetComponent(out target))
            {
                if (target != null)
                {
                    TargetsAcquired++;
                    CollectorEnvironmentController.TargetAcquired(target);
                }
            }
        }
    }
}