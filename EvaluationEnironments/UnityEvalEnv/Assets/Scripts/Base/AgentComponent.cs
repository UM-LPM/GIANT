using System.Collections.Generic;
using AgentControllers;
using UnityEngine;
using Fitnesses;

namespace Base
{
    [DisallowMultipleComponent]
    public abstract class AgentComponent : MonoBehaviour
    {

        [field: SerializeField, Header("Base Agent Configuration")]
        public AgentFitness AgentFitness { get; set; }
        public Vector3 StartPosition { get; set; }
        public Quaternion StartRotation { get; set; }
        public Vector3? LastSectorPosition { get; set; }
        public List<Vector3> LastKnownSectorPositions { get; set; }
        public ActionBuffer ActionBuffer { get; set; }

        // New properties
        [field: SerializeField]
        public AgentController AgentController { get; set; }

        public int IndividualID { get; set; }
        
        //public int TeamID { get; set; }
        public TeamIdentifier TeamIdentifier { get; set; }



        private void Awake()
        {
            AgentFitness = new AgentFitness();
            StartPosition = transform.position;
            StartRotation = transform.rotation;

            LastSectorPosition = null;
            LastKnownSectorPositions = new List<Vector3>
            {
                transform.position
            };

            TeamIdentifier = GetComponent<TeamIdentifier>();

            DefineAdditionalDataOnAwake();
        }

        private void Start()
        {
            DefineAdditionalDataOnStart();
        }

        protected virtual void DefineAdditionalDataOnAwake() { }
        protected virtual void DefineAdditionalDataOnStart() { }
    }
}