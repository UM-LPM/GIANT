using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AntColonyFitness : Fitness
{

    public static Dictionary<string, int> FitnessValues = new Dictionary<string, int> {
        { "AgentBringFood", 1 },
    };

    public static string[] Keys = FitnessValues.Keys.ToArray();

    public enum FitnessKeys
    {
        AgentBringFood,
    }
}