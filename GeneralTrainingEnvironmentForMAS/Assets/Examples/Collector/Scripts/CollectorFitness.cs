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
        { "AgentPickedTarget", -50f },
        { "AgentExploredSector", -3f },
        { "AgentReExploredSector", -1f },
        { "AgentNearTarget", -2f },
        { "AgentSpottedTarget", -2f },
        { "AgentsBtContainsMainObject", -5f },
        { "AgentTouchedStaticObject", 5f },
        { "AgentBTNodePenalty", 1f },
        { "AgentNotMoved", 0f },
        { "TimePassedPenalty", 0f },
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
