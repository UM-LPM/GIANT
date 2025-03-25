using System.Collections.Generic;
using System.Linq;

namespace Problems.Collector
{
    public class DummyFitness
    {
        public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
        { "AgentPickedTarget", -50f },
        { "AgentExploredSector", -3f },
        { "AgentReExploredSector", -1f }
    };

        public static string[] Keys = FitnessValues.Keys.ToArray();

        public enum FitnessKeys
        {
            AgentPickedTarget,
            AgentExploredSector,
            AgentReExploredSector
        }
    }
}