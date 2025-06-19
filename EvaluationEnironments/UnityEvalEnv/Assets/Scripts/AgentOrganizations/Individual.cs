using AgentControllers;
using System;
using UnityEngine;


namespace AgentOrganizations
{
    [Serializable]
    [CreateAssetMenu(fileName ="Individual", menuName = "AgentOrganizations/Individual")]
    public class Individual : ScriptableObject
    {
        public int IndividualId;
        public AgentController[] AgentControllers;

        public Individual Clone()
        {
            Individual clone = CreateInstance<Individual>();
            clone.IndividualId = IndividualId;
            clone.AgentControllers = new AgentController[AgentControllers.Length];
            
            for (int i = 0; i < AgentControllers.Length; i++)
            {
                clone.AgentControllers[i] = AgentControllers[i].Clone();
            }
            
            return clone;
        }
    }
}