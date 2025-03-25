using Base;
using System.Collections.Generic;

namespace Fitnesses
{
    public class TeamFitness
    {
        public int TeamID { get; set; }
        public List<IndividualFitness> IndividualFitness { get; set;}

        public TeamFitness()
        {
            IndividualFitness = new List<IndividualFitness>();
        }

        public void AddAgentFitness(AgentComponent agent)
        {
            IndividualFitness individualFitness = IndividualFitness.Find(tf => tf.IndividualID == agent.IndividualID);
            if(individualFitness == null)
            {
                individualFitness = new IndividualFitness();
                individualFitness.IndividualID = agent.IndividualID;
                IndividualFitness.Add(individualFitness);
            }

            individualFitness.AddAgentFitness(agent);
        }

        public Dictionary<string, float> GetTeamIndividualValues()
        {
            Dictionary<string, float> teamIndividualValues = new Dictionary<string, float>();
            foreach (var individualFitness in IndividualFitness)
            {
                foreach (var individualValue in individualFitness.IndividualValues)
                {
                    if (teamIndividualValues.ContainsKey(individualValue.Key))
                    {
                        teamIndividualValues[individualValue.Key] += individualValue.Value;
                    }
                    else
                    {
                        teamIndividualValues.Add(individualValue.Key, individualValue.Value);
                    }
                }
            }
            return teamIndividualValues;
        }

        public float GetTeamFitness()
        {
            float teamFitness = 0.0f;
            foreach (var individualFitness in IndividualFitness)
            {
                teamFitness += individualFitness.GetIndividualFitness();
            }
            return teamFitness;
        }
    }
}