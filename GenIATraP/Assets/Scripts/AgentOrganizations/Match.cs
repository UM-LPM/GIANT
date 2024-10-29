using System.IO;
using UnityEngine;

namespace AgentOrganizations
{
    [CreateAssetMenu(menuName = "AgentOrganizations/Match")]
    public class Match : ScriptableObject
    {
        public int MatchId;
        public Team[] Teams;
        public MatchType MatchType;
    }

    public enum MatchType
    {
        _Xvs0,
        _XvsY,

    }
}