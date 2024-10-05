using System.Collections.Generic;
using System.Linq;

public class AntColonyFitness : Fitness
{

    public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
        { "AgentBringFood", 1},
    };

    public static string[] Keys = FitnessValues.Keys.ToArray();

    public enum FitnessKeys
    {
        AgentBringFood,
    }
}