using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitnesses
{
    [Serializable]
    public class AgentFitness
    {
        public float Value { get; set; }
        public Dictionary<string, float> IndividualValues { get; set; }


        public AgentFitness()
        {
            this.Value = 0f;
            this.IndividualValues = new Dictionary<string, float>();
        }

        public AgentFitness(float startValue)
        {
            this.Value = startValue;
            this.IndividualValues = new Dictionary<string, float>();
        }

        public float GetFitness()
        {
            return Value;
        }

        public Dictionary<string, float> GetIndividualFitnessValues()
        {
            return IndividualValues;
        }

        public void SetFitness(float value)
        {
            this.Value = value;
        }

        public virtual void UpdateFitness(float value, string keyValue)
        {
            this.Value += value;

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
    }
}


/*public class AgentFitness {
    public float Value { get; set; }
    public Dictionary<string, float> IndividualValues { get; set; }


    public AgentFitness() {
        this.Value = 0f;
        this.IndividualValues = new Dictionary<string, float>();
    }

    public AgentFitness(float startValue) {
        this.Value = startValue;
        this.IndividualValues = new Dictionary<string, float>();
    }

    public float GetFitness() {
        return Value;
    }

    public Dictionary<string, float> GetIndividualFitnessValues() {
        return IndividualValues;
    }

    public void SetFitness(float value) {
        this.Value = value;
    }

    public virtual void UpdateFitness(float value, string keyValue) {
        this.Value += value;

        if (value != 0) {
            // Update individual values
            if (IndividualValues.ContainsKey(keyValue)) {
                IndividualValues[keyValue] += value;
            }
            else {
                IndividualValues.Add(keyValue, value);
            }
        }
    }
}

public class PopFitness {

    public FitnessIndividual[] FitnessIndividuals { get; set; }

    public PopFitness(int popSize) {
        FitnessIndividuals = new FitnessIndividual[popSize];
        for (int i = 0; i < FitnessIndividuals.Length; i++) {
            FitnessIndividuals[i] = new FitnessIndividual(i);
        }
    }
}

public class FitnessIndividual {
    [JsonIgnore] public int IndividualID { get; set; }
    [JsonIgnore] public AgentFitness AgentFitness { get; set; } // Used to store fitness from a single game scenario (which is currently running)

    public float FinalFitness { get; set; } // Used to store final fitness (sum of all fitnesses from different game scenarios)
    public float FinalFitnessStats { get; set; } // Used to store final fitness calculated statistic (mean, std deviation, min, max,...)
    public Dictionary<string, AgentFitness> Fitnesses { get; set;} // Used to store fitnesses from different game scenarios

    public FitnessIndividual() {
        IndividualID = -1;
        Fitnesses = new Dictionary<string, AgentFitness>();
        AgentFitness = new AgentFitness();
    }

    public FitnessIndividual(int individualId) { 
        IndividualID = individualId;
        Fitnesses = new Dictionary<string, AgentFitness>();
        AgentFitness = new AgentFitness();
    }

    public float SumFitness() {
        float sum = 0f;
        foreach (KeyValuePair<string, AgentFitness> fitness in Fitnesses) {
            sum += fitness.Value.GetFitness();
        }
        return sum;
    }

    public float MinFitness() {
        if(Fitnesses.Count == 0) {
            throw new Exception("Fitnesses list is empty");
        }

        AgentFitness minFitness = null;
        foreach (KeyValuePair<string, AgentFitness> fitness in Fitnesses) {
            if (minFitness == null || fitness.Value.GetFitness() < minFitness.GetFitness()) {
                minFitness = fitness.Value;
            }
        }

        return minFitness.GetFitness();
    }

    public float MaxFitness() {
        if(Fitnesses.Count == 0) {
            throw new Exception("Fitnesses list is empty");
        }

        AgentFitness maxFitness = null;
        foreach (KeyValuePair<string, AgentFitness> fitness in Fitnesses) {
            if (maxFitness == null || fitness.Value.GetFitness() > maxFitness.GetFitness()) {
                maxFitness = fitness.Value;
            }
        }
        return maxFitness.GetFitness();
    }

    public float MeanFitness() {
        if(Fitnesses.Count == 0) {
            throw new Exception("Fitnesses list is empty");
        }

        float sum = 0f;
        foreach (KeyValuePair<string, AgentFitness> fitness in Fitnesses) {
            sum += fitness.Value.GetFitness();
        }

        return sum / Fitnesses.Count;
    }

    public float StdDeviationFitness() {
        if(Fitnesses.Count == 0) {
            throw new Exception("Fitnesses list is empty");
        }

        float mean = MeanFitness();
        float sum = 0f;
        foreach (KeyValuePair<string, AgentFitness> fitness in Fitnesses) {
            sum += (fitness.Value.GetFitness() - mean) * (fitness.Value.GetFitness() - mean);
        }

        return (float)Math.Sqrt(sum / Fitnesses.Count);
    }
}*/