using System.Collections.Generic;
using System.Linq;

namespace Problems.Collector
{
    public class CollectorFitness
    {
        public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
        { "TimePenalty", -25f },
        { "SectorExploration", -50f },
        { "TargetsAcquired", -500f }
    };

        public static string[] Keys = FitnessValues.Keys.ToArray();

        public enum FitnessKeys
        {
            TimePenalty,
            SectorExploration,
            TargetsAcquired
        }
    }
}