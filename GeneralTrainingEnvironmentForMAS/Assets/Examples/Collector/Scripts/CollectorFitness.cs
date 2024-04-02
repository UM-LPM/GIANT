using System.Collections.Generic;
using System.Linq;

public class CollectorFitness : Fitness {

    public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
        { "AgentPickedTarget", -5f },
        { "AgentMovedBonus", -0.005f },
        { "AgentNearWall", 0f }, //0.005f },
        { "AgentNearTarget", 0f }, //-1 },
        { "TimePassedPenalty", 0f }, //0.001f },
    };

    public static string[] Keys = FitnessValues.Keys.ToArray();

    public enum FitnessKeys {
        AgentPickedTarget,
        AgentMovedBonus,
        AgentNearWall,
        AgentNearTarget,
        TimePassedPenalty
    }
}

