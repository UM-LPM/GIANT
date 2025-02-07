using System.Collections.Generic;
using AgentControllers;
using UnityEngine;

namespace Problems.Moba_game
{
    [DisallowMultipleComponent]
    public abstract class GoldComponent : MonoBehaviour
    {

        [field: SerializeField, Header("Base Gold Configuration")]
        public Vector3 StartPosition { get; set; }
        public Quaternion StartRotation { get; set; }

        private void Awake()
        {
            StartPosition = transform.position;
            StartRotation = transform.rotation;

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