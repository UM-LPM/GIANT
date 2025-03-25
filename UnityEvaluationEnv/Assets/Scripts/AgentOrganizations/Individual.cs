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
    }
}