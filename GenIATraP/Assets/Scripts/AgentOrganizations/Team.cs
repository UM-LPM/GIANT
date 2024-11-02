using System;
using UnityEngine;

namespace AgentOrganizations
{
    [Serializable]
    [CreateAssetMenu(fileName = "Team", menuName = "AgentOrganizations/Team")]
    public class Team: ScriptableObject
    {
        public int TeamId;
        public string TeamName;
        public Individual[] Individuals;
    }
}