using Base;
using System.Collections.Generic;
using UnityEngine;

namespace Fitnesses
{
    public class IndividualFitness
    {
        public int IndividualID { get; set; }
        public float Value { get; set; }
        public Dictionary<string, float> IndividualValues { get; set; }

        public Dictionary<string, float> AdditionalValues { get; set; }

        public IndividualFitness()
        {
            IndividualID = -1;
            Value = 0f;
            IndividualValues = new Dictionary<string, float>();
            AdditionalValues = new Dictionary<string, float>();
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

        public float GetIndividualFitness()
        {
            return Value;
        }

        public void UpdateAdditionalValues(string key, float value)
        {
            if (AdditionalValues.ContainsKey(key))
            {
                AdditionalValues[key] += value;
            }
            else
            {
                AdditionalValues.Add(key, value);
            }
        }
    }

    public class FinalIndividualFitness
    {
        public int IndividualID { get; set; }
        public float Value { get; set; }
        public List<IndividualMatchResult> IndividualMatchResults { get; set; }
        public Dictionary<string, float> AdditionalValues { get; set; }

        public FinalIndividualFitness()
        {
            IndividualID = -1;
            Value = 0f;
            IndividualMatchResults = new List<IndividualMatchResult>();
            AdditionalValues = new Dictionary<string, float>();
        }

        public void AddIndividualFitness(IndividualFitness individualFitness, string matchName)
        {
            if (IndividualID == -1)
            {
                IndividualID = individualFitness.IndividualID;
            }
            else if (IndividualID != individualFitness.IndividualID)
            {
                throw new System.Exception("Individual ID does not match");
                // TODO Add error reporting here
            }

            Value += individualFitness.Value;
            IndividualMatchResults.Add(new IndividualMatchResult()
            {
                MatchName = matchName,
                Value = individualFitness.Value,
                IndividualValues = individualFitness.IndividualValues,
                OpponentsIDs = new int[] { } // TODO Add support in the future
            });

            // Update additional values
            foreach (var additionalValue in individualFitness.AdditionalValues)
            {
                if (AdditionalValues.ContainsKey(additionalValue.Key))
                {
                    AdditionalValues[additionalValue.Key] += additionalValue.Value;
                }
                else
                {
                    AdditionalValues.Add(additionalValue.Key, additionalValue.Value);
                }
            }
        }
    }

    public class IndividualMatchResult
    {
        public int[] OpponentsIDs { get; set; }
        public string MatchName { get; set; }
        public float Value { get; set; }
        public Dictionary<string, float> IndividualValues { get; set; }
    }

    public class FinalIndividualFitnessWrapper
    {
        public List<FinalIndividualFitness> FinalIndividualFitnesses { get; set; }

        public FinalIndividualFitnessWrapper()
        {
            FinalIndividualFitnesses = new List<FinalIndividualFitness>();
        }

        public void UpdateFinalIndividualFitnesses(IndividualFitness individualFitness, string matchName)
        {
            FinalIndividualFitness finalIndividualFitness = FinalIndividualFitnesses.Find(x => x.IndividualID == individualFitness.IndividualID);
            if (finalIndividualFitness == null)
            {
                finalIndividualFitness = new FinalIndividualFitness();
                finalIndividualFitness.AddIndividualFitness(individualFitness, matchName);
                FinalIndividualFitnesses.Add(finalIndividualFitness);
            }
            else
            {
                finalIndividualFitness.AddIndividualFitness(individualFitness, matchName);
            }
        }
    }
}