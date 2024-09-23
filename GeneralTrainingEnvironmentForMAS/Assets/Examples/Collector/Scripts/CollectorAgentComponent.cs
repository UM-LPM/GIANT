using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Collector
{
    /// <summary>
    /// Component for the Collector Agent that extends the AgentComponent and holds the Rigidbody of the Agent
    /// </summary>
    public class CollectorAgentComponent : AgentComponent
    {

        public Rigidbody Rigidbody { get; set; }

        public bool NearTarget{ get; set; }

        public int TargetsAquired { get; set; }

        public List<GameObject> HitObjects { get; set; }

        protected override void DefineAdditionalDataOnAwake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            HitObjects = new List<GameObject>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Obstacle"))
            {
                if (CollectorFitness.FitnessValues[CollectorFitness.Keys[(int)CollectorFitness.FitnessKeys.AgentTouchedStaticObject]] != 0 && (!HitObjects.Contains(collision.gameObject)))
                {
                    AgentFitness.Fitness.UpdateFitness((CollectorFitness.FitnessValues[CollectorFitness.Keys[(int)CollectorFitness.FitnessKeys.AgentTouchedStaticObject]]), CollectorFitness.FitnessKeys.AgentTouchedStaticObject.ToString());
                    HitObjects.Add(collision.gameObject);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            SectorComponent sectorComponent = other.gameObject.GetComponent<SectorComponent>();
            // New Sector Explored
            if (sectorComponent != null && AgentExploredNewSector(sectorComponent))
            {
                //Debug.Log("New Sector Explored"); // TODO Remove
                if (CollectorFitness.FitnessValues[CollectorFitness.Keys[(int)CollectorFitness.FitnessKeys.AgentExploredSector]] != 0)
                {
                    AgentFitness.Fitness.UpdateFitness((CollectorFitness.FitnessValues[CollectorFitness.Keys[(int)CollectorFitness.FitnessKeys.AgentExploredSector]]), CollectorFitness.FitnessKeys.AgentExploredSector.ToString());
                }
                
                // Add explored sector to the list of explored sectors
                LastKnownPositions.Add(sectorComponent.transform.position);
                return;
            }
            // Re-explored Sector
            else if (sectorComponent != null && !AgentExploredNewSector(sectorComponent))
            {
                if (CollectorFitness.FitnessValues[CollectorFitness.Keys[(int)CollectorFitness.FitnessKeys.AgentReExploredSector]] != 0)
                {
                    AgentFitness.Fitness.UpdateFitness((CollectorFitness.FitnessValues[CollectorFitness.Keys[(int)CollectorFitness.FitnessKeys.AgentReExploredSector]]), CollectorFitness.FitnessKeys.AgentReExploredSector.ToString());
                }
            }

            TargetComponent targetComponent = other.gameObject.GetComponentInParent<TargetComponent>();
            if (targetComponent != null && !NearTarget && other.gameObject.GetComponent<SphereCollider>() != null)
            {
                //Debug.Log("Near Target Enter"); // TODO Remove
                NearTarget = true;
                if (CollectorFitness.FitnessValues[CollectorFitness.Keys[(int)CollectorFitness.FitnessKeys.AgentNearTarget]] != 0)
                {
                    AgentFitness.Fitness.UpdateFitness((CollectorFitness.FitnessValues[CollectorFitness.Keys[(int)CollectorFitness.FitnessKeys.AgentNearTarget]]), CollectorFitness.FitnessKeys.AgentNearTarget.ToString());
                }
                // Disable collider to prevent multiple triggers
                other.gameObject.GetComponent<SphereCollider>().enabled = false;
            }
        }

        private bool AgentExploredNewSector(SectorComponent sectorComponent)
        {
            return !LastKnownPositions.Contains(sectorComponent.transform.position);
        }
    }
}