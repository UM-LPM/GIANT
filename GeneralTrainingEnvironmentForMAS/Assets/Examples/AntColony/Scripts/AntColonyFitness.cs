using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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