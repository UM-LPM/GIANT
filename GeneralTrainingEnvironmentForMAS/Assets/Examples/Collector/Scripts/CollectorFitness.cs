using System.Collections.Generic;
using System.Linq;

public class CollectorFitness : Fitness {

    public static Dictionary<string, int> FitnessValues = new Dictionary<string, int> {
        { "AgentPickedTarget", -10 },
        { "AgentMovedBonus", -1 },
        { "AgentNearWall", 1 },
        { "AgentNearTarget", 0 }, //-1 },
    };

    public static string[] Keys = FitnessValues.Keys.ToArray();

    public enum FitnessKeys {
        AgentPickedTarget,
        AgentMovedBonus,
        AgentNearWall,
        AgentNearTarget
    }
}

