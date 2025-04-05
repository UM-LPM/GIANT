using System.Collections.Generic;
using AgentControllers;
using UnityEngine;

namespace Problems.PlanetConquest
{
    [DisallowMultipleComponent]
    public class BaseComponent : MonoBehaviour
    {
        public HealthComponent HealthComponent { get; set; }
        public int LavaAmount;
        public int IceAmount;

        private void Awake()
        {
            HealthComponent = GetComponent<HealthComponent>();
            if (HealthComponent == null)
            {
                throw new System.Exception("HealthComponent component is missing");
                // TODO Add error reporting here
            }
        }

        public void TakeDamage(int value)
        {
            HealthComponent.Health -= value;
        }
    }
}