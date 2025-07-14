using Base;
using System.Collections.Generic;
using System.Linq;
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using UnityEngine;

namespace Fitnesses
{
    public class IndividualFitness
    {
        public int IndividualID { get; set; }
        public float Value { get; set; }
        public Dictionary<string, float> IndividualValues { get; set; }

        public Dictionary<string, double> AdditionalValues { get; set; }
        public Dictionary<string, object> IndividualPerformanceData { get; set; }

        public IndividualFitness()
        {
            IndividualID = -1;
            Value = 0f;
            IndividualValues = new Dictionary<string, float>();
            AdditionalValues = new Dictionary<string, double>();
            IndividualPerformanceData = new Dictionary<string, object>();
        }

        public void AddAgentFitness(AgentComponent agent, bool includeNodeCallFrequencyCounts)
        {
            if (IndividualID == -1)
            {
                IndividualID = agent.IndividualID;
            }
            else if (IndividualID != agent.IndividualID)
            {
                throw new System.Exception("Individual ID does not match");
            }

            // Check if agent controller is of type BehaviourController
            if (agent.AgentController is BehaviorTreeAgentController behaviourController && includeNodeCallFrequencyCounts)
            {
                IndividualPerformanceData["NodeCallFrequencyCount"] = behaviourController.GetNodeCallFrequencies(false);
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
        public IndividualMatchResult AvgMatchResult { get; set; }
        public Dictionary<string, double> AdditionalValues { get; set; }

        public FinalIndividualFitness()
        {
            IndividualID = -1;
            Value = 0f;
            IndividualMatchResults = new List<IndividualMatchResult>();
            AvgMatchResult = null;
            AdditionalValues = new Dictionary<string, double>();
        }

        public void AddIndividualFitness(IndividualFitness individualFitness, string matchName, int[] opponentIDs, Dictionary<string, object> individualPerformanceData)
        {
            if (IndividualID == -1)
            {
                IndividualID = individualFitness.IndividualID;
            }
            else if (IndividualID != individualFitness.IndividualID)
            {
                throw new System.Exception("Individual ID does not match");
            }

            Value += individualFitness.Value;
            IndividualMatchResults.Add(new IndividualMatchResult()
            {
                MatchName = matchName,
                Value = individualFitness.Value,
                IndividualValues = individualFitness.IndividualValues,
                OpponentsIDs = opponentIDs,
                IndividualPerformanceData = individualPerformanceData
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

        public void CalculateAvgMatchResultFitness()
        {
            if (IndividualMatchResults.Count == 0)
            {
                return;
            }

            AvgMatchResult = new IndividualMatchResult()
            {
                MatchName = "Avg Match Result",
                Value = 0,
                IndividualValues = new Dictionary<string, float>()
            };

            foreach (var individualMatchResult in IndividualMatchResults)
            {
                foreach (var individualValue in individualMatchResult.IndividualValues)
                {
                    if (AvgMatchResult.IndividualValues.ContainsKey(individualValue.Key))
                    {
                        AvgMatchResult.IndividualValues[individualValue.Key] += individualValue.Value;
                    }
                    else
                    {
                        AvgMatchResult.IndividualValues.Add(individualValue.Key, individualValue.Value);
                    }
                }
            }

            Dictionary<string, float> individualValues = new Dictionary<string, float>();
            foreach (var individualValue in AvgMatchResult.IndividualValues)
            {
                individualValues.Add(individualValue.Key, individualValue.Value / IndividualMatchResults.Count);
            }

            AvgMatchResult.IndividualValues = individualValues;
        }
    }

    public class IndividualMatchResult
    {
        public int[] OpponentsIDs { get; set; }
        public string MatchName { get; set; }
        public float Value { get; set; }
        public Dictionary<string, float> IndividualValues { get; set; }
        public Dictionary<string, object> IndividualPerformanceData { get; set; }
    }

    public class FinalIndividualFitnessWrapper
    {
        public List<FinalIndividualFitness> FinalIndividualFitnesses { get; set; }

        public FinalIndividualFitnessWrapper()
        {
            FinalIndividualFitnesses = new List<FinalIndividualFitness>();
        }

        public void UpdateFinalIndividualFitnesses(IndividualFitness individualFitness, string matchName, int[] opponentIDs, Dictionary<string, object> individualPerformanceData)
        {
            FinalIndividualFitness finalIndividualFitness = FinalIndividualFitnesses.Find(x => x.IndividualID == individualFitness.IndividualID);
            if (finalIndividualFitness == null)
            {
                finalIndividualFitness = new FinalIndividualFitness();
                finalIndividualFitness.AddIndividualFitness(individualFitness, matchName, opponentIDs, individualPerformanceData);
                FinalIndividualFitnesses.Add(finalIndividualFitness);
            }
            else
            {
                finalIndividualFitness.AddIndividualFitness(individualFitness, matchName, opponentIDs, individualPerformanceData);
            }
        }

        public bool IndividualAlreadyAdded(int individualId)
        {
            return FinalIndividualFitnesses.Where(x => x.IndividualID == individualId).ToList().Count() > 0;
        }

        public void AddFinalIndividualFitness(FinalIndividualFitness finalIndividualFitness)
        {
            FinalIndividualFitnesses.Add(finalIndividualFitness);
        }
    }
}