using System.Collections.Generic;
using System.Linq;

public class AntColonyFitness : Fitness
{

    public static Dictionary<string, int> FitnessValues = new Dictionary<string, int> {
        { "AgentMove", 1 },
        { "AgentKill", 5 },
        { "AgentObjectGathered", 10 },
        { "AgentPheromoneRelease", 4 },
        { "AgentPheromoneReinforce", 4 },
        { "AgentHitByThreat", -5 },

    };


    public static string[] Keys = FitnessValues.Keys.ToArray();

    public enum FitnessKeys
    {
        AgentMove,
        AgentHitByThreat,
        AgentKill,
        AgentObjectGathered,
        AgentPheromoneRelease,
        AgentPheromoneReinforce
    }
}