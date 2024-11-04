using System;
using System.IO;
using UnityEngine;

namespace AgentOrganizations
{
    [Serializable]
    [CreateAssetMenu(fileName = "Match", menuName = "AgentOrganizations/Match")]
    public class Match : ScriptableObject
    {
        public int MatchId;
        public Team[] Teams;
        //public MatchType MatchType; // TODO Add support in the future 

        public Match(int matchId, Team[] teams)
        {
            MatchId = matchId;
            Teams = teams;
        }
    }

    /*public enum MatchType
    {
        _Xvs0,
        _XvsY,

    }*/
}