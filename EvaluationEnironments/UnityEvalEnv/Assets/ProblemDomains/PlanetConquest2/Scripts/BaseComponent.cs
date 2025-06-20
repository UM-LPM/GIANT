using Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.PlanetConquest2
{
    public class BaseComponent : MonoBehaviour
    {
        public TeamIdentifier TeamIdentifier { get; set; }
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

            TeamIdentifier = GetComponent<TeamIdentifier>();
            if (TeamIdentifier == null)
            {
                throw new System.Exception("TeamIdentifier component is missing");
                // TODO Add error reporting here
            }
        }

        public void TakeDamage(int value)
        {
            HealthComponent.Health -= value;
        }
    }
}
