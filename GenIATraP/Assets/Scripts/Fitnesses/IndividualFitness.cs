using System.Collections.Generic;

namespace Fitnesses
{
    public class IndividualFitness
    {
        public int IndividualID { get; set; }
        public float Value { get; set; }
        public Dictionary<string, float> IndividualValues { get; set; }

        public IndividualFitness()
        {
            IndividualID = -1;
            Value = 0f;
            IndividualValues = new Dictionary<string, float>();
        }

        public void AddAgentFitness(AgentComponent agent)
        {
            if (IndividualID == -1)
            {
                IndividualID = agent.IndividualID;
            }
            else if (IndividualID != agent.IndividualID)
            {
                throw new System.Exception("Individual ID does not match");
                // TODO Add error reporting here
            }

            UpdateFitness(agent.AgentFitness);
        }

        public void UpdateFitness(AgentFitness agentFitness)
        {
            UpdateFitness(agentFitness.IndividualValues);
        }

        public void UpdateFitness(Dictionary<string, float> individualValues)
        {
            foreach (var individualValue in individualValues)
            {
                UpdateFitness(individualValue.Value, individualValue.Key);
            }
        }

        public void UpdateFitness(float value, string keyValue)
        {
            Value += value;

            if (value != 0)
            {
                // Update individual values
                if (IndividualValues.ContainsKey(keyValue))
                {
                    IndividualValues[keyValue] += value;
                }
                else
                {
                    IndividualValues.Add(keyValue, value);
                }
            }
        }

        // TODO Implement additional methods
    }
}