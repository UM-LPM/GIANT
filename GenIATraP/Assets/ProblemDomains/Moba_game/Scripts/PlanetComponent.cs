using System.Collections.Generic;
using AgentControllers;
using UnityEngine;

namespace Problems.Moba_game
{
    [DisallowMultipleComponent]
    public abstract class PlanetComponent : MonoBehaviour
    {
        private void Awake()
        {
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