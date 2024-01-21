using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Fitness {
    protected float Value { get; set; }

    public Fitness() {
        this.Value = 0f;
    }

    public Fitness(float startValue) {
        this.Value = startValue;
    }

    public float GetFitness() {
        return Value;
    }

    public void SetFitness(float value) {
        this.Value = value;
    }

    public virtual void UpdateFitness(float value) {
        this.Value += value;
    }

}

public class PopFitness {
    public float[] FinalFitnesses { get; set; }
    public List<float>[] Fitnesses { get; set; }

    public PopFitness(int popSize) {
        FinalFitnesses = new float[popSize]; // Used to store final fitness (mean, std deviation, min, max,...)
        Fitnesses = new List<float>[popSize]; // Used to store all fitnesses from different game scenarios
        for (int i = 0; i < Fitnesses.Length; i++) {
            Fitnesses[i] = new List<float>();
        }
    }
}

public class GroupFitness {
    public FitnessIndividual[] FitnessIndividuals { get; set; }

    public GroupFitness(int groupSize) {
        this.FitnessIndividuals = new FitnessIndividual[groupSize];
    }
}

public class FitnessIndividual {
    public int IndividualId { get; set; }
    public Fitness Fitness { get; set; }

    public FitnessIndividual() {
        IndividualId = -1;
    }

    public FitnessIndividual(int individualId) { 
        IndividualId = individualId;
        Fitness = new Fitness();
    }

    public FitnessIndividual(float fitness) {
        IndividualId = -1;
        Fitness = new Fitness(fitness);
    }

    public FitnessIndividual(int individualId, float fitness) {
        IndividualId = individualId;
        Fitness = new Fitness(fitness);
    }
}