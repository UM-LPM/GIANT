using AgentControllers;
using UnityEngine;


namespace AgentOrganizations
{
    [CreateAssetMenu(menuName = "AgentOrganizations/Individual")]
    public class Individual : ScriptableObject
    {
        public AgentController[] AgentControllers;
    }
}