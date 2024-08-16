using System.Collections.Generic;
using System.Linq;

namespace Collector
{
    /// <summary>
    /// Fitness definition for the Collector problem
    /// </summary>
    public class CollectorFitness : Fitness
    {
        public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
        { "AgentPickedTarget", -5f }, // Implemented
        { "AgentExploredSector", -0.03f }, // Implemented
        { "AgentReExploredSector", -0.002f }, // Implemented
        { "AgentNearTarget", -0.2f }, // Implemented
        { "AgentSpottedTarget", -0.02f }, // Implemented
        { "AgentsBtContainsMainObject", -0.2f }, // Implemented
        { "AgentTouchedStaticObject", 0.1f }, // Implemented
        { "AgentBTNodePenalty", 0.01f }, // Implemented


        { "AgentNotMoved", 0f }, //0.5f },
        //{ "AgentTouchedStaticObject", 0f }, //0.05f },
        //{ "AgentNearTarget", 0f }, //-1 },
        { "TimePassedPenalty", 0f }, //0.001f },
    };

        public static string[] Keys = FitnessValues.Keys.ToArray();

        public enum FitnessKeys
        {
            AgentPickedTarget,
            AgentExploredSector,
            AgentReExploredSector,
            AgentNearTarget,
            AgentSpottedTarget,
            AgentsBtContainsMainObject,
            AgentTouchedStaticObject,
            AgentBTNodePenalty,

            AgentNotMoved,
            //AgentNearTarget,
            TimePassedPenalty
        }
    }
}
