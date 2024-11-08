using System.Collections.Generic;

namespace Fitnesses
{
    public class MatchFitness
    {
        public int MatchId { get; set; }
        public string MatchName { get; set; }
        public List<TeamFitness> TeamFitnesses { get; set; }
        public bool IsDummy { get; set; }

        public MatchFitness()
        {
            TeamFitnesses = new List<TeamFitness>();
            IsDummy = false;
        }

        public void AddAgentFitness(AgentComponent agent)
        {
            TeamFitness teamFitness = TeamFitnesses.Find(tf => tf.TeamID == agent.TeamID);
            if(teamFitness == null)
            {
                teamFitness = new TeamFitness();
                teamFitness.TeamID = agent.TeamID;
                TeamFitnesses.Add(teamFitness);
            }

            teamFitness.AddAgentFitness(agent);
        }

        public float[] GetTeamFitnesses()
        {
            float[] teamFitnesses = new float[TeamFitnesses.Count];
            for(int i = 0; i < TeamFitnesses.Count; i++)
            {
                teamFitnesses[i] = TeamFitnesses[i].GetTeamFitness();
            }

            return teamFitnesses;
        }
    }
}