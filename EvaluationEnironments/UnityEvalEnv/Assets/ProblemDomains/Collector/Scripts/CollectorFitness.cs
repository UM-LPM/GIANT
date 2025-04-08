using System.Collections.Generic;
using System.Linq;

namespace Problems.Collector
{
    public class CollectorFitness
    {
        public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
            { "SectorExploration", -30f },
            { "TargetsAcquired", -500f }
    };

        public static string[] Keys = FitnessValues.Keys.ToArray();

        public enum FitnessKeys
        {
            SectorExploration,
            TargetsAcquired,
        }
    }
}