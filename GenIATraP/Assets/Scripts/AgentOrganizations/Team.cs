
using UnityEngine;

namespace AgentOrganizations
{
    [CreateAssetMenu(menuName = "AgentOrganizations/Team")]
    public class Team: ScriptableObject
    {
        public string TeamName;
        public Individual[] Individuals;
    }
}