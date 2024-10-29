
using UnityEngine;

namespace AgentOrganizations
{
    [CreateAssetMenu(menuName = "AgentOrganizations/Match")]
    public class Match : ScriptableObject
    {
        public Team[] Teams;
    }
}