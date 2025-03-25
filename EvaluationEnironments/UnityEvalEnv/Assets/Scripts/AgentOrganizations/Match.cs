using System;
using UnityEngine;

namespace AgentOrganizations
{
    [Serializable]
    [CreateAssetMenu(fileName = "Match", menuName = "AgentOrganizations/Match")]
    public class Match : ScriptableObject
    {
        public int MatchId;
        public Team[] Teams;

        public Match(int matchId, Team[] teams)
        {
            MatchId = matchId;
            Teams = teams;
            name = "Match_" + matchId;
        }

        public virtual Match Initialize(int matchId, Team[] teams)
        {
            MatchId = matchId;
            Teams = teams;
            name = "Match_" + matchId;

            return this;
        }
    }
}